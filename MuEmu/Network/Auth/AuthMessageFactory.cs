using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Auth
{
    public interface IAuthMessage
    { }

    public class AuthMessageFactory : MessageFactory<CSOpCode, IAuthMessage>
    {
        public AuthMessageFactory()
        {
            // C2S
            if (Program.Season12)
            {
                Register<CIDAndPassS12>(CSOpCode.Login);
            }
            else
            {
                Register<CIDAndPass>(CSOpCode.Login);
            }
            
            Register<CCharacterList>(CSOpCode.CharacterList);
            Register<CCharacterCreate>(CSOpCode.CharacterCreate);
            Register<CCharacterDelete>(CSOpCode.CharacterDelete);
            Register<CCharacterMapJoin>(CSOpCode.JoinMap);
            Register<CCharacterMapJoin2>(CSOpCode.JoinMap2);

            // S2C
            Register<SJoinResult>(CSOpCode.JoinResult);
            Register<SLoginResult>(CSOpCode.Login);
            Register<SCharacterList>(CSOpCode.CharacterList);
            Register<SCharacterListS12>(CSOpCode.CharacterList);
            Register<SCharacterCreate>(CSOpCode.CharacterCreate);
            Register<SCharacterDelete>(CSOpCode.CharacterDelete);
            Register<SCharacterMapJoin>(CSOpCode.JoinMap);
            Register<SCharacterMapJoin2>(CSOpCode.JoinMap2);
        }
    }
}
