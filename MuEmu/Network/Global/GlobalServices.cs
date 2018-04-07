using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network.Global
{
    public class GlobalServices : MessageHandler
    {
        [MessageHandler(typeof(CLiveClient))]
        public void LiveClient(GSSession session, CLiveClient message)
        { }
    }
}
