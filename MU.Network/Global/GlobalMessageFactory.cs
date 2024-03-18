using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.Global
{
    public interface IGlobalMessage
    { }

    public class GlobalMessageFactory : MessageFactory<GlobalOpCode, IGlobalMessage>
    {
        public GlobalMessageFactory(ServerSeason Season)
        {
            // C2S
            Register<CLiveClient>(GlobalOpCode.LiveClient);
             
        }        
    }
}
