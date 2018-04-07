using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebZen.Network;
using WebZen.Handlers;
using MuEmu.Network.Auth;

namespace MuEmu.Network
{
    internal class WZGameServer : WZServer
    {
        public WZGameServer(IPEndPoint address, MessageHandler[] handler, MessageFactory[] factories)
        {
            Initialize(address, handler, new GSSessionFactory(), factories);
        }

        protected override void OnConnect(WZClient session)
        {
            var Session = session as GSSession;

            Session.SendAsync(new SJoinResult
            {
                Result = 1,
                NumberH = (byte)(Session.ID / 256),
                NumberL = (byte)(Session.ID % 256),
                ClientVersion = new byte[] { (byte)'1', (byte)'0', (byte)'6', (byte)'3', (byte)'5' }
            });
        }
    }
}
