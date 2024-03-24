using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.Guild
{
    public interface IGuildMessage
    { }

    public class GuildMessageFactory : MessageFactory<GuildOpCode, IGuildMessage>
    {
        public GuildMessageFactory(ServerSeason Season)
        {
            // C2S
            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, true);
            Register<CGuildRequest>(GuildOpCode.GuildRequest);
            Register<CGuildInfoSave>(GuildOpCode.GuildSaveInfo);
            Register<CGuildReqViewport>(GuildOpCode.GuildReqViewport);
            Register<CGuildListAll>(GuildOpCode.GuildListAll);
            Register<CGuildRequestAnswer>(GuildOpCode.GuildResult);
            Register<CGuildSetStatus>(GuildOpCode.GuildSetStatus);
            Register<CGuildRemoveUser>(GuildOpCode.RemoveUser);
            Register<CUnionList>(GuildOpCode.GuildUnionList);
            Register<CRelationShipJoinBreakOff>(GuildOpCode.GuildRelationShip);
            Register<CGuildMatchingListSearch>(GuildOpCode.GuildMatchingListSearch);
            Register<CGuildMatchingList>(GuildOpCode.GuildMatchingList);
            Register<CGuildMatchingRegister>(GuildOpCode.GuildMatchingRegister);
            Register<CGuildMatchingRegisterCancel>(GuildOpCode.GuildMatchingRegisterCancel);
            Register<CGuildMatchingJoin>(GuildOpCode.GuildMatchingJoin);
            Register<CGuildMatchingJoinAccept>(GuildOpCode.GuildMatchingJoinAccept);
            Register<CGuildMatchingJoinCancel>(GuildOpCode.GuildMatchingJoinCancel);
            Register<CGuildMatchingJoinList>(GuildOpCode.GuildMatchingJoinList);
            Register<CGuildMatchingJoinInfo>(GuildOpCode.GuildMatchingJoinInfo);


            // S2C
            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, false);
            Register<SGuildMasterQuestion>(GuildOpCode.MasterQuestion);
            Register<SGuildViewPort>(GuildOpCode.GuildViewPort);
            Register<SGuildAnsViewport>(GuildOpCode.GuildReqViewport);
            Register<SGuildList>(GuildOpCode.GuildListAll);
            Register<SGuildListS9>(GuildOpCode.GuildListAll);

            VersionSelector.Register<SGuildList>(Resources.ServerSeason.Season6Kor, GuildOpCode.GuildListAll);
            VersionSelector.Register<SGuildListS9>(Resources.ServerSeason.Season9Eng, GuildOpCode.GuildListAll);

            Register<SGuildCreateResult>(GuildOpCode.GuildSaveInfo);
            Register<SGuildResult>(GuildOpCode.GuildResult);
            Register<SGuildSetStatus>(GuildOpCode.GuildSetStatus);
            Register<SGuildRemoveUser>(GuildOpCode.RemoveUser);
            Register<SRelationShipJoinBreakOff>(GuildOpCode.GuildRelationShipAns);
            Register<SUnionList>(GuildOpCode.GuildUnionList);

            Register<SGuildMatchingList>(GuildOpCode.GuildMatchingList);
            Register<SGuildMatchingRegister>(GuildOpCode.GuildMatchingRegister);
            Register<SGuildMatchingRegisterCancel>(GuildOpCode.GuildMatchingRegisterCancel);
            Register<SGuildMatchingJoin>(GuildOpCode.GuildMatchingJoin);
            Register<SGuildMatchingAccept>(GuildOpCode.GuildMatchingJoinAccept);
            Register<SGuildMatchingJoinList>(GuildOpCode.GuildMatchingJoinList);
            Register<SGuildMatchingJoinInfo>(GuildOpCode.GuildMatchingJoinInfo);
            Register<SGuildMatchingNotify>(GuildOpCode.GUildMatchingNotify);
            Register<SGuildMatchingNotifyMaster>(GuildOpCode.GUildMatchingNotifyMaster);
        }
    }
}
