using MuEmu.Monsters;
using MuEmu.Network.QuestSystem;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Util;

namespace MuEmu.Network.Game
{
    public class GameServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GameServices));

        [MessageHandler(typeof(CCheckSum))]
        public void CCheckSum(GSSession session, CCheckSum message)
        {
            //session.Player.CheckSum.IsValid(message.Key);
            Logger
                .ForAccount(session)
                .Debug("Key {0:X4}", message.Key);
        }

        [MessageHandler(typeof(CClientMessage))]
        public void CClientMessage(GSSession session, CClientMessage message)
        {
            Logger
                .ForAccount(session)
                .Information("Client Hack Check {0}", message.Flag);
        }

        [MessageHandler(typeof(CAction))]
        public void CAction(GSSession session, CAction message)
        {
            session.Player.Character.Direction = message.Dir;
            var ans = new SAction((ushort)session.Player.Account.ID, message.Dir, message.ActionNumber, message.Target);
            session.SendAsync(ans);

            foreach (var plr in session.Player.Character.PlayersVP)
                plr.Session.SendAsync(ans);
        }

        [MessageHandler(typeof(CMove))]
        public void CMove(GSSession session, CMove message)
        {
            var dirs = new List<Point>
            {
                new Point(-1,-1),
                new Point(0, -1),
                new Point(1, -1),

                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1),

                new Point(-1, 1),
                new Point(-1, 0)
            };

            var @char = session.Player.Character;
            var count = message.Path[0] & 0x0F;
            var solvedPath = new List<Point>();
            var Cpos = new Point(message.X, message.Y);
            solvedPath.Add(Cpos);

            var valid = true;

            for (int i = 1; i <= count; i++)
            {
                var a = (message.Path[(i + 1) / 2] >> (((i % 2) == 1) ? 4 : 0)) & 0x0F;
                Cpos.Offset(dirs[a]);
                solvedPath.Add(Cpos);
                //Logger.Debug("Path solved [{0}] X:{1} Y:{2}", i, Cpos.X, Cpos.Y);
                var att = @char.Map.GetAttributes(Cpos);
                if (att.Where(y => y == Resources.Map.MapAttributes.NoWalk || y == Resources.Map.MapAttributes.Hide).Count() > 0)
                {
                    valid = false;
                }
            }

            if (!valid)
            {
                session.SendAsync(new SPositionSet { Number = (ushort)session.Player.Account.ID.ShufleEnding(), X = (byte)@char.Position.X, Y = (byte)@char.Position.Y });
                Logger
                    .ForAccount(session)
                    .Error("Invalid path");
                return;
            }

            @char.Position = Cpos;

            session.SendAsync(new SMove((ushort)session.Player.Account.ID, (byte)Cpos.X, (byte)Cpos.Y, message.Path[0]));
        }

        [MessageHandler(typeof(CChatNickname))]
        public void CChatNickname(GSSession session, CChatNickname message)
        {
            Logger
                .ForAccount(session)
                .Information("Chat [" + message.Character.MakeString() + "] {0}", message.Message.MakeString());

        }

        [MessageHandler(typeof(CNewQuestInfo))]
        public void CNewQuestInfo(GSSession session, CNewQuestInfo message)
        {
            Logger
                .ForAccount(session)
                .Information("Quest S5 {0}", message.Quest);
            session.SendAsync(message);
        }

        [MessageHandler(typeof(CCloseWindow))]
        public void CCloseWindow(GSSession session)
        {

        }

        [MessageHandler(typeof(CClientClose))]
        public void CClinetClose(GSSession session, CClientClose message)
        {
            Logger
                .ForAccount(session)
                .Information("User request {0}", message.Type);

            for(int i = 1; i <= 5; i++)
            {
                SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5-i), new SNotice(NoticeType.Blue, $"Saldras en {i} segundos"));
            }

            SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5), new SCloseMsg { Type = message.Type });

            session.Player.Status = message.Type==ClientCloseType.SelectChar?LoginStatus.Logged:LoginStatus.NotLogged;
        }

        [MessageHandler(typeof(CMoveItem))]
        public void CMoveItem(GSSession session, CMoveItem message)
        {
            Logger.Debug("Move item {0}:{1} to {2}:{3}", message.sFlag, message.Source, message.tFlag, message.Dest);

            if (session.Player.Character.Inventory.Move(message.sFlag, message.Source, message.tFlag, message.Dest))
            {
                session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = message.Dest,
                    Result = (byte)message.tFlag
                });
            }
            else
            {
                session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = 0xff,
                    Result = (byte)message.tFlag
                });
            }
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public void CUseItem(GSSession session, CUseItem message)
        {

        }

        [MessageHandler(typeof(CEventEnterCount))]
        public void CEventEnterCount(GSSession session, CEventEnterCount message)
        {
            session.SendAsync(new SEventEnterCount { Type = message.Type });
        }

        [MessageHandler(typeof(CTalk))]
        public void CTalk(GSSession session, CTalk message)
        {
            var npcs = ResourceCache.Instance.GetNPCs();
            var ObjectIndex = message.Number.ShufleEnding();
            var obj = MonstersMng.Instance.GetMonster(ObjectIndex);
            var @char = session.Player.Character;
            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                if (npc.Shop != null)
                {
                    session.Player.Window = npc.Shop.Storage;
                    session.SendAsync(new STalk { Result = 0 });
                    session.SendAsync(new SShopItemList(npc.Shop.Storage.GetInventory()) { ListType = 0 });
                    session.SendAsync(new STax { Type = TaxType.Shop, Rate = 4 });
                }
                else if (npc.Warehouse)
                {
                    session.Player.Window = session.Player.Account.Vault;
                    session.SendAsync(new SNotice(NoticeType.Blue, $"Active Vault: " + (session.Player.Account.ActiveVault + 1)));
                    session.SendAsync(new STalk { Result = 2 });
                    session.SendAsync(new SShopItemList(session.Player.Account.Vault.GetInventory()));
                    session.SendAsync(new SWarehouseMoney { wMoney = 0, iMoney = 0 });
                } else if (npc.JewelMix)
                {
                    session.SendAsync(new STalk { Result = 9 });
                } else if (npc.Buff != 0)
                {
                    @char.Spells.SetBuff((SkillStates)npc.Buff, TimeSpan.FromSeconds(30));
                } else if (npc.Quest != 0xffff)
                {
                    var quest = @char.Quests.Find(obj.Info.Monster);

                    if (quest == null)
                    {
                        session.SendAsync(new SChatTarget(ObjectIndex, "I don't have Quest for you"));
                        return;
                    }

                    var details = quest.Details;
                    Logger.ForAccount(session)
                        .Information("Talk to QuestNPC: {0}, Found Quest:{1}, State:{2}", details.NPC, details.Name, quest.State);
                    session.SendAsync(new SSetQuest { Index = (byte)quest.Index, State = quest.StateByte });
                }
            }
            else
            {
                session.SendAsync(new SChatTarget(ObjectIndex, "Have a good day"));
            }
        }

        [MessageHandler(typeof(CBuy))]
        public void CBuy(GSSession session, CBuy message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            if (plr.Window == null)
            {
                throw new ArgumentException("Player isn't in buy/trade/box/Quest", nameof(session.Player.Window));
            }

            if(plr.Window.GetType() != typeof(Storage))
            {
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var shop = plr.Window as Storage;
            var item = shop.Items[message.Position];
            var bResult = new SBuy
            {
                Result = 0xff,
                ItemInfo = item.GetBytes()
            };

            if(item.BuyPrice > @char.Money)
            {
                Logger
                    .ForAccount(session)
                    .Information("Insuficient Money");
                session.SendAsync(bResult);
                return;
            }

            bResult.Result = @char.Inventory.Add(item);
            if(bResult.Result == 0xff)
            {
                Logger
                    .ForAccount(session)
                    .Information("Insuficient Space");
                session.SendAsync(bResult);
                return;
            }

            @char.Money -= item.BuyPrice;

            Logger
                .ForAccount(session)
                .Information("Buy {0} for {1}", item.BasicInfo.Number, item.BuyPrice);

            session.SendAsync(bResult);
        }

        [MessageHandler(typeof(CSell))]
        public void CSell(GSSession session, CSell message)
        {
            if (session.Player.Window == null)
            {
                throw new ArgumentException("Player isn't in buy/trade/box", nameof(session.Player.Window));
            }

            if (session.Player.Window.GetType() != typeof(Storage))
            {
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var shop = session.Player.Window as Storage;
            var inve = session.Player.Character.Inventory;
            var item = inve.Get(message.Position);
            inve.Remove(message.Position);

            session.Player.Character.Money += item.SellPrice;
            var result = new SSell { Result = 1, Money = session.Player.Character.Money };

            session.SendAsync(result);
        }

        [MessageHandler(typeof(CAttackS5E2))]
        public void CAttackS5E2(GSSession session, CAttackS5E2 message)
        {
            CAttack(session, new CAttack { AttackAction = message.AttackAction, DirDis = message.DirDis, Number = message.Number });
        }

        [MessageHandler(typeof(CAttack))]
        public void CAttack(GSSession session, CAttack message)
        {
            var target = message.Number.ShufleEnding();
            var unkA = message.DirDis & 0x0F;
            var unkB = (message.DirDis & 0xF0) >> 4;

            Logger.ForAccount(session).Debug("Attack {0} {1}:{2} {3}", message.AttackAction, unkA, unkB, target);
            session.Player.Character.Direction = message.DirDis;
            session.Player.SendV2Message(new SAction((ushort)session.ID, message.DirDis, message.AttackAction, target));

            if (target >= MonstersMng.MonsterStartIndex) // Is Monster
            {
                var monster = MonstersMng.Instance.GetMonster(target);
                if(monster.Type == ObjectType.NPC)
                {
                    Logger.ForAccount(session)
                        .Error("NPC Can't be attacked");
                    return;
                }

                //var ad = session.Player.Character.Attack - monster.Defense;
            }
            else
            {

            }
        }

        [MessageHandler(typeof(CWarp))]
        public void CWarp(GSSession session, CWarp message)
        {
            var gates = ResourceCache.Instance.GetGates();

            var gate = (from g in gates
                        where g.Value.GateType == GateType.Warp && g.Value.Target == message.MoveNumber
                        select g.Value).FirstOrDefault();

            if (gate == null)
            {
                Logger.ForAccount(session)
                    .Error("Invalid Gate {0}", message.MoveNumber);

                session.SendAsync(new SNotice(NoticeType.Blue, "You can't go there"));
                return;
            }

            var @char = session.Player.Character;

            if(gate.ReqLevel > @char.Level)
            {
                Logger.ForAccount(session)
                .Error("Level too low");

                session.SendAsync(new SNotice(NoticeType.Blue, $"Try again at Level {gate.ReqLevel}"));
                return;
            }

            if(gate.ReqZen > @char.Money)
            {
                Logger.ForAccount(session)
                .Error("Money too low");

                session.SendAsync(new SNotice(NoticeType.Blue, $"Try again with more Zen"));
                return;
            }

            @char.Money -= gate.ReqZen;

            @char.WarpTo(gate.Map, gate.Door.Location, gate.Dir);
        }

        [MessageHandler(typeof(CJewelMix))]
        public void CJewelMix(GSSession session, CJewelMix message)
        {
            var @char = session.Player.Character;
            var result = @char.Inventory.FindAll(new ItemNumber(14, (ushort)(13 + message.JewelType)));
            var neededJewels = new int[][] {
                new int[] { 10,  500000 },
                new int[] { 20, 1000000 },
                new int[] { 30, 1500000 } };

            if (message.JewelMix > 2)
            {
                Logger.ForAccount(session)
                    .Error("JewelMix out of bounds: {0}", message.JewelMix);
                session.SendAsync(new SJewelMix(0));
                return;
            }
            
            if(result.Count() < neededJewels[message.JewelMix][0])
            {
                Logger.ForAccount(session)
                    .Error("JewelMix Insuficient Jewel count: {0} < {1}", result.Count(), neededJewels[message.JewelMix][0]);
                session.SendAsync(new SJewelMix(0));
                return;
            }

            if(@char.Money < neededJewels[message.JewelMix][1])
            {
                Logger.ForAccount(session)
                    .Error("JewelMix Insuficient Money: {0} < {1}", @char.Money, neededJewels[message.JewelMix][1]);
                session.SendAsync(new SJewelMix(8));
                return;
            }

            foreach (var i in result.Take(neededJewels[message.JewelMix][0]))
            {
                @char.Inventory.Delete(i);
            }

            @char.Inventory.Add(new Item(new ItemNumber(12, (ushort)(30 + message.JewelType)), 0, new { Plus = message.JewelMix }));
            @char.Inventory.SendInventory();
            session.SendAsync(new SJewelMix(1));
        }

        [MessageHandler(typeof(CJewelUnMix))]
        public void CJewelUnMix(GSSession session, CJewelUnMix message)
        {
            var @char = session.Player.Character;
            var target = @char.Inventory.Get(message.JewelPos);
            var neededJewels = new int[][] {
                new int[] { 10,  500000 },
                new int[] { 20, 1000000 },
                new int[] { 30, 1500000 } };

            if (target == null)
            {
                Logger.ForAccount(session)
                    .Error("Item not found: {0}", message.JewelPos);
                session.SendAsync(new SJewelMix(4));
                return;
            }

            if(target.Plus != message.JewelLevel)
            {
                Logger.ForAccount(session)
                    .Error("Item level no match: {0} != {1}", message.JewelLevel, target.Plus);
                session.SendAsync(new SJewelMix(3));
                return;
            }

            if(@char.Money < 1000000)
            {
                Logger.ForAccount(session)
                    .Error("Insuficient money: {0} < 1000000", @char.Money);
                session.SendAsync(new SJewelMix(8));
                return;
            }

            for(var i = 0; i < neededJewels[message.JewelLevel][0]; i++)
            {
                @char.Inventory.Add(new Item(new ItemNumber(14, (ushort)(13 + message.JewelType)), 0));
            }

            @char.Inventory.Delete(message.JewelPos);
            @char.Inventory.SendInventory();
            session.SendAsync(new SJewelMix(7));
        }
    }
}
