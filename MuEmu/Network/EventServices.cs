using MU.Network.CastleSiege;
using MU.Network.Event;
using MU.Network.Game;
using MU.Resources;
using MuEmu.Events;
using MuEmu.Events.AcheronGuardian;
using MuEmu.Events.BloodCastle;
using MuEmu.Events.CastleSiege;
using MuEmu.Events.ChaosCastle;
using MuEmu.Events.Crywolf;
using MuEmu.Events.DevilSquare;
using MuEmu.Events.Event_Egg;
using MuEmu.Events.ImperialGuardian;
using MuEmu.Events.Kanturu;
using MuEmu.Events.LuckyCoins;
using MuEmu.Events.MineSweeper;
using MuEmu.Events.Rummy;
using MuEmu.Monsters;
using MuEmu.Resources;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network
{
    public class EventServices : MessageHandler
    {
        [MessageHandler(typeof(CEventRemainTime))]
        public async Task CEventRemainTime(GSSession session, CEventRemainTime message)
        {
            var res = new SEventRemainTime { EventType = message.EventType };
            switch (message.EventType)
            {
                case EventEnterType.DevilSquare:
                    var evds = Program.EventManager.GetEvent<DevilSquares>();
                    res.RemainTime = evds.RemainTime;
                    res.EnteredUser = evds.Count;
                    break;
                case EventEnterType.BloodCastle:
                    var evbc = Program.EventManager.GetEvent<BloodCastles>();
                    res.RemainTime = evbc.RemainTime;
                    break;
                case EventEnterType.ChaosCastle:
                    var ev = Program.EventManager.GetEvent<ChaosCastles>();
                    res.RemainTime = ev.RemainTime;
                    res.EnteredUser = ev.Count;
                    break;
                case EventEnterType.IllusionTemple:
                    res.RemainTime = 0;
                    break;
            }

            await session.SendAsync(res);
        }

        [MessageHandler(typeof(CLuckyCoinsCount))]
        public async Task CLuckyCoinsCount(GSSession session/*, CLuckyCoinsCount message*/)
        {
            var coins = await LuckyCoins.Registered(session.Player);
            await session.SendAsync(new SLuckyCoinsCount(coins));
        }

        [MessageHandler(typeof(CLuckyCoinsRegistre))]
        public async Task CLuckyCoinsRegistre(GSSession session/*, CLuckyCoinsRegistre message*/)
        {
            var coins = await LuckyCoins.Registre(session.Player);
            await session.SendAsync(new SLuckyCoinsCount(coins));
        }

        [MessageHandler(typeof(CBloodCastleMove))]
        public async Task CBloodCastleMove(GSSession session, CBloodCastleMove message)
        {
            var plr = session.Player;
            var @char = session.Player.Character;

            var invisibleCloack = @char.Inventory.Get(message.ItemPos);
            var evbc = Program.EventManager.GetEvent<BloodCastles>();
            var itemLevel = evbc.GetEventNumber(plr);

            if(invisibleCloack.Plus != message.Bridge && invisibleCloack.Number != ItemNumber.FromTypeIndex(13,47))
            {
                await session.SendAsync(new SBloodCastleMove(1));
                return;
            }

            if(itemLevel != invisibleCloack.Plus)
            {
                await session.SendAsync(new SBloodCastleMove((byte)(itemLevel > invisibleCloack.Plus ? 4 : 3)));
                return;
            }

            if (!evbc.TryAdd(plr))
            {
                await session.SendAsync(new SBloodCastleMove(5));
                return;
            }

            await @char.Inventory.Delete(message.ItemPos);
        }

        [MessageHandler(typeof(CCrywolfBenefit))]
        public void CCrywolfBenefit(GSSession session)
        {
            Program.EventManager
                .GetEvent<Crywolf>()
                .SendBenefit(session);
        }

        [MessageHandler(typeof(CCrywolfState))]
        public void CCrywolfState(GSSession session)
        {
            Program.EventManager
                .GetEvent<Crywolf>()
                .SendState(session);
        }

        [MessageHandler(typeof(CCrywolfContract))]
        public void CCrywolfContract(GSSession session, CCrywolfContract message)
        {
            session.Player.Window = MonstersMng.Instance.GetMonster(message.Index);
            Program.EventManager
                .GetEvent<Crywolf>()
                .NPCTalk(session.Player);
        }

        [MessageHandler(typeof(CDevilSquareMove))]
        public async Task CDevilSquareMove(GSSession session, CDevilSquareMove message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            var itemPos = (byte)(message.InvitationItemPos - 12);
            var item = @char.Inventory.Get(itemPos);
            if (item.Plus != message.SquareNumber+1)
                return;

            var dsm = Program.EventManager.GetEvent<DevilSquares>();
            if (dsm.GetPlayerDS(plr) != message.SquareNumber+1)
                return;

            if (!dsm.TryAdd(plr))
                return;

            await @char.Inventory.Delete(itemPos);
        }

        [MessageHandler(typeof(CChaosCastleMove))]
        public async Task CChaosCastleMove(GSSession session, CChaosCastleMove message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            var item = @char.Inventory.Get(message.InvitationItemPos);

            var dsm = Program.EventManager.GetEvent<ChaosCastles>();

            if (!dsm.TryAdd(plr))
                return;

            await @char.Inventory.Delete(item);
        }

        [MessageHandler(typeof(CKanturuStateInfo))]
        public void CKanturuStateInfo(GSSession session)
        {
            var kanturu = Program.EventManager.GetEvent<Kanturu>();
            kanturu.NPCTalk(session.Player);
        }

        [MessageHandler(typeof(CKanturuEnterBossMap))]
        public void CKanturuEnterBossMap(GSSession session)
        {
            var kanturu = Program.EventManager.GetEvent<Kanturu>();
            kanturu.TryAdd(session.Player);
        }

        [MessageHandler(typeof(CImperialGuardianEnter))]
        public void CImperialGuardianEnter(GSSession session)
        {
            Program.EventManager.GetEvent<ImperialGuardian>()
                .TryAdd(session.Player);
        }

        [MessageHandler(typeof(CMuRummyOpen))]
        public void CMuRummyOpen(GSSession session)
        {
            var muRummy = Program.EventManager.GetEvent<MuRummy>();
            var eventEgg = Program.EventManager.GetEvent<EventEgg>();
            DateTime end;
            byte result;

            if(muRummy.CurrentState == EventState.Playing)
            {
                end = DateTime.UtcNow.Add(muRummy.TimeLeft);
                result = 1;
            }
            else
            {
                end = DateTime.UtcNow.Add(eventEgg.TimeLeft);
                result = 3;
            }
            
            var unixTimestamp = (int)(end.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var bytes = BitConverter.GetBytes(unixTimestamp);
            var msg = new SMuRummyOpen
            {
                btResult = result,
                btEventTime1 = bytes[3],
                btEventTime2 = bytes[2],
                btEventTime3 = bytes[1],
                btEventTime4 = bytes[0],
            };
            _ = session.SendAsync(msg);

            //session.Player.Character.Inventory.SendEventInventory();
        }

        [MessageHandler(typeof(CMineSweeperOpen))]
        public void CMineSweeperOpen(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var msg = new SMineSweeperOpen
            {
                Result = (byte)(@event.CurrentState==EventState.Open?1:1),
                Cells = Array.Empty<ushort>(),
            };
            _ = session.SendAsync(msg);

            //session.Player.Character.Inventory.SendEventInventory();
        }

        [MessageHandler(typeof(CMineSweeperStart))]
        public void CMineSweeperStart(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var game = @event.GetGame(session.Player);
            var msg = new SMineSweeperStart
            {
                Result = 0,
            };
            var board = game.GetBoard();
            var msg2 = new SMineSweeperOpen
            {
                Result = 2,
                Cells = board.ToArray(),
                Count = (byte)board.Count(),
                CurrentScore = game.Score,
                RemainBombs = game.RemainMines,
            };
            _ = session.SendAsync(msg);
            _ = session.SendAsync(new SMineSweeperCreateCell { Effect = 11, X = 8, Y = 6 });
            _ = session.SendAsync(msg2);
        }

        [MessageHandler(typeof(CMineSweeperReveal))]
        public void CMineSweeperReveal(GSSession session, CMineSweeperReveal message)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var game = @event.GetGame(session.Player);

            var result = game.Reveal(message.Cell).ToArray();

            _ = session.SendAsync(new SMineSweeperReveal
            {
                Cell = message.Cell,
                Cells = result,
                Score = game.Score,
            });

            if(!result.Any())
            {
                game.Finish(true);
            }

            if(game.IsClear())
            {
                MineSweeperSendScore(session, game);
            }
        }

        private void MineSweeperSendScore(GSSession session, MineSweeperGame game)
        {
            var cells = game.FailedBomb.Select(x => x.Cell);
            _ = session.SendAsync(new SMineSweeperEnd
            {
                SteppedOnBomb = (byte)(game.Losed ? 50 : 0),
                Cells = cells.ToArray(),
                Count = (byte)cells.Count(),
                BombsFound = (ushort)(game.Correct * 50),
                BombsFailure = (ushort)(game.Incorrect * 20),
                Clear = (byte)(game.Losed ? 0 : 500),
                Result = (byte)(game.Losed ? 1 : 0),
                Score = game.Score,
                TotalScore = game.TotalScore,
            });
        }
        [MessageHandler(typeof(CMineSweeperMark))]
        public void CMineSweeperMark(GSSession session, CMineSweeperMark message)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var game = @event.GetGame(session.Player);

            var result = game.Mark(message.Cell);

            _ = session.SendAsync(new SMineSweeperMark
            {
                Cell = message.Cell,
                RemainBombs = game.RemainMines,
                Result = result,
            });

            if(game.Finished)
            {
                MineSweeperSendScore(session, game);
            }
        }

        [MessageHandler(typeof(CMineSweeperGetReward))]
        public void CMineSweeperGetReward(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var game = @event.GetGame(session.Player);

            if(!game.Finished)
            {
                return;
            }

            var reward = game.GetReward();
            @event.Clear(session.Player);

            session.Player.Character.GremoryCase.AddItem(reward, DateTime.Now.AddDays(1), GremoryStorage.Character, GremorySource.GMReward);
            session.Player.Character.GremoryCase.SendList();

            _ = session.SendAsync(new SMineSweeperGetReward
            {
                Result = 0,
            });
        }

        [MessageHandler(typeof(CEventItemGet))]
        public void CEventItemGet(GSSession session, CEventItemGet message)
        {
            var @char = session.Player.Character;
            var map = @char.Map;
            Item item;

            try
            {
                item = map.ItemPickUp(@char, message.Number);
            } catch (Exception ex)
            {
                session.Exception(ex);
                return;
            }

            var pos = @char.Inventory.AddEvent(item);
            if (pos != 0xff)
            {
                _ = session.SendAsync(new SEventItemGet { Result = pos, Item = item.GetBytes() });
            }
            else
            {
                session.Player.Character.Inventory.SendEventInventory();
            }
        }

        [MessageHandler(typeof(CEventItemThrow))]
        public void CEventItemThrow(GSSession session, CEventItemThrow message)
        {
            var plr = session.Player;
            var inv = plr.Character.Inventory;
            var item = inv.GetEvent(message.Ipos);
            if (item == null)
                return;

            var bag = (from b in ResourceCache.Instance.GetItemBags()
                       where b.Number == item.Number && (b.Plus == item.Plus || b.Plus == 0xffff)
                       select b).FirstOrDefault();

            _ = session.SendAsync(new SEventItemThrow { Result = 1, Pos = message.Ipos });
            if (bag != null)
            {
                if (bag.LevelMin <= plr.Character.Level)
                {
                    inv.DeleteEvent(message.Ipos);
                    foreach (var reward in bag.GetReward())
                    {
                        plr.Character.Map.AddItem(message.px, message.py, reward, plr.Character);
                    }
                    var msg = new SCommand(ServerCommandType.Fireworks, (byte)plr.Character.Position.X, (byte)plr.Character.Position.X);
                    _ = plr.Session.SendAsync(msg);
                    plr.SendV2Message(msg);
                }
                else
                {
                    _ = item.Drop(message.px, message.py);
                    return;
                }
            }
            else
            {
                _ = item.Drop(message.px, message.py);
                inv.RemoveEvent(message.Ipos);
            }
        }

        [MessageHandler(typeof(CSiegeState))]
        public async Task CSiegeState(GSSession session)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();

            var msg = new SSiegeState
            {
                Result = 1,
                CastleSiegeState = (byte)siege.State,
                StartYear = ((ushort)siege.StateStart.Year).ShufleEnding(),
                StartMonth = (byte)siege.StateStart.Month,
                StartDay = (byte)siege.StateStart.Day,
                StartHour = (byte)siege.StateStart.Hour,
                StartMinute = (byte)siege.StateStart.Minute,
                EndYear = ((ushort)siege.StateEnd.Year).ShufleEnding(),
                EndMonth = (byte)siege.StateEnd.Month,
                EndDay = (byte)siege.StateEnd.Day,
                EndHour = (byte)siege.StateEnd.Hour,
                EndMinute = (byte)siege.StateEnd.Minute,
                SiegeStartYear = ((ushort)siege.SiegeExpectedPeriod.Year).ShufleEnding(),
                SiegeStartMonth = (byte)siege.SiegeExpectedPeriod.Month,
                SiegeStartDay = (byte)siege.SiegeExpectedPeriod.Day,
                SiegeStartHour = (byte)siege.SiegeExpectedPeriod.Hour,
                SiegeStartMinute = (byte)siege.SiegeExpectedPeriod.Minute,
                StateLeftSec = siege.StageTimeLeft.ShufleEnding(),
                OwnerGuild = siege.Owner?.Name??"",
                OwnerGuildMaster = siege.Owner?.Master.Name??"",
            };
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CGuildRegisteInfo))]
        public async Task CGuildRegisteInfo(GSSession session)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();

            var rank = siege.AttackGuild.OrderByDescending(x => x.Marks).ToList();
            var guildInfo = rank.FirstOrDefault(x => x.Guild == session.Player.Character.Guild);

            var msg = new SGuildRegisteInfo
            {
                Result = 0,
                GuildName = session.Player.Character.Guild?.Name??"",
            };

            if (guildInfo != null)
            {
                msg.Result = 1;
                msg.RegRank = (byte)(rank.IndexOf(guildInfo)+1);
                msg.IsGiveUp = (byte)(guildInfo.GiveUp ? 1 : 0);
                msg.GuildMark = guildInfo.Marks;
            }

            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CGuildMarkOfCastleOwner))]
        public async Task CGuildMarkOfCastleOwner(GSSession session)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();
            var msg = new SGuildMarkOfCastleOwner { Mark = new byte[32] };
            Array.Fill(msg.Mark, (byte)0xcc);
            if(siege.Owner != null)
            {
                msg.Mark = siege.Owner.Mark;
            }
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CGuildRegiste))]
        public async Task CGuildRegiste(GSSession session)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();

            if(siege.State != SiegeStates.RegisteSiege)
            {
                await session.SendAsync(new SGuildRegiste { Result = 7 });
                return;
            }

            if (session.Player.Character.Guild == null)
            {
                await session.SendAsync(new SGuildRegiste { Result = 6 });
                return;
            }

            if (!session.Player.Character.Guild.IsUnionMaster)
            {
                await session.SendAsync(new SGuildRegiste { Result = 0 });
                return;
            }

            if (siege.Owner == session.Player.Character.Guild.Union[0])
            {
                await session.SendAsync(new SGuildRegiste { Result = 3 });
                return;
            }

            siege.RegisteAttackGuild(session.Player);
        }

        [MessageHandler(typeof(CSiegeGuildList))]
        public async Task CSiegeGuildList(GSSession session)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();

            var list = siege
                .AttackGuild
                .OrderByDescending(x => x.Marks)
                .Select((x, i) => new SiegueGuildDto { GuildName = x.Guild.Name, IsGiveUp = (byte)(x.GiveUp ? 1 : 0), SeqNum = (byte)(i+1), RegMarks = x.Marks})
                .ToArray();

            await session.SendAsync(new SSiegeGuildList { List = list, Result = 1 });
        }

        [MessageHandler(typeof(CSiegeRegisteMark))]
        public async Task CSiegeRegisteMark(GSSession session, CSiegeRegisteMark message)
        {
            var siege = Program.EventManager.GetEvent<CastleSiege>();
            var item = session.Player.Character.Inventory.FindAllItems(7189).Where(x => x.Plus == 3).FirstOrDefault();
            var guild = session.Player.Character.Guild;

            var msg = new SSiegeRegisteMark
            {
                Result = 3,
                GuildName = guild?.Name??"",
            };

            if (siege.State != SiegeStates.RegisteMark || guild==null || item == null)
            {
                await session.SendAsync(msg);
                return;
            }

            msg.Result = 1;
            msg.GuildMark = siege.RegisteMark(guild, item.Durability);
            if(msg.GuildMark == 0)
            {
                msg.Result = 0;
            }
            else
            {
                await session.Player.Character.Inventory.Delete((byte)item.SlotId);
            }
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CAcheronEventEnter))]
        public void AcheronEventEnter(GSSession session)
        {
            Program.EventManager.GetEvent<AcheronGuardian>()
                .TryAdd(session.Player);
        }
    }
}
