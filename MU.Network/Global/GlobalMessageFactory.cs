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

            // S2C

            switch (Season)
            {
                case ServerSeason.Season17Kor:
                    ChangeOPCode<CLiveClient>(GlobalOpCode.LiveClientS17K75);
                    break;
                default:
                    break;
            }
        }        
    }
}
