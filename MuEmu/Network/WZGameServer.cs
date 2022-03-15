using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebZen.Network;
using WebZen.Handlers;
using WebZen.Util;
using System.Threading.Tasks;
using System.Linq;
using MuEmu.Entity;
using Serilog;
using Serilog.Core;
using MuEmu.Monsters;
using MU.Resources;
using MU.Network.Auth;
using MuEmu.Network.ConnectServer;

namespace MuEmu.Network
{
    internal class WZServerEventArgs : EventArgs
    {
        public GSSession session { get; set; }
    }
    internal class WZGameServer : WZServer
    {
        public string ClientVersion { get; set; }
        public string ClientSerial { get; set; }
        public IEnumerable<GSSession> Clients => _clients.Values.Select(x => x as GSSession);


        /// <summary>
        /// Sender as Server, WZServerEventArgs with GSSession
        /// </summary>
        public event EventHandler<WZServerEventArgs> Connect;
        public event EventHandler<WZServerEventArgs> Disconnect;

        public WZGameServer(IPEndPoint address, MessageHandler[] handler, MessageFactory[] factories, bool useRijndael)
        {
            Initialize(address, handler, new GSSessionFactory(), factories, useRijndael);
        }

        protected override void OnConnect(WZClient session)
        {
            var Session = session as GSSession;

            Session.Player = new Player(Session);

            Session.SendAsync(new SJoinResult(1, Session.ID, ClientVersion)).Wait();

            Connect?.Invoke(this, new WZServerEventArgs { session = Session });
        }

        public override void OnDisconnect(WZClient session)
        {
            var Session = session as GSSession;
            if (Session.Player != null)
            {
                if (Session.Player.Character != null)
                {
                    Program.client
                        .SendAsync(new SCRem
                        {
                            Server = (byte)Program.ServerCode,
                            List = new CliRemDto[] {
                                new CliRemDto {
                                    btName = Session.Player.Character.Name.GetBytes()
                                }
                            }
                        });
                }
                Session.Player.Status = LoginStatus.NotLogged;

                using (var db = new GameContext())
                {
                    if (Session.Player.Account != null)
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
                Session.Player.Account = null;
            }
            //db.SaveChanges();

            Session.Player = null;
            base.OnDisconnect(session);
            Disconnect?.Invoke(this, new WZServerEventArgs { session = Session });
        }

        public async Task SendAll(object message)
        {
            foreach(var cl in _clients.Values.Select(x => x as GSSession).Where(x => x.Player.Status == LoginStatus.Playing))
                await cl.SendAsync(message);
        }
    }
}
