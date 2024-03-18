using MU.Resources;
using MySqlX.XDevAPI;
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
            // C2S
            Register<CIDAndPassS12>(CSOpCode.LoginRes);
            Register<SLoginResult>(CSOpCode.Login);

            Register<CCharacterList>(CSOpCode.CharacterListRes);
            Register<SCharacterList>(CSOpCode.CharacterList);
            Register<SCharacterListS9>(CSOpCode.CharacterList);
            Register<SCharacterListS12>(CSOpCode.CharacterList);
            Register<SCharacterListS16Kor>(CSOpCode.CharacterList);

            Register<CCharacterMapJoin>(CSOpCode.JoinMapRes); 
            Register<SCharacterMapJoin>(CSOpCode.JoinMap);

            Register<CCharacterMapJoin2>(CSOpCode.JoinMap2Res); 
            Register<SCharacterMapJoin2>(CSOpCode.JoinMap2); 
            Register<SCharacterMapJoin2S12Eng>(CSOpCode.JoinMap2);
            Register<SCharacterMapJoin2S16Kor>(CSOpCode.JoinMap2);

            Register<CCharacterCreate>(CSOpCode.CharacterCreateRes);
            Register<SCharacterCreate>(CSOpCode.CharacterCreate);

            Register<CCharacterDelete>(CSOpCode.CharacterDeleteRes);
            Register<SCharacterDelete>(CSOpCode.CharacterDelete);

            Register<CServerMove>(CSOpCode.ServerMoveAuth);
            Register<SServerMove>(CSOpCode.ServerMove);

            Register<CServerList>(CSOpCode.ChannelListRes); 
            Register<SServerList>((CSOpCode)CSOpCode.ChannelList);
            // S2C
            Register<SJoinResult>(CSOpCode.JoinResult);
            Register<SJoinResultS16Kor>((CSOpCode)CSOpCode.JoinResult); 
            Register<SEnableCreation>((CSOpCode)CSOpCode.EnableCreate);


            VersionSelector.Register<SJoinResult>(ServerSeason.Season6Kor, CSOpCode.JoinResult);
            VersionSelector.Register<SJoinResultS16Kor>(ServerSeason.Season16Kor, CSOpCode.JoinResult);
            VersionSelector.Register<SCharacterMapJoin2>(ServerSeason.Season6Kor, CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterMapJoin2S12Eng>(ServerSeason.Season12Eng, CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterMapJoin2S16Kor>(ServerSeason.Season16Kor, CSOpCode.JoinMap2);
            VersionSelector.Register<SCharacterList>(ServerSeason.Season6Kor, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS9>(ServerSeason.Season9Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS12>(ServerSeason.Season12Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS16Kor>(ServerSeason.Season16Kor, CSOpCode.CharacterList);
             
        }
    }
}