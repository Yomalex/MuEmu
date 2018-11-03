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
        public ushort Port { get; set; }
        public byte Load { get; set; }
        public DateTime LastPush { get; set; }
    }

    public class ServerManager
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ServerManager));
        private Dictionary<ushort, ServerInfo> _servers;
        private Dictionary<CSSession, ushort> _GSsessions;

        public static ServerManager Instance { get; private set; }

        public ServerManager()
        {
            _servers = new Dictionary<ushort, ServerInfo>();
            _GSsessions = new Dictionary<CSSession, ushort>();
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception("Already initialized");

            Instance = new ServerManager();
        }

        public void Register(CSSession session, ushort index, string address, ushort port)
        {
            lock (_servers)
            {
                _servers.Add(index, new ServerInfo { Index = index, Address = address, Port = port, LastPush = DateTime.Now });
                _GSsessions.Add(session, index);
            }

            Logger.Information("New server found {0}", new { index, address, port });
        }

        public void Unregister(ushort index)
        {
            lock (_servers)
            {
                _servers.Remove(index);
                var session = _GSsessions.Where(x => x.Value == index).Select(x => x.Key).FirstOrDefault();
                _GSsessions.Remove(session);
            }

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

        public void Keep(ushort index, byte load)
        {
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
            List<ushort> deadServers;
            lock (_servers)
            {
                deadServers = (from s in _servers
                                where DateTime.Now - s.Value.LastPush > TimeSpan.FromSeconds(30)
                                select s.Key).ToList();
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
    }
}
