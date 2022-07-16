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
using MuEmu.Events.Minigames;
using MuEmu.Monsters;
using MuEmu.Resources;
using MuEmu.Util;
using System;
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

            if (invisibleCloack.Plus != message.Bridge && invisibleCloack.Number != ItemNumber.FromTypeIndex(13, 47))
            {
                await session.SendAsync(new SBloodCastleMove(1));
                return;
            }

            if (itemLevel != invisibleCloack.Plus)
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
            if (item.Plus != message.SquareNumber + 1)
                return;

            var dsm = Program.EventManager.GetEvent<DevilSquares>();
            if (dsm.GetPlayerDS(plr) != message.SquareNumber + 1)
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

        [MessageHandler(typeof(CEventInventoryOpenS16))]
        public async Task EventInventoryOpen(GSSession session, CEventInventoryOpenS16 message)
        {
            var msg = new SEventInventoryOpenS16
            {
                Result = 3,
                EventTime = 0,
            };

            var muRummy = Program.EventManager.GetEvent<MuRummy>();
            var JewelBingo = Program.EventManager.GetEvent<JeweldryBingo>();
            var ballsAndCows = Program.EventManager.GetEvent<BallsAndCows>();
            var eventEgg = Program.EventManager.GetEvent<EventEgg>();

            switch ((BannerType)message.Event)
            {
                case BannerType.Evomon:
                    msg.Id = EventInventoryType.Evomon;
                    break;
                case BannerType.MineSweeper:
                    await session.SendAsync(new SMineSweeperOpen
                    {
                        Result = (byte)(Program.EventManager.GetEvent<MineSweeper>().CurrentState == EventState.Open ? 1 : 0),
                        Cells = Array.Empty<ushort>(),
                    });
                    return;
                case BannerType.MuRummy:
                    if (muRummy.CurrentState == EventState.Open)
                    {
                        msg.EventTime = ((int)muRummy.TimeLeft.TotalSeconds).ShufleEnding();
                        msg.Result = 1;
                        msg.Id = EventInventoryType.MuRummy;
                        msg.Data = 1;
                    }
                    break;
                case BannerType.JeweldryBingo:
                    if (JewelBingo.CurrentState != EventState.None)
                    {
                        await session.SendAsync(new SJewelBingoState
                        {
                            State = JBState.Open,
                        });
                        msg.EventTime = ((int)JewelBingo.TimeLeft.TotalSeconds).ShufleEnding();
                        msg.Id = EventInventoryType.JeweldryBingo;
                    }
                    break;
                case BannerType.MerryXMas:
                    msg.Id = EventInventoryType.XMas;
                    break;
                case BannerType.NewYear:
                    msg.Id = EventInventoryType.NewYear;
                    break;
                case BannerType.BallsAndCows:
                    msg.EventTime = ((int)ballsAndCows.TimeLeft.TotalSeconds).ShufleEnding();
                    msg.Id = EventInventoryType.BallsAndCows;
                    await session.SendAsync(new SBallsAndCowsOpen
                    {
                        Result = 1,
                        Ball = new byte[5],
                        Strikes = new byte[5],
                        Numbers = new byte[15],
                    });
                    return;
                case BannerType.UnityBattleField:
                    msg.Id = EventInventoryType.BattleCore;
                    break;
                default:
                    break;
            }
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CBallsAndCowsStart))]
        public async Task BallsAndCowsStart(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<BallsAndCows>();
            var game = @event.GetGame(session.Player);

            if (game.State == 0)
            {
                var cardDecks = session.Player.Character.Inventory.FindAllEvent(7384);
                if (cardDecks.Count() == 0)
                {
                    await session.SendAsync(new SBallsAndCowsStart { Result = 0x0E });
                    return;
                }
                session.Player.Character.Inventory.DeleteEvent(cardDecks.First());
                session.Player.Character.Inventory.SendEventInventory();
                game.Start();
            }

            await session.SendAsync(new SBallsAndCowsStart { Result = 0x0 });
            if (game.State == 1)
            {
                await session.SendAsync(new SBallsAndCowsOpen {
                    Result = 2,
                    Ball = game.Ball,
                    Strikes = game.Strikes,
                    Numbers = game.Numbers,
                    Score = game.Score,
                });
            }
        }
        [MessageHandler(typeof(CBallsAndCowsPick))]
        public async Task BallsAndCowsPick(GSSession session, CBallsAndCowsPick message)
        {
            var @event = Program.EventManager.GetEvent<BallsAndCows>();
            var game = @event.GetGame(session.Player);

            game.SetNumber(message.Numbers);         

            if(game.State == 2)
            {
                var result = game.Strikes.Any(x => x == 3);
                var msg = new SBallsAndCowsResult
                {
                    Ball = game.Ball,
                    Strikes = game.Strikes,
                    Numbers = game.Hidden,
                    Score = game.Score,
                    Result = (byte)(result ? 3 : 2),
                    Data4 = (byte)(result ? 3 : 2),
                };
                await session.SendAsync(msg);
                var it = new Item(7591);
                if(game.Score > 450)
                {
                    it.Number = 7593;
                }
                else if(game.Score > 300)
                {
                    it.Number = 7592;
                }
                session.Player.Character.GremoryCase.AddItem(it, DateTime.Now.AddDays(1), GremoryStorage.Character, GremorySource.Event);
                @event.ClearGame(session.Player);
            }
            else
            {
                await session.SendAsync(new SBallsAndCowsOpen
                {
                    Result = 2,
                    Ball = game.Ball,
                    Strikes = game.Strikes,
                    Numbers = game.Numbers,
                    Score = game.Score,
                });
            }
        }

        [MessageHandler(typeof(CMuRummyStart))]
        public async Task MuRummyStart(GSSession session, CMuRummyStart message)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);

            if (game.State == 0)
            {
                var cardDecks = session.Player.Character.Inventory.FindAllEvent((ushort)(message.Type == 1 ? 7445 : 7384));
                if (cardDecks.Count() == 0)
                {
                    await session.SendAsync(new SMuRummyMessage { Index = (byte)(message.Type == 1 ? 12 : 0), Value = new ushortle(0) });
                    return;
                }
                session.Player.Character.Inventory.DeleteEvent(cardDecks.First());
                session.Player.Character.Inventory.SendEventInventory();
                game.Start(message.Type);
            }

            await session.SendAsync(new SMuRummyStart
            {
                CardCount = game.CardCount,
                CardInfo = game.GetCardInfo(),
                Score = game.Score,
                SlotStatus = game.GetSlotStatus(),
                SpecialCardCount = game.SpecialCardCount,
                Type = game.Type,
            });
        }

        [MessageHandler(typeof(CMuRummyPlayCard))]
        public async Task MuRummyPlayCard(GSSession session, CMuRummyPlayCard message)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);

            var pc = game.MovePlayCard(message.From, message.To);

            await session.SendAsync(new SMuRummyPlayCard
            {
                From = message.From,
                To = message.To,
                Color = pc.Color,
                Number = pc.Number,
            });
        }
        [MessageHandler(typeof(CMuRummyThrow))]
        public async Task MuRummyThrow(GSSession session, CMuRummyThrow message)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);

            var newCard = game.ThrowPlayCard(message.From);
            await session.SendAsync(message);
            await session.SendAsync(new SMuRummyMessage { Index = 4 });
        }

        [MessageHandler(typeof(CMuRummyReveal))]
        public async Task MuRummyReveal(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);

            if(game.CardCount == 0 || !game.GetCardInfo().Take(5).Any(x => x.Color == 0))
            {
                await session.SendAsync(new SMuRummyMessage { Index = 2 });
                return;
            }

            if(game.GetPlayedCard().Any(x => x.Color != 0))
            {
                await session.SendAsync(new SMuRummyMessage { Index = 3 });
                return;
            }

            var result = game.Reveal();

            await session.SendAsync(new SMuRummyReveal
            {
                CardCount = game.CardCount,
                CardInfo = result.ToArray(),
                SpecialCardCount = game.SpecialCardCount,
            });
        }

        [MessageHandler(typeof(CMuRummyExit))]
        public async Task MuRummyExit(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);
            var @char = session.Player.Character;


            if (game.Score < 250)
            {
                if(@char.Money == int.MaxValue)
                {
                    await session.SendAsync(new SMuRummyMessage { Index = 11 });
                    return;
                }
                @char.Money += 500000;
                await session.SendAsync(new SMuRummyMessage { Index = 10 });
            }
            else
            {
                var it = new Item(7537);
                if(game.Score < 400)
                {
                    it.Number = 7535;
                }
                else if (game.Score < 500)
                {
                    it.Number = 7536;
                }
                @char.GremoryCase.AddItem(it, DateTime.Now.AddDays(1), GremoryStorage.Character, GremorySource.Event);
                await session.SendAsync(new SMuRummyMessage { Index = 9, Value = it.Number.Number });
            }

            await session.SendAsync(new SMuRummyExit { Result = 1 });
            await session.SendAsync(new SMuRummyMessage { Index = 8 });
            @event.ClearGame(session.Player);
        }
        [MessageHandler(typeof(CMuRummySpecialMatch))]
        public async Task MuRummySpecialMatch(GSSession session)
        {
            await MuRummyMatch(session);
        }

        [MessageHandler(typeof(CMuRummyMatch))]
        public async Task MuRummyMatch(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MuRummy>();
            var game = @event.GetGame(session.Player);

            var score = game.Match();
            if(score == 0)
            {
                await session.SendAsync(new SMuRummyMessage
                {
                    Index = 6,
                    Value = new ushortle(0),
                });
            }
            else
            {
                await session.SendAsync(new SMuRummyMatch
                {
                    Result = 1,
                    Score = score,
                    TotalScore = game.Score
                });
                await session.SendAsync(new SMuRummyMessage
                {
                    Index = 5,
                    Value = new ushortle(0),
                });
            }

            await session.SendAsync(new SMuRummyCardList
            {
                CardInfo = game.GetCardInfo()
            });
        }

        [MessageHandler(typeof(CMineSweeperOpen))]
        public async Task CMineSweeperOpen(GSSession session)
        {
            await EventInventoryOpen(session, new CEventInventoryOpenS16 { Event = (byte)BannerType.MineSweeper });
        }

        [MessageHandler(typeof(CMineSweeperStart))]
        public async Task CMineSweeperStart(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<MineSweeper>();
            var game = @event.GetGame(session.Player);
            var cardDecks = session.Player.Character.Inventory.FindAllEvent(7384);
            var msg = new SMineSweeperStart
            {
                Result = 0,
            };
            if (cardDecks.Count() == 0)
            {
                msg.Result = 1;
                await session.SendAsync(msg);
                return;
            }
            session.Player.Character.Inventory.DeleteEvent(cardDecks.First());
            session.Player.Character.Inventory.SendEventInventory();

            var board = game.GetBoard();
            var msg2 = new SMineSweeperOpen
            {
                Result = 2,
                Cells = board.ToArray(),
                Count = (byte)board.Count(),
                CurrentScore = game.Score,
                RemainBombs = game.RemainMines,
            };
            await session.SendAsync(msg);
            await session.SendAsync(new SMineSweeperCreateCell { Effect = 11, X = 8, Y = 6 });
            await session.SendAsync(msg2);
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
            @event.ClearGame(session.Player);

            session.Player.Character.GremoryCase.AddItem(reward, DateTime.Now.AddDays(1), GremoryStorage.Character, GremorySource.Event);
            session.Player.Character.GremoryCase.SendList();

            _ = session.SendAsync(new SMineSweeperGetReward
            {
                Result = 0,
            });
        }

        [MessageHandler(typeof(CJewelBingoStart))]
        public async Task CJewelBingoStart(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<JeweldryBingo>();
            var game = @event.GetGame(session.Player);

            if (game.State == JBState.Open)
            {
                var cardDecks = session.Player.Character.Inventory.FindAllEvent(7384);
                if(cardDecks.Count() == 0)
                {
                    await session.SendAsync(new SJewelBingoState
                    {
                        State = JBState.InsuficientCardDeck,
                    });                    
                    return;
                }
                session.Player.Character.Inventory.DeleteEvent(cardDecks.First());
                session.Player.Character.Inventory.SendEventInventory();
                game.State = JBState.State1;
            }

            if (game.State == JBState.State1)
            {
                await session.SendAsync(new SJewelBingoInfo
                {
                    CurrentJewel = game.AvailableJewels,
                    Grid = game.GetGrid(),
                    Result = 0,
                });
            }else if(game.State == JBState.BoxSelect)
            {
                _= session.SendAsync(new SJewelBingoBox());
            }else if(game.State == JBState.Playing)
            {
                _= session.SendAsync(new SJewelBingoPlayInfo
                {
                    CurrentBox = game.Box,
                    CurrentJewel = game.CurrentJewel,
                    Grid = game.GetGrid(),
                    JewelCount = game.LeftJewels,
                    MatchingJewel = game.GetMatching(),
                    Result = 0
                });
            }else if(game.State == JBState.State6)
            {
                _= session.SendAsync(new SJewelBingoPlayResult
                {
                    Grid = game.GetGrid(),
                    MatchingJewel = game.GetMatching(),
                    Result = 0,
                    LuckyClear = game.LuckyScore,
                    NormalClear = game.NormalScore,
                    JewelryClear = game.JewelryScore,
                });
            }
        }

        [MessageHandler(typeof(CJewelBingoMove))]
        public async Task CJewelBingoMove(GSSession session, CJewelBingoMove message)
        {
            var @event = Program.EventManager.GetEvent<JeweldryBingo>();
            var game = @event.GetGame(session.Player);

            if (message.Type == 0)
                game.Place(message.Slot, message.JewelType);
            else
                game.AutoPlace();

            await session.SendAsync(new SJewelBingoInfo
            {
                CurrentJewel = game.AvailableJewels,
                Grid = game.GetGrid(),
                Result = 0,
            });

            if(game.AvailableJewels.Sum(x => x) == 0)
            {
                game.State = JBState.BoxSelect;
                await session.SendAsync(new SJewelBingoState
                {
                    State = game.State,
                });
                await session.SendAsync(new SJewelBingoBox());
            }
        }

        [MessageHandler(typeof(CJewelBingoBox))]
        public async Task CJewelBingoBox(GSSession session, CJewelBingoBox message)
        {
            var @event = Program.EventManager.GetEvent<JeweldryBingo>();
            var game = @event.GetGame(session.Player);

            game.SelectBox(message.Box);
            game.State = JBState.Playing;
            await session.SendAsync(new SJewelBingoState
            {
                State = game.State,
            });
            await session.SendAsync(new SJewelBingoPlayInfo
            {
                CurrentBox = game.Box,
                CurrentJewel = game.CurrentJewel,
                Grid = game.GetGrid(),
                JewelCount = game.LeftJewels,
                MatchingJewel = game.GetMatching(),
                Result = 0
            });
        }
        [MessageHandler(typeof(CJewelBingoSelect))]
        public async Task CJewelBingoSelect(GSSession session, CJewelBingoSelect message)
        {
            var @event = Program.EventManager.GetEvent<JeweldryBingo>();
            var game = @event.GetGame(session.Player);

            game.SelectJewel(message.Slot, message.JewelType);
            await session.SendAsync(message);
            if (game.LeftJewels > 0)
            {
                await session.SendAsync(new SJewelBingoPlayInfo
                {
                    CurrentBox = game.Box,
                    CurrentJewel = game.CurrentJewel,
                    Grid = game.GetGrid(),
                    JewelCount = game.LeftJewels,
                    MatchingJewel = game.GetMatching(),
                    Result = 0
                });
            }
            else
            {
                game.State = JBState.Playing;
                await session.SendAsync(new SJewelBingoState
                {
                    State = game.State,
                });
                await session.SendAsync(new SJewelBingoPlayResult
                {
                    Grid = game.GetGrid(),
                    MatchingJewel = game.GetMatching(),
                    Result = 0,
                    LuckyClear = game.LuckyScore,
                    NormalClear = game.NormalScore,
                    JewelryClear = game.JewelryScore,
                });
            }
        }

        [MessageHandler(typeof(CJewelBingoGetReward))]
        public async Task CJewelBingoGetReward(GSSession session)
        {
            var @event = Program.EventManager.GetEvent<JeweldryBingo>();
            var game = @event.GetGame(session.Player);

            var reward = game.GetReward();
            @event.ClearGame(session.Player);

            session.Player.Character.GremoryCase.AddItem(reward, DateTime.Now.AddDays(1), GremoryStorage.Character, GremorySource.Event);
            session.Player.Character.GremoryCase.SendList();
            await session.SendAsync(new SJewelBingoState { State = JBState.Open });
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
                _ = session.SendAsync(new SEventItemGet { Result = 0xff, Item = Array.Empty<byte>() });
                session.Exception(ex);
                return;
            }

            var pos = @char.Inventory.AddEvent(item);
            if (pos != 0xff && pos != 0xfd)
            {
                _ = session.SendAsync(new SEventItemGet { Result = pos, Item = item.GetBytes() });
            }
            else
            {
                _ = session.SendAsync(new SEventItemGet { Result = 0xff, Item = item.GetBytes() });
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
