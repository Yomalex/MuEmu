using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Global
{
    public interface IGlobalMessage
    { }

    public class GlobalMessageFactory : MessageFactory<GlobalOpCode, IGlobalMessage>
    {
        public GlobalMessageFactory()
        {
            // C2S
            Register<CLiveClient>(GlobalOpCode.LiveClient);

            // S2C
        }        
    }
}
