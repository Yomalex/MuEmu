using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.GensSystem
{
    public interface IGensMessage { }
    public class GensMessageFactory : MessageFactory<GensOpCode, IGensMessage>
    {
        public GensMessageFactory()
        {
            Register<SGensSendInfoS9>(GensOpCode.SendGensInfo);
        }
    }
}
