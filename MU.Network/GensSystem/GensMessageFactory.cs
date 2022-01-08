using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.GensSystem
{
    public interface IGensMessage { }
    public class GensMessageFactory : MessageFactory<GensOpCode, IGensMessage>
    {
        public GensMessageFactory()
        {
            Register<CRequestJoin>(GensOpCode.RequestJoin);
            Register<CRequestLeave>(GensOpCode.RequestLeave);
            Register<CRequestMemberInfo>(GensOpCode.RequestMemberInfo);
            Register<CRequestReward>(GensOpCode.RequestReward);
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
