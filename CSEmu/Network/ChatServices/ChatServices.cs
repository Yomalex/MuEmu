using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace CSEmu.Network.ChatServices
{
    public class ChatServices : MessageHandler
    {
        [MessageHandler(typeof(CChatAuth))]
        public void CChatAuth(CSSession session, CChatAuth message)
        {

        }
    }
}
