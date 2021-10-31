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
        public GuildMessageFactory()
        {
            // C2S
            Register<CGuildRequest>(GuildOpCode.GuildRequest);
            Register<CGuildInfoSave>(GuildOpCode.GuildSaveInfo);
            Register<CGuildReqViewport>(GuildOpCode.GuildReqViewport);
            Register<CGuildListAll>(GuildOpCode.GuildListAll);
            Register<CGuildRequestAnswer>(GuildOpCode.GuildResult);
            Register<CGuildSetStatus>(GuildOpCode.GuildSetStatus);
            Register<CGuildRemoveUser>(GuildOpCode.RemoveUser);
            Register<CUnionList>(GuildOpCode.GuildUnionList);


            // S2C
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
            Register<CRelationShipJoinBreakOff>(GuildOpCode.GuildRelationShip);
            Register<SRelationShipJoinBreakOff>(GuildOpCode.GuildRelationShipAns);
            Register<SUnionList>(GuildOpCode.GuildUnionList);
        }
    }
}
