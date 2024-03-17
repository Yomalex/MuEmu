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
            Register<CIDAndPassS12>(CSOpCode.Login);
            Register<CCharacterList>(CSOpCode.CharacterList);
            Register<CCharacterMapJoin>(CSOpCode.JoinMap);
            Register<CCharacterMapJoin2>(CSOpCode.JoinMap2);
            Register<CCharacterCreate>(CSOpCode.CharacterCreate);
            Register<CCharacterDelete>(CSOpCode.CharacterDelete);
            Register<CServerMove>(CSOpCode.ServerMoveAuth);
            Register<CServerList>(CSOpCode.ChannelList);

            // S2C
            Register<SServerList>(CSOpCode.ChannelList);
            Register<SEnableCreation>(CSOpCode.EnableCreate);

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

            VersionSelector.Register<SCharacterList>(ServerSeason.Season6Kor, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS9>(ServerSeason.Season9Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS12>(ServerSeason.Season12Eng, CSOpCode.CharacterList);
            VersionSelector.Register<SCharacterListS16Kor>(ServerSeason.Season16Kor, CSOpCode.CharacterList);
            Register<SCharacterList>(CSOpCode.CharacterList);
            Register<SCharacterListS9>(CSOpCode.CharacterList);
            Register<SCharacterListS12>(CSOpCode.CharacterList);
            Register<SCharacterListS16Kor>(CSOpCode.CharacterList);

            Register<SLoginResult>(CSOpCode.Login);
            Register<SCharacterMapJoin>(CSOpCode.JoinMap);
            Register<SCharacterMapJoin2>(CSOpCode.JoinMap2);


            switch (Season)
            {
                default:
                    ChangeType<CIDAndPass>(CSOpCode.Login, typeof(CIDAndPassS12));
                    break;
                case ServerSeason.Season17Kor75:
                    ChangeOPCode<CCharacterCreate>((CSOpCode)0x2652);
                    ChangeOPCode<CCharacterDelete>((CSOpCode)0x0652);
                    ChangeOPCode<SCharacterDelete>((CSOpCode)0x0782);
                    ChangeOPCode<CServerList>((CSOpCode)0x58F7);
                    ChangeOPCode<SServerList>((CSOpCode)0x3151);

                    ChangeOPCode<CIDAndPassS12>((CSOpCode)0x02F3);
                    ChangeOPCode<CCharacterList>((CSOpCode)0x6052);
                    ChangeOPCode<CCharacterMapJoin>((CSOpCode)0x0152);
                    ChangeOPCode<CCharacterMapJoin2>((CSOpCode)0x0052);
                    ChangeOPCode<SJoinResultS16Kor>((CSOpCode)0x003A);
                    ChangeOPCode<SLoginResult>((CSOpCode)0xFD3A);
                    ChangeOPCode<SCharacterListS16Kor>((CSOpCode)0x0482);
                    ChangeOPCode<SCharacterMapJoin>((CSOpCode)0x0582);
                    ChangeOPCode<SCharacterMapJoin2>((CSOpCode)0x1482);

                    ChangeOPCode<SEnableCreation>((CSOpCode)CSOpCode.EnableCreate);
                    //MiningSystemUnk 0x11C4
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
                    //Register<SResets>(CSOpCode.Resets);
                    break;
                case ServerSeason.Season9Eng:
                    //Register<CIDAndPassS12>(CSOpCode.Login);
                    //Register<SResets>(CSOpCode.Resets);
                    //Register<SResetCharList>(CSOpCode.ResetList);
                    break;
            }
        }
    }
}
