using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Auth
{
    public interface IAuthMessage
    { }

    public class AuthMessageFactory : MessageFactory<AuthOpCode, IAuthMessage>
    {
        public AuthMessageFactory()
        {
            // C2S
            Register<CIDAndPass>(AuthOpCode.Login);

            // S2C
            Register<SJoinResult>(AuthOpCode.JoinResult);
        }
    }
}
