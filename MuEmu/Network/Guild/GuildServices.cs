using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network.Guild
{
    public class GuildServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GuildServices));

        // 0xC1 0x55
        [MessageHandler(typeof(CGuildInfoSave))]
        public void CGuildInfoSave(GSSession session, CGuildInfoSave message)
        {
            GuildManager.CreateGuild(session.Player, message.Name, message.Mark, message.Type);
        }

        // 0xC1 0x66
        [MessageHandler(typeof(CGuildReqViewport))]
        public void CGuildReqViewport(GSSession session, CGuildReqViewport message)
        {
            var guild = GuildManager.Get(message.Guild);

            if (guild == null)
            {
                Logger.Error("Try to get an invalid Guild {0}", message.Guild);
                return;
            }

            session.SendAsync(new SGuildAnsViewport
            {
                GuildName = guild.Name,
                UnionName = "",
                btGuildType = 0,
                GuildNumber = guild.Index,
                Mark = guild.Mark,
            });
        }

        // 0xC1 0x52
        [MessageHandler(typeof(CGuildListAll))]
        public void CGuildListAll(GSSession session)
        {
            GuildManager.SendList(session.Player);
        }
    }
}
