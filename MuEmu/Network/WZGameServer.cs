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
using Serilog;
using Serilog.Core;

namespace MuEmu.Network
{
    internal class WZGameServer : WZServer
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZGameServer));
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
                    if (Session.Player.Status == LoginStatus.Playing)
                        Session.Player.Character?.Map.DelPlayer(Session.Player.Character);

                    Session.Player.Status = LoginStatus.NotLogged;
                    Logger.ForAccount(Session).Information("Saving...");
                    Session.Player.Save(db);
                    Logger.ForAccount(Session).Information("Saved!");
                }
                if(Session.Player.Account != null)
                {
                    var acc = (from a in db.Accounts
                              where a.AccountId == Session.Player.Account.ID
                              select a).First();

                    acc.IsConnected = false;

                    db.Accounts.Update(acc);
                    db.SaveChanges();
                    Logger.ForAccount(Session).Information("Disconnecting...");
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
