using MU.Network.Auth;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network.ConnectServer
{
    public class CSServices : MessageHandler
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CSServices));
        [MessageHandler(typeof(SCAdd))]
        public void SCAdd(CSClient session, SCAdd message)
        {
            foreach(var c in Program.server.Clients.Where(x => x.Player != null && x.Player.Status == MU.Resources.LoginStatus.Playing))
            {
                var name = message.btName.MakeString();
                c.Player.Character.Friends.ConnectFriend(name, message.Server);
                var m = c.Player.Character.Guild?.Find(name)??null;
                if(m != null && message.Server != Program.ServerCode)
                {
                    m.Server = message.Server;
                }
            }
        }

        [MessageHandler(typeof(SCRem))]
        public void SCRem(CSClient session, SCRem message)
        {
            foreach (var c in Program.server.Clients.Where(x => x.Player != null && x.Player.Status == MU.Resources.LoginStatus.Playing))
            {
                foreach(var p in message.List)
                {
                    var name = p.btName.MakeString();
                    c.Player.Character?.Friends.DisconnectFriend(name);
                    var m = c.Player.Character.Guild?.Find(name)??null;
                    if (m != null && message.Server != Program.ServerCode)
                    {
                        m.Server = 0xff;
                    }
                }                
            }
        }

        [MessageHandler(typeof(CSServerList))]
        public void SServerList(CSClient session, CSServerList message)
        {
            Program.ServerList = message.List.Take((message.CountH<<8)+message.CountL).Select(x => new MU.Network.Auth.ServerDto {
                type = x.Type,
                data1 = 32,
                data2 = 0,
                gold = 0,
                server = x.Index,
            });
        }
    }
}
