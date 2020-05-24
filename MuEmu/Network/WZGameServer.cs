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
        public string ClientVersion { get; set; }
        public string ClientSerial { get; set; }
        public IEnumerable<GSSession> Clients => _clients.Values.Select(x => x as GSSession);

        public WZGameServer(IPEndPoint address, MessageHandler[] handler, MessageFactory[] factories, bool useRijndael)
        {
            Initialize(address, handler, new GSSessionFactory(), factories, useRijndael);
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
                    if (Session.Player.Status == LoginStatus.Playing && Session.Player.Character != null)
                    {
                        var @char = Session.Player.Character;
                        @char.Map.DelPlayer(Session.Player.Character);
                        @char.Party?.Remove(Session.Player);
                        @char.Duel?.Leave(@char.Player);
                        @char.Party = null;
                        Game.GameServices.CCloseWindow(Session);
                        foreach(var m in @char.MonstersVP.Select(x => Monsters.MonstersMng.Instance.GetMonster(x)).Where(x => x.Target == Session.Player))
                        {
                            m.Target = null;
                        }
                    }

                    Session.Player.Status = LoginStatus.NotLogged;
                    Logger.ForAccount(Session).Information("Saving...");
                    Session.Player.Save(db).Wait();
                    Logger.ForAccount(Session).Information("Saved!");
                }
                if(Session.Player.Account != null)
                {
                    var acc = (from a in db.Accounts
                              where a.AccountId == Session.Player.Account.ID
                              select a).First();

                    acc.IsConnected = false;

                    db.Accounts.Update(acc);
                    Logger.ForAccount(Session).Information("Disconnecting...");
                }
                db.SaveChanges();
            }
            base.OnDisconnect(session);
        }

        public async Task SendAll(object message)
        {
            foreach(var cl in _clients.Values.Select(x => x as GSSession).Where(x => x.Player.Status == LoginStatus.Playing))
                await cl.SendAsync(message);
        }
    }
}
