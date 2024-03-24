using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.GensSystem
{
    public interface IGensMessage { }
    public class GensMessageFactory : MessageFactory<GensOpCode, IGensMessage>
    {
        public GensMessageFactory(ServerSeason Season)
        {
            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, true);
            Register<CRequestJoin>(GensOpCode.RequestJoin);
            Register<CRequestLeave>(GensOpCode.RequestLeave);
            Register<CRequestMemberInfo>(GensOpCode.RequestMemberInfo);
            Register<CRequestReward>(GensOpCode.RequestReward);

            if (Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, false);
            Register<SRequestJoin>(GensOpCode.RequestJoin);
            Register<SGensSendInfoS9>(GensOpCode.SendGensInfo);
            Register<SViewPortGens>(GensOpCode.ViewPortGens);
            Register<SRegMember>(GensOpCode.RegMember);
            Register<SGensLeaveResult>(GensOpCode.RemoveMember);
            Register<SGensReward>(GensOpCode.RewardSend);
            //Register<SGensBattleZoneData>(GensOpCode.BattleZoneData);
        }
    }
}
