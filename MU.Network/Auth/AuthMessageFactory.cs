using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.Auth
{
    public interface IAuthMessage
    { }

    public class AuthMessageFactory : MessageFactory<CSOpCode, IAuthMessage>
    {
        public AuthMessageFactory(int Season)
        {
            switch(Season)
            {
                default:
                    Register<CIDAndPass>(CSOpCode.Login);
                    break;
                case 12:
                    Register<CIDAndPassS12>(CSOpCode.Login);
                    Register<SResets>(CSOpCode.Resets);
                    break;
                case 9:
                    Register<CIDAndPassS12>(CSOpCode.Login);
                    Register<SResets>(CSOpCode.Resets);
                    Register<SResetCharList>(CSOpCode.ResetList);
                    break;
            }

            Register<SEnableCreation>(CSOpCode.EnableCreate);
            Register<CCharacterList>(CSOpCode.CharacterList);
            Register<CCharacterCreate>(CSOpCode.CharacterCreate);
            Register<CCharacterDelete>(CSOpCode.CharacterDelete);
            Register<CCharacterMapJoin>(CSOpCode.JoinMap);
            Register<CCharacterMapJoin2>(CSOpCode.JoinMap2);
            Register<CServerMove>(CSOpCode.ServerMoveAuth);

            // S2C
            Register<SJoinResult>(CSOpCode.JoinResult);
            Register<SLoginResult>(CSOpCode.Login);
            Register<SCharacterList>(CSOpCode.CharacterList);
            Register<SCharacterListS9>(CSOpCode.CharacterList);
            Register<SCharacterListS12>(CSOpCode.CharacterList);
            Register<SCharacterCreate>(CSOpCode.CharacterCreate);
            Register<SCharacterDelete>(CSOpCode.CharacterDelete);
            Register<SCharacterMapJoin>(CSOpCode.JoinMap);
            Register<SCharacterMapJoin2>(CSOpCode.JoinMap2);
            Register<SServerMove>(CSOpCode.ServerMove);
        }
    }
}
