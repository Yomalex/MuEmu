using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebZen.Network;
using WebZen.Handlers;
using MuEmu.Network.Auth;
using WebZen.Util;

namespace MuEmu.Network
{
    internal class WZGameServer : WZServer
    {
        public string ClientVersion { get; set; }
        public WZGameServer(IPEndPoint address, MessageHandler[] handler, MessageFactory[] factories)
        {
            Initialize(address, handler, new GSSessionFactory(), factories);
            ClientVersion = "10635";
        }

        protected override void OnConnect(WZClient session)
        {
            var Session = session as GSSession;

            Session.Player = new Player(Session);

            Session.SendAsync(new SJoinResult(1, Session.ID, ClientVersion));
        }
    }
}
