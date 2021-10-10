using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Serilog.Core;
using Serilog;
using CSEmu.Network;

namespace CSEmu
{
    public class ServerInfo
    {
        public ushort Index { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public ushort Port { get; set; }
        public byte Load { get; set; }
        public DateTime LastPush { get; set; }
        public bool Visible { get; set; }
    }

    public class ServerManager
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ServerManager));
        private Dictionary<ushort, ServerInfo> _servers;
        private Dictionary<CSSession, byte> _GSsessions;
        private string _token;

        public static ServerManager Instance { get; private set; }

        public ServerManager()
        {
            _servers = new Dictionary<ushort, ServerInfo>();
            _GSsessions = new Dictionary<CSSession, byte>();
        }

        public static void Initialize(string token)
        {
            if (Instance != null)
                throw new Exception("Already initialized");

            Instance = new ServerManager();
            Instance._token = token;
        }

        public void Register(CSSession session, byte index, string address, ushort port, bool display, string token, string name)
        {
            if(_token != token)
            {
                Logger.Error("Auth Token invalid");
                return;
            }

            lock (_servers)
            {
                _servers.Add(index, new ServerInfo { Index = index, Address = address, Port = port, LastPush = DateTime.Now, Visible = display, Name = name });
                _GSsessions.Add(session, index);
            }

            Program.Clients.AddServer(index);

            Logger.Information("New server found [{0}]{4} {1}:{2} {3}", index, address, port, display ? "SHOW":"HIDE", name);
        }

        public void Unregister(byte index)
        {
            lock (_servers)
            {
                _servers.Remove(index);
                var session = _GSsessions.Where(x => x.Value == index).Select(x => x.Key).FirstOrDefault();
                _GSsessions.Remove(session);
            }

            Program.Clients.RemServer(index);
            Logger.Information("Server {0} dead", index);
        }

        public void Unregister(CSSession session)
        {
            lock (_servers)
            {
                if (_GSsessions.ContainsKey(session))
                    Unregister(_GSsessions[session]);
            }
        }

        public void Keep(byte index, byte load, string token)
        {
            if (_token != token)
            {
                Logger.Error("Auth Token invalid");
                return;
            }

            lock (_servers)
            {
                _servers[index].LastPush = DateTime.Now;
                _servers[index].Load = load;
            }
        }

        public List<ServerInfo> Servers => GetServerList();

        public ServerInfo GetServer(ushort index)
        {
            lock (_servers)
            {
                return _servers[index];
            }
        }

        private List<ServerInfo> GetServerList()
        {
            List<byte> deadServers;
            lock (_servers)
            {
                deadServers = (from s in _servers
                                where DateTime.Now - s.Value.LastPush > TimeSpan.FromSeconds(30) && s.Value.Visible
                                select (byte)s.Key).ToList();
            }

            foreach(var s in deadServers)
            {
                Unregister(s);
            }

            lock (_servers)
            {
                return _servers.Select(x => x.Value).ToList();
            }
        }

        public void BroadCast(object message)
        {
            foreach(var gs in _GSsessions.Keys)
                gs.SendAsync(message);
        }
    }
}
