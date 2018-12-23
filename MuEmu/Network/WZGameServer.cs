using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebZen.Network;
using WebZen.Handlers;
using MuEmu.Network.Auth;
using WebZen.Util;
using System.Threading.Tasks;

namespace MuEmu.Network
{
    internal class WZGameServer : WZServer
    {
        public string ClientVersion { get; set; }
        public string ClientSerial { get; set; }
        public WZGameServer(IPEndPoint address, MessageHandler[] handler, MessageFactory[] factories)
        {
            Initialize(address, handler, new GSSessionFactory(), factories);
        }

        protected override void OnConnect(WZClient session)
        {
            var Session = session as GSSession;

            Session.Player = new Player(Session);

            Session.SendAsync(new SJoinResult(1, Session.ID, ClientVersion));
        }

        public override void OnDisconnect(WZClient session)
        {
            var Session = session as GSSession;
            Session.Player.Status = LoginStatus.NotLogged;
            Session.Player?.Character?.Map.DelPlayer(Session.Player.Character);
            base.OnDisconnect(session);
        }

        public async Task SendAll(object message)
        {
            foreach(var cl in _clients)
            {
                var session = cl.Value as GSSession;
                await session.SendAsync(message);
            }
        }
    }
}
