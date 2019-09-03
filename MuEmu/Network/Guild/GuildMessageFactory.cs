using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Guild
{
    public interface IGuildMessage
    { }

    class GuildMessageFactory : MessageFactory<GuildOpCode, IGuildMessage>
    {
        public GuildMessageFactory()
        {
            // C2S
            Register<CGuildInfoSave>(GuildOpCode.GuildSaveInfo);
            Register<CGuildReqViewport>(GuildOpCode.GuildReqViewport);
            Register<CGuildListAll>(GuildOpCode.GuildListAll);

            // S2C
            Register<SGuildMasterQuestion>(GuildOpCode.MasterQuestion);
            Register<SGuildViewPort>(GuildOpCode.GuildViewPort);
            Register<SGuildAnsViewport>(GuildOpCode.GuildReqViewport);
            Register<SGuildList>(GuildOpCode.GuildListAll);
            Register<SGuildCreateResult>(GuildOpCode.GuildSaveInfo);
        }
    }
}
