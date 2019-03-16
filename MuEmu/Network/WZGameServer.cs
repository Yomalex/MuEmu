using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebZen.Network;
using WebZen.Handlers;
using MuEmu.Network.Auth;
using WebZen.Util;
using System.Threading.Tasks;
using System.Linq;
using MuEmu.Entity;

namespace MuEmu.Network
{
    internal class WZGameServer : WZServer
    {
        public string ClientVersion { get; set; }
        public string ClientSerial { get; set; }
        public IEnumerable<GSSession> Clients => _clients.Values.Select(x => x as GSSession);

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
            using (var db = new GameContext())
            {
                if (Session.Player != null)
                {
                    Session.Player.Status = LoginStatus.NotLogged;
                    Session.Player.Character?.Map.DelPlayer(Session.Player.Character);
                    Session.Player.Save(db);
                }
                if(Session.Player.Account != null)
                {
                    var acc = (from a in db.Accounts
                              where a.AccountId == Session.Player.Account.ID
                              select a).First();

                    acc.IsConnected = false;

                    db.Accounts.Update(acc);
                    db.SaveChanges();
                }
            }
            base.OnDisconnect(session);
        }

        public async Task SendAll(object message)
        {
            foreach(var cl in _clients.Values.Select(x => x as GSSession))
                await cl.SendAsync(message);
        }
    }
}
