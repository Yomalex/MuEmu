using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Logger.ForAccount(session).Debug("Create Guild: {0}:{1}", message.Name, message.Type);
            GuildManager.CreateGuild(session.Player, message.Name, message.Mark, message.Type);
        }

        // 0xC1 0x66
        [MessageHandler(typeof(CGuildReqViewport))]
        public void CGuildReqViewport(GSSession session, CGuildReqViewport message)
        {
            var log = Logger.ForAccount(session);
            var guild = GuildManager.Get(message.Guild);

            if (guild == null)
            {
                log.Error("Try to get an invalid Guild {0}", message.Guild);
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

        // 0xC1 0x50
        [MessageHandler(typeof(CGuildRequest))]
        public async Task CGuildRequest(GSSession session, CGuildRequest message)
        {
            var log = Logger.ForAccount(session);
            var target = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);

            if(target == null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.PlayerOffline));
                return;
            }

            if(target.Player.Window != null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.InTransaction));
                return;
            }

            if(session.Player.Character.Level < 6)
            {
                await session.SendAsync(new SGuildResult(GuildResult.InsuficientLevel));
                return;
            }

            if(session.Player.Character.Guild != null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.HaveGuild));
                return;
            }

            if(target.Player.Character.Guild.Master.Player != target.Player)
            {
                await session.SendAsync(new SGuildResult(GuildResult.NotGuildMaster));
                return;
            }

            if(!target.Player.Character.Guild.CanAdd())
            {
                await session.SendAsync(new SGuildResult(GuildResult.CannotAcceptMoreMembers));
                return;
            }

            message.Number = (ushort)session.ID;
            await target.SendAsync(message);
        }

        // 0xC1 0x51
        [MessageHandler(typeof(CGuildRequestAnswer))]
        public async Task CGuildRequestAnswer(GSSession session, CGuildRequestAnswer message)
        {
            var source = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);

            if(source == null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.CannotAcceptMoreMembers));
                return;
            }

            if(message.Result == 1)
            {
                session.Player.Character.Guild.Add(source.Player, GuildStatus.Member);
            }else
            {
                await source.SendAsync(new SGuildResult(GuildResult.Fail));
                return;
            }
        }

        // 0xC1 0xE1
        [MessageHandler(typeof(CGuildSetStatus))]
        public async Task CGuildSetStatus(GSSession session, CGuildSetStatus message)
        {
            var log = Logger.ForAccount(session);
            var guild = session.Player.Character.Guild;
            if(guild == null)
            {
                await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.NotExist, message.Name));
                return;
            }

            if(guild.Master.Player != session.Player)
            {
                await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.NotExistPermission, message.Name));
                return;
            }

            var memb = guild.Find(message.Name);
            if(memb == null)
            {
                return;
            }

            if(memb.Name == session.Player.Character.Name)
            {
                await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.NotExistPermission, message.Name));
                return;
            }

            if(message.Type == 1 || message.Type == 2)
            {
                if(message.Status == GuildStatus.Assistant && guild.Assistant == null)
                {
                    log.Information("Status for {0} changed to {1}", message.Name, message.Status);
                    memb.UpdateRank(message.Status);
                    await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.Success, message.Name));
                }
                else if(message.Status == GuildStatus.BattleMaster)
                {
                    var bm = guild.BattleMasters;
                    var bmmax = session.Player.Character.Level / 200 + 1;

                    if(bm.Count() < bmmax)
                    {
                        log.Information("Status for {0} changed to {1}", message.Name, message.Status);
                        memb.UpdateRank(message.Status);
                        await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.Success, message.Name));
                    }
                }
            }else if(message.Type == 3)
            {
                memb.UpdateRank(message.Status);
                await session.SendAsync(new SGuildSetStatus(message.Type, GuildResult.Success, message.Name));
            }
        }

        // 0xC1 0xE3
        [MessageHandler(typeof(CGuildRemoveUser))]
        public async Task CGuildRemoveUser(GSSession session, CGuildRemoveUser message)
        {
            var log = Logger.ForAccount(session);
            var guild = session.Player.Character.Guild;
            if (guild == null)
            {
                await session.SendAsync(new SGuildRemoveUser(GuildResult.NotExist));
                return;
            }

            if (guild.Master.Player != session.Player)
            {
                await session.SendAsync(new SGuildRemoveUser(GuildResult.NotExistPermission));
                return;
            }

            var memb = guild.Find(message.Name);
            if (memb == null)
            {
                return;
            }

            if (memb.Name == session.Player.Character.Name)
            {
                await session.SendAsync(new SGuildRemoveUser(GuildResult.NotExistPermission));
                return;
            }

            guild.Remove(memb);
            await session.SendAsync(new SGuildRemoveUser(GuildResult.Success));
        }
    }
}
