using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace CSEmu.Network.ChatServices
{
    public interface IChatMessage
    { }
    internal class ChatMessageFactory : MessageFactory<ChatOpCode, IChatMessage>
    {
        public ChatMessageFactory()
        {
            Register<CChatAuth>(ChatOpCode.Auth);
        }
    }
}
