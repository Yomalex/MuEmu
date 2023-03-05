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
            Register<CIDAndPassS12>(CSOpCode.Login);
            Register<SLoginResult>(CSOpCode.Login);
            Register<CCharacterList>(CSOpCode.CharacterList);

            VersionSelector.Register<SCharacterList>(ServerSeason.Season6Kor, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS9>(ServerSeason.Season9Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS12>(ServerSeason.Season12Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS16Kor>(ServerSeason.Season16Kor, CSOpCode.CharacterList);
            Register<SCharacterList>(CSOpCode.CharacterList);
            Register<SCharacterListS9>(CSOpCode.CharacterList);
            Register<SCharacterListS12>(CSOpCode.CharacterList);
            Register<SCharacterListS16Kor>(CSOpCode.CharacterList);
            Register<CCharacterMapJoin>(CSOpCode.JoinMap);
            Register<CCharacterMapJoin2>(CSOpCode.JoinMap2);
            Register<SCharacterMapJoin>(CSOpCode.JoinMap);
            Register<SCharacterMapJoin2>(CSOpCode.JoinMap2);
            switch (Season)
            {
                default:
                    ChangeType<CIDAndPass>(CSOpCode.Login, typeof(CIDAndPassS12));
                    break;
                case ServerSeason.Season17Kor:
                    ChangeOPCode<CIDAndPassS12>(CSOpCode.LoginS17Kor);
                    ChangeOPCode<SLoginResult>(CSOpCode.LoginS17KorResp);
                    ChangeOPCode<CCharacterList>(CSOpCode.CharacterListS17Kor);
                    ChangeOPCode<SCharacterListS16Kor>(CSOpCode.CharacterListS17KorResp);
                    ChangeOPCode<CCharacterMapJoin>(CSOpCode.JoinMapS17Kor);
                    ChangeOPCode<SCharacterMapJoin>(CSOpCode.JoinMapS17KorResp);
                    ChangeOPCode<CCharacterMapJoin2>(CSOpCode.JoinMap2S17Kor);
                    ChangeOPCode<SCharacterMapJoin2>(CSOpCode.JoinMap2S17KorResp);
                    break;
                case ServerSeason.Season16Kor:
                case ServerSeason.Season12Eng:
                    Register<SResets>(CSOpCode.Resets);
                    break;
                case ServerSeason.Season9Eng:
                    //Register<CIDAndPassS12>(CSOpCode.Login);
                    Register<SResets>(CSOpCode.Resets);
                    Register<SResetCharList>(CSOpCode.ResetList);
                    break;
            }
            //Register<SLoginResultS17>(CSOpCode.LoginS17KorResp);

            Register<CServerList>(CSOpCode.ChannelList);
            Register<SServerList>(CSOpCode.ChannelList);
            Register<SEnableCreation>(CSOpCode.EnableCreate);
            Register<CCharacterCreate>(CSOpCode.CharacterCreate);
            Register<CCharacterDelete>(CSOpCode.CharacterDelete);
            Register<CServerMove>(CSOpCode.ServerMoveAuth);

            // S2C
            Register<SJoinResult>(CSOpCode.JoinResult);
            Register<SJoinResultS16Kor>(CSOpCode.JoinResult);
            VersionSelector.Register<SJoinResult>(ServerSeason.Season6Kor, CSOpCode.JoinResult);
            VersionSelector.Register<SJoinResultS16Kor>(ServerSeason.Season16Kor, CSOpCode.JoinResult);
            Register<SCharacterCreate>(CSOpCode.CharacterCreate);
            Register<SCharacterDelete>(CSOpCode.CharacterDelete);
            Register<SCharacterMapJoin2S12Eng>(CSOpCode.JoinMap2);
            Register<SCharacterMapJoin2S16Kor>(CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterMapJoin2>(ServerSeason.Season6Kor, CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterMapJoin2S12Eng>(ServerSeason.Season12Eng, CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterMapJoin2S16Kor>(ServerSeason.Season16Kor, CSOpCode.JoinMap2);
            Register<SServerMove>(CSOpCode.ServerMove);
        }
    }
}
