using CSEmu.Network.Data;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Handlers;
using WebZen.Util;

namespace CSEmu.Network.Services
{
    public class MainServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MainServices));

        [MessageHandler(typeof(CServerList))]
        public void ServerListHandler(CSSession session, CServerList message)
        {
            var servers = ServerManager.Instance.Servers.Where(x => x.Visible).Select(x => new ServerDto { Index = x.Index, Load = x.Load, Padding = 0x77 }).ToArray();
            //Logger.Information("Sending Server list {0} servers", servers.Length);
            session.SendAsync(new SServerList(servers));
        }

        [MessageHandler(typeof(CServerInfo))]
        public void ServerInfoHandler(CSSession session, CServerInfo message)
        {
            var server = ServerManager.Instance.GetServer(message.Index);
            Logger.Information("Forwarding to {0}:{1}", server.Address, server.Port);
            session.SendAsync(new SServerInfo
            {
                Address = server.Address,
                Port = server.Port
            });
        }

        [MessageHandler(typeof(CRegistryReq))]
        public void RegistryHandler(CSSession session, CRegistryReq message)
        {
            ServerManager.Instance.Register(session, (byte)message.Index, message.Address, message.Port, message.Show != 0, message.Token);
        }

        [MessageHandler(typeof(CKeepAlive))]
        public void KeepAliveHandler(CSSession session, CKeepAlive message)
        {
            ServerManager.Instance.Keep((byte)message.Index, message.Load, message.Token);
        }

        [MessageHandler(typeof(SCAdd))]
        public void SCAdd(CSSession session, SCAdd message)
        {
            Logger.Information("FriendSystem: [" + message.Server + "] " + message.btName.MakeString() + " ADD");
            Program.Clients.AddClient(message);
        }

        [MessageHandler(typeof(SCRem))]
        public void SCRem(CSSession session, SCRem message)
        {
            Program.Clients.RemClient(message);
        }
    }
}
