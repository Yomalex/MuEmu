using MuEmu.Events.BloodCastle;
using MuEmu.Events.EventChips;
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
            if(session.Player.Window != null)
                Logger.Debug("Player close window:{0}", session.Player.Window.GetType().ToString());
            session.Player.Window = null;
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

        [MessageHandler(typeof(CPointAdd))]
        public void CPointAdd(GSSession session, CPointAdd message)
        {
            var @char = session.Player.Character;
            var msg = new SPointAdd
            {
                Result = (byte)(0x10 | (byte)message.Type),
            };

            switch(message.Type)
            {
                case PointAdd.Strength:
                    if (@char.Strength + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Strength++;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    }else
                    {
                        msg.Result = 0;
                    }
                    break;
                case PointAdd.Agility:
                    if (@char.Agility + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Agility++;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    }
                    else
                    {
                        msg.Result = 0;
                    }
                    break;
                case PointAdd.Vitality:
                    if (@char.Vitality + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Vitality++;
                        msg.MaxLifeAndMana = (ushort)@char.MaxHealth;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    }
                    else
                    {
                        msg.Result = 0;
                    }
                    break;
                case PointAdd.Energy:
                    if (@char.Energy + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Energy++;
                        msg.MaxLifeAndMana = (ushort)@char.MaxMana;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    }
                    else
                    {
                        msg.Result = 0;
                    }
                    break;
                case PointAdd.Command:
                    if (@char.Command + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Command++;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    }
                    else
                    {
                        msg.Result = 0;
                    }
                    break;
            }

            session.SendAsync(msg);
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public async Task CUseItem(GSSession session, CUseItem message)
        {
            var @char = session.Player.Character;
            var inv = @char.Inventory;

            Logger.Debug("CUseItem {0} {1} {2}", message.Source, message.Dest, message.Type);

            var Source = inv.Get(message.Source);

            if(Source.BasicInfo.Skill != Spell.None)
            {
                if(@char.Spells.TryAdd(Source.BasicInfo.Skill))
                {
                    await inv.Delete(message.Source);
                }
                return;
            }

            switch(Source.Number)
            {
                case 14 * 512 + 0:// Apple
                case 14 * 512 + 1:// Small HP Potion
                case 14 * 512 + 2:// Medium HP Potion
                case 14 * 512 + 3:// Big HP Potion
                    var AddLife = (Source.SellPrice * 10) - (@char.Level * 2);
                    if (AddLife < 0)
                        AddLife = 0;

                    float AddLifeRate = ((Source.Number.Index+1) * 10.0f) + (Source.Plus * 5.0f);
                    AddLife += (long)(@char.MaxHealth * AddLifeRate / 100.0f);
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;
                    break;
                case 14 * 512 + 4:// Small MP Potion
                case 14 * 512 + 5:// Medium MP Potion
                case 14 * 512 + 6:// Big MP Potion
                    var AddMana = (Source.SellPrice * 10) - @char.Level;
                    if (AddMana < 0)
                        AddMana = 0;

                    float AddManaRate = ((Source.Number.Index - 3) * 10.0f) + (Source.Plus * 5.0f);
                    AddMana += (uint)(@char.MaxMana * AddManaRate / 100.0f);
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;
                    break;
                case 14 * 512 + 35:// Small SD Potion
                case 14 * 512 + 36:// Medium SD Potion
                case 14 * 512 + 37:// Big SD Potion
                    float addSDRate = @char.MaxShield * (25.0f + (Source.Number.Index - 35) * 10.0f) / 100.0f;

                    @char.Shield += addSDRate;
                    await session.SendAsync(new SEffect((ushort)session.ID, ClientEffect.RecoverShield));
                    break;
                case 14 * 512 + 13: //  Jewel of Bless
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Plus >= 7)
                            break;

                        await inv.Delete(message.Source);
                        Target.Plus++;
                    }
                    break;
                case 14 * 512 + 14: //  Jewel of Soul
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Plus >= 9)
                            break;

                        await inv.Delete(message.Source);
                        var soulRate = 50 + (Target.Luck ? 25 : 0);
                        if (new Random().Next(100) < soulRate)
                        {
                            Target.Plus++;
                        }
                        else
                        {
                            if (Target.Plus > 7)
                                Target.Plus = 0;
                            else
                                Target.Plus--;
                        }
                    }
                    break;
                case 14 * 512 + 16: // Jewel of Life
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Option28 >= 3)
                            break;

                        if (!Target.BasicInfo.Option)
                            break;

                        await inv.Delete(message.Source);
                        Target.Option28++;
                    }
                    break;
            }
        }

        [MessageHandler(typeof(CItemThrow))]
        public void CItemThrow(GSSession session, CItemThrow message)
        {
            var logger = Logger.ForAccount(session);
            var plr = session.Player;
            var inv = plr.Character.Inventory;
            var item = inv.Get(message.Source);
            inv.Delete(message.Source);

            var date = plr.Character.Map.AddItem(message.MapX, message.MapY, item);
            session.SendAsync(new SItemThrow { Source = message.Source, Result = 1 });
            logger.Information("Drop item {0} at {1},{2} in {3} deleted at {4}", item.Number, message.MapX, message.MapY, plr.Character.MapID, date);
        }

        [MessageHandler(typeof(CItemGet))]
        public async Task CItemGet(GSSession session, CItemGet message)
        {
            var @char = session.Player.Character;
            var item = (from obj in @char.Map.Items
                       where obj.Index == message.Number && obj.State == Resources.Map.ItemState.Created
                       select obj).FirstOrDefault();

            if(item == null)
            {
                Logger.ForAccount(session)
                    .Error("Item {0} no exist", message.Number);
                await session.SendAsync(new SItemGet { Result = 0xff });
                return;
            }
            
            if(item.Item.Number != ItemNumber.Zen)
            {
                var pos = @char.Inventory.Add(item.Item);
                if (pos == 0xff)
                {
                    await session.SendAsync(new SItemGet { Result = 0xff });
                    return;
                }
                await session.SendAsync(new SItemGet { ItemInfo = item.Item.GetBytes(), Result = pos });
            }
            else
            {
                session.Player.Character.Money += item.Item.BuyPrice;
            }

            item.State = Resources.Map.ItemState.Deleted;
            var msg = new SViewPortItemDestroy { ViewPort = new Data.VPDestroyDto[] { new Data.VPDestroyDto(item.Index) } };
            await session.SendAsync(msg);
            await session.Player.SendV2Message(msg);
        }

        [MessageHandler(typeof(CEventEnterCount))]
        public void CEventEnterCount(GSSession session, CEventEnterCount message)
        {
            session.SendAsync(new SEventEnterCount { Type = message.Type, Left = 10 });
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
                    session.Player.Character.Inventory.Lock = true;

                    session.SendAsync(new SNotice(NoticeType.Blue, $"Active Vault: " + (session.Player.Account.ActiveVault + 1)));
                    session.SendAsync(new STalk { Result = 2 });
                    session.SendAsync(new SShopItemList(session.Player.Account.Vault.GetInventory()));
                    session.SendAsync(new SWarehouseMoney(session.Player.Account.VaultMoney, session.Player.Character.Money));
                }
                else if (npc.EventChips)
                {
                    EventChips.NPCTalk(session.Player);
                }
                else if (npc.MessengerAngel)
                {
                    BloodCastles.MessengerAngelTalk(session.Player);
                }
                else if (npc.KingAngel)
                {
                    BloodCastles.AngelKingTalk(session.Player);
                }
                else if (npc.Window != 0)
                {
                    session.SendAsync(new STalk { Result = npc.Window });

                    if (npc.Window == 3) // ChaosMachine
                    {
                        session.Player.Character.Inventory.Lock = true;
                        session.Player.Window = session.Player.Character.Inventory.ChaosBox;
                    }

                } else if (npc.Buff != 0)
                {
                    @char.Spells.SetBuff((SkillStates)npc.Buff, TimeSpan.FromSeconds(120));
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
                        .Information("Talk to QuestNPC: {0}, Found Quest:{1}, State:{2}", obj.Info.Name, details.Name, quest.State);
                    session.SendAsync(new SSetQuest { Index = (byte)quest.Index, State = quest.StateByte });
                }
            }
            else
            {
                Logger.ForAccount(session)
                    .Debug("Talk to unasigned NPC {0}", obj.Info.Monster);
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
        public async Task CAttack(GSSession session, CAttack message)
        {
            var target = message.Number.ShufleEnding();
            var Dir = message.DirDis & 0x0F;
            var Dis = (message.DirDis & 0xF0) >> 4;

            Logger.ForAccount(session)
                .Debug("Attack {0} {1}:{2} {3}", message.AttackAction, Dir, Dis, target);

            session.Player.Character.Direction = message.DirDis;

            if (target >= MonstersMng.MonsterStartIndex) // Is Monster
            {
                try
                {
                    var monster = MonstersMng.Instance.GetMonster(target);

                    if (monster.Life <= 0)
                        return;

                    await session.Player.SendV2Message(new SAction((ushort)session.ID, message.DirDis, message.AttackAction, target));
                    if (monster.Type == ObjectType.NPC)
                    {
                        Logger.ForAccount(session)
                            .Error("NPC Can't be attacked");
                        return;
                    }

                    DamageType type;
                    var attack = session.Player.Character.Attack(monster, out type);
                    await monster.GetAttacked(session.Player, attack, type);
                }
                catch(Exception ex)
                {
                    Logger.ForAccount(session)
                        .Error(ex, "Invalid monster #{0}", target);
                }
            }
            else
            {

            }
        }

        [MessageHandler(typeof(CMagicAttack))]
        public void CMagicAttack(GSSession session, CMagicAttack message)
        {
            var @char = session.Player.Character;
            var target = message.Target;
            MuEmu.Data.SpellInfo spell;
            Spells spells = null;
            Monster monster = null;
            Player player = null;

            if (!@char.Spells.SpellDictionary.ContainsKey(message.MagicNumber))
            {
                Logger.Error("Invalid Magic, user don't own this spell {0}", message.MagicNumber);
                return;
            }

            spell = @char.Spells.SpellDictionary[message.MagicNumber];

            try
            {
                if (target >= MonstersMng.MonsterStartIndex) // Is Monster
                {
                    monster = MonstersMng.Instance.GetMonster(target);
                    spells = monster.Spells;
                }
                else
                {
                    player = Program.server.Clients.First(x => x.ID == target).Player;
                    spells = player.Character.Spells;
                }
            }catch(Exception)
            {
                Logger.Error("MagicAttack: Invalid target");
                return;
            }

            var mana = @char.Mana - spell.Mana;
            var bp = @char.Stamina;

            if (mana >= 0 && bp >= 0)
            {
                switch(spell.Number)
                {
                    case Spell.Poison:
                        @char.Spells.AttackSend(spell.Number, message.Target, false);
                        spells.SetBuff(SkillStates.Poison, TimeSpan.FromSeconds(60), @char);
                        break;
                    case Spell.Heal:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        var addLife = @char.EnergyTotal / 5;

                        if(spells.Character == null)
                        {
                            return;
                        }

                        spells.Character.Health += addLife;
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.GreaterDefense:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        spells.SetBuff(SkillStates.Defense, TimeSpan.FromSeconds(60), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.GreaterDamage:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        spells.SetBuff(SkillStates.Attack, TimeSpan.FromSeconds(60), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;

                    case Spell.SoulBarrier:
                        spells.SetBuff(SkillStates.SoulBarrier, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 40), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;

                    case Spell.GreaterFortitude:
                        spells.SetBuff(SkillStates.SwellLife, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 10), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    default:
                        @char.Spells.AttackSend(spell.Number, message.Target, false);
                        break;
                }

                @char.Mana = mana;
                DamageType type = DamageType.Regular;
                var attack = 0.0f;

                switch(spell.Number)
                {
                    case Spell.Falling_Slash:
                    case Spell.Lunge:
                    case Spell.Uppercut:
                    case Spell.Cyclone:
                    case Spell.Slash:
                    case Spell.TwistingSlash:
                    case Spell.RagefulBlow:
                    case Spell.DeathStab:
                    case Spell.CrescentMoonSlash:
                    case Spell.Impale:
                    case Spell.FireBreath:
                        if (monster != null)
                            attack = @char.SkillAttack(spell, monster, out type);
                        //else
                        //    @char.SkillAttack(spell, player, out type);
                        break;
                    default:
                        if(@char.BaseClass == HeroClass.Summoner || @char.BaseClass == HeroClass.DarkWizard || @char.BaseClass == HeroClass.MagicGladiator)
                        {
                            if (monster != null)
                                attack = @char.MagicAttack(spell, monster, out type);
                        }
                        else
                        {
                            if (monster != null)
                                attack = @char.SkillAttack(spell, monster, out type);
                        }
                        break;
                }

                monster?.GetAttackedDelayed(@char.Player, (int)attack, type, TimeSpan.FromMilliseconds(500));
            }
        }

        [MessageHandler(typeof(CWarp))]
        public async Task CWarp(GSSession session, CWarp message)
        {
            var gates = ResourceCache.Instance.GetGates();

            var gate = (from g in gates
                        where g.Value.GateType != GateType.Exit && g.Value.Move == message.MoveNumber
                        select g.Value).FirstOrDefault();

            if (gate == null)
            {
                Logger.ForAccount(session)
                    .Error("Invalid Gate {0}", message.MoveNumber);

                await session.SendAsync(new SNotice(NoticeType.Blue, "You can't go there"));
                return;
            }

            var @char = session.Player.Character;

            if(gate.ReqLevel > @char.Level)
            {
                Logger.ForAccount(session)
                .Error("Level too low");

                await session.SendAsync(new SNotice(NoticeType.Blue, $"Try again at Level {gate.ReqLevel}"));
                return;
            }

            if(gate.ReqZen > @char.Money)
            {
                Logger.ForAccount(session)
                .Error("Money too low");

                await session.SendAsync(new SNotice(NoticeType.Blue, $"Try again with more Zen"));
                return;
            }

            @char.Money -= gate.ReqZen;

            await @char.WarpTo(gate.Number);
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

        [MessageHandler(typeof(CChaosBoxItemMixButtonClick))]
        public async Task CChaosBoxItemMixButtonClick(GSSession session)
        {
            var @char = session.Player.Character;
            var cbMix = @char.Inventory.ChaosBox;
            
            var mixInfo = ResourceCache.Instance.GetChaosMixInfo();
            var mixMatched = mixInfo.FindMix(@char);

            if (mixMatched == null)
            {
                Logger.ForAccount(session)
                    .Error("No mix found, try to hack?");

                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.Fail });
                return;
            }

            Logger.ForAccount(session)
                .Information("Mix found, match: {0}", mixMatched.Name);

            var result = mixMatched.Execute(@char);

            if(result == ChaosBoxMixResult.Fail)
            {
                await session.SendAsync(new SShopItemList(cbMix.GetInventory()) { ListType = 3 });
            }

            await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = result, ItemInfo = cbMix.Items.Values.FirstOrDefault()?.GetBytes()??Array.Empty<byte>() });
            Logger.ForAccount(session)
                .Information("Mix Result: {0} : {1}", mixMatched.Name, result);
        }

        [MessageHandler(typeof(CChaosBoxUseEnd))]
        public void CChaosBoxUseEnd(GSSession session)
        {
            session.Player.Character.Inventory.Lock = false;
        }

        [MessageHandler(typeof(CWarehouseUseEnd))]
        public void CWarehouseUseEnd(GSSession session)
        {
            session.Player.Character.Inventory.Lock = false;
        }
    }
}
