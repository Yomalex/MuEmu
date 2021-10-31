using MU.Network.Game;
using MU.Network.Guild;
using MU.Resources;
using MuEmu.Events.CastleSiege;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network
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

            var msg = new SGuildAnsViewport
            {
                GuildName = guild.Name,
                UnionName = guild.Union.FirstOrDefault()?.Name ?? "",
                btGuildType = 0,
                GuildNumber = guild.Index,
                Mark = guild.Mark,
            };

            _=session.SendAsync(msg);
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


        // 0xC1 E5
        [MessageHandler(typeof(CRelationShipJoinBreakOff))]
        public async Task CRelationShipJoinBreakOff(GSSession session, CRelationShipJoinBreakOff message)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();

            if (siege.State >= SiegeStates.Notify && siege.State <= SiegeStates.StartSiege)
            {
                //MsgOutput(aIndex, Lang.GetText(0, 197));
                await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Guild_RelationShipCantChange)));
                return;
            }

            Character src, dst;

            src = session.Player.Character;
            if(message.TargetUserIndex == 0)
            {
                dst = GuildManager.Instance.Guilds.FirstOrDefault(x => x.Value.Name == message.Guild).Value.Master?.Player?.Character??null;
            }
            else
            {
                dst = Program.server.Clients.FirstOrDefault(x => x.ID == message.TargetUserIndex)?.Player?.Character ?? null;
            }

            if(dst == null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.PlayerOffline));
                return;
            }

            if (src.Guild == null || dst.Guild == null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.HaveGuild));
                return;
            }

            if (src.Guild.Master.Player != src.Player || dst.Guild.Master.Player != dst.Player)
            {
                await session.SendAsync(new SGuildResult(GuildResult.NotGuildMaster));
                return;
            }

            if (src.Player.Window != null || dst.Player.Window != null)
            {
                await session.SendAsync(new SGuildResult(GuildResult.InTransaction));
                return;
            }

            var errmsg = new SRelationShipJoinBreakOff
            {
                RequestType = message.RequestType,
                Result = 0,
                RelationShipType = message.RelationShipType,
                wzTargetUserIndex = message.wzTargetUserIndex,
            };

            switch (message.RequestType)
            {
                case GuildUnionRequestType.Join:
                    switch(message.RelationShipType)
                    {
                        case GuildRelationShipType.Union:
                            break;
                        case GuildRelationShipType.Rivals:
                            break;
                    }
                    break;
                case GuildUnionRequestType.BreakOff:
                    switch (message.RelationShipType)
                    {
                        case GuildRelationShipType.Union:
                            break;
                        case GuildRelationShipType.Rivals:
                            break;
                    }
                    break;
            }

            message.wzTargetUserIndex = ((ushort)session.ID).ShufleEnding();
            await dst.Player.Session.SendAsync(message);
        }

        // 0xC1 E6
        [MessageHandler(typeof(SRelationShipJoinBreakOff))]
        public async Task SRelationShipJoinBreakOff(GSSession session, SRelationShipJoinBreakOff message)
        {
            var src = session.Player.Character;
            Character dst = null;

            if (message.wzTargetUserIndex.ShufleEnding() != session.Player.ID)
                dst = Program.server.Clients.First(x => x.ID == message.wzTargetUserIndex.ShufleEnding()).Player.Character;
            else if (message.RelationShipType == GuildRelationShipType.Union)
                dst = src.Guild.Union.First().Master.Player?.Character ?? null;
            else if(message.RelationShipType == GuildRelationShipType.Rivals)
                dst = src.Guild.Rival.First().Master.Player?.Character ?? null;

            if (message.Result == 1)
            {
                if (dst != null)
                    src.Guild.ChangeRelation(dst.Guild, message.RequestType, message.RelationShipType);
                else if (message.RelationShipType == GuildRelationShipType.Union)
                    src.Guild.ChangeRelation(src.Guild.Union.First(), message.RequestType, message.RelationShipType);
                else if (message.RelationShipType == GuildRelationShipType.Rivals)
                    src.Guild.ChangeRelation(src.Guild.Rival.First(), message.RequestType, message.RelationShipType);

                message.wzTargetUserIndex = dst.Player.ID.ShufleEnding();
                _= src.Player.Session.SendAsync(message);

                if (dst != null)
                {
                    message.wzTargetUserIndex = src.Player.ID.ShufleEnding();
                    _ = dst.Player.Session.SendAsync(message);
                    CGuildReqViewport(dst.Player.Session, new MU.Network.Guild.CGuildReqViewport { Guild = dst.Guild.Index });
                    CGuildReqViewport(dst.Player.Session, new MU.Network.Guild.CGuildReqViewport { Guild = src.Guild.Index });
                }
                CGuildReqViewport(src.Player.Session, new MU.Network.Guild.CGuildReqViewport { Guild = dst.Guild.Index });
                CGuildReqViewport(src.Player.Session, new MU.Network.Guild.CGuildReqViewport { Guild = src.Guild.Index });

                Logger.Information("Relation Changed, {0} now have {1} to {2} as {3}",
                    dst.Guild.Name,
                    message.RequestType,
                    src.Guild.Name,
                    message.RelationShipType);
            }
            else
            {
                message.wzTargetUserIndex = src.Player.ID.ShufleEnding();
                await dst.Player.Session.SendAsync(message);
            }
        }

        // 0xC1 E9
        [MessageHandler(typeof(CUnionList))]
        public async Task CUnionList(GSSession session)
        {
            var guild = session.Player.Character.Guild;
            var list = guild.Union.Select((x,i) => new UnionListDto
            {
                GuildName = x.Name,
                Mark = x.Mark,
                MemberNum = (byte)x.Members.Count
            }).ToArray();
            await session.SendAsync(new SUnionList
            {
                List = list,
                Count = (byte)list.Count(),
                Result = 1,
                RivalMemberNum = 0,
                UnionMemberNum = 0,
            });
        }
    }
}
