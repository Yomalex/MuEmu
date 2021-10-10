using MU.Resources;
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
        public AuthMessageFactory(ServerSeason Season)
        {
            switch(Season)
            {
                default:
                    Register<CIDAndPass>(CSOpCode.Login);
                    break;
                case ServerSeason.Season12Eng:
                    Register<CIDAndPassS12>(CSOpCode.Login);
                    Register<SResets>(CSOpCode.Resets);
                    break;
                case ServerSeason.Season9Eng:
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

            VersionSelector.Register<SCharacterList>(ServerSeason.Season6Kor, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS9>(ServerSeason.Season9Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS12>(ServerSeason.Season12Eng, CSOpCode.CharacterList);

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
