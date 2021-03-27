using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
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
            foreach(var c in Program.server.Clients)
            {
                c.Player.Character.Friends.ConnectFriend(message.btName.MakeString(), message.Server);
            }
        }

        [MessageHandler(typeof(SCRem))]
        public void SCRem(CSClient session, SCRem message)
        {
            foreach (var c in Program.server.Clients)
            {
                foreach(var p in message.List)
                {
                    c.Player.Character?.Friends.DisconnectFriend(p.btName.MakeString());
                }                
            }
        }
    }
}
