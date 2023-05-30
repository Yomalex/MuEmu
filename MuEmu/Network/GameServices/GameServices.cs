using MU.DataBase;
using MU.Network;
using MU.Network.Event;
using MU.Network.Game;
using MU.Network.MuunSystem;
using MU.Network.QuestSystem;
using MU.Resources;
using MU.Resources.Game;
using MuEmu.Entity;
using MuEmu.Events.BloodCastle;
using MuEmu.Events.CastleSiege;
using MuEmu.Events.DevilSquare;
using MuEmu.Events.EventChips;
using MuEmu.Events.ImperialGuardian;
using MuEmu.Events.Kanturu;
using MuEmu.Monsters;
using MuEmu.Network.ConnectServer;
using MuEmu.Network.Data;
using MuEmu.Network.UBFSystem;
using MuEmu.Resources;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Util;

namespace MuEmu.Network.GameServices
{
    public partial class GameServices : MessageHandler
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
            var ans = new SAction((ushort)session.ID, message.Dir, message.ActionNumber, message.Target);
            //session.SendAsync(ans).Wait();
            session.Player.SendV2Message(ans);
        } 

        [MessageHandler(typeof(CDataLoadOK))]
        public void CDataLoadOk(GSSession session)
        {
            session.Player.Character.DataLoaded = true;
        }

        [MessageHandler(typeof(CMove))]
        public static async Task CMove(GSSession session, CMove message)
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
            var Cpos = new Point(message.X, message.Y);

            var valid = true;
            byte ld = 0;

            for (int i = 1; i <= count; i++)
            {
                ld = (byte)((message.Path[(i + 1) / 2] >> (((i % 2) == 1) ? 4 : 0)) & 0x07);

                Cpos.Offset(dirs[ld]);
                var att = @char.Map.GetAttributes(Cpos);
                if (att.Where(y => y == MapAttributes.NoWalk || y == MapAttributes.Hide).Count() > 0)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                var msgp = VersionSelector.CreateMessage<SPositionSet>((ushort)session.ID, @char.Position);
                await session.SendAsync(msgp);
                session.Player.SendV2Message(msgp);
                Logger
                    .ForAccount(session)
                    .Error("Invalid path");
                return;
            }

            @char.Position = Cpos;

            var msg = new SMove((ushort)session.ID, (byte)Cpos.X, (byte)Cpos.Y, ld);
            session.Player.SendV2Message(msg);
            session.Player.Character.TPosition = Cpos;
        }

        [MessageHandler(typeof(CMoveEng))]
        public async Task CMoveEng(GSSession session, CMoveEng message)
        {
            await CMove(session, new CMove { Path = message.Path, X = message.X, Y = message.Y });
        }

        [MessageHandler(typeof(CMove12Eng))]
        public async Task CMove12Eng(GSSession session, CMove12Eng message)
        {
            await CMove(session, new CMove { Path = message.Path, X = message.X, Y = message.Y });
        }

        [MessageHandler(typeof(CPositionSet))]
        public async Task CPositionSet(GSSession session, CPositionSet message)
        {
            var pos = message.Position;
            Logger.ForAccount(session).Debug("Position set Recv {0}", pos);
            var @char = session.Player.Character;
            var msg = VersionSelector.CreateMessage<SPositionSet>((ushort)session.ID, pos);
            @char.Position = pos;
            @char.TPosition = pos;
            await session.SendAsync(msg);
            @char.SendV2Message(msg);
        }

        [MessageHandler(typeof(CPositionSetS9))]
        public async Task CPositionSet(GSSession session, CPositionSetS9 message)
        {
            var pos = message.Position;
            //Logger.ForAccount(session).Debug("PositionS9 set Recv {0}", pos);
            var @char = session.Player.Character;
            var msg = new SPositionSetS9Eng((ushort)session.ID, pos);
            @char.Position = pos;
            @char.TPosition = pos;
            await session.SendAsync(msg);
            @char.SendV2Message(msg);
        }

        #region Chat MessageHandlers
        // 0xC1 0x00
        [MessageHandler(typeof(CChatNickname))]
        public async Task CChatNickname(GSSession session, CChatNickname message)
        {
            if (!Program.Handler.ProcessCommands(session, message.Message.MakeString()))
            {
                session.Player.SendV2Message(message);
                await session.SendAsync(message);
            }
        }

        // 0xC1 0x01
        [MessageHandler(typeof(CChatNumber))]
        public async Task CChatNumber(GSSession session, CChatNumber message)
        {
            message.Number = (ushort)session.ID;
            session.Player.SendV2Message(message);
            await session.SendAsync(message);
        }

        // 0xC1 0x02
        [MessageHandler(typeof(CChatWhisper))]
        public async Task CChatWhisper(GSSession session, CChatWhisper message)
        {
            var target = Program.server.Clients
                .Where(x => x.Player != null)
                .Where(x => x.Player.Character != null)
                .FirstOrDefault(x => x.Player.Character.Name.ToLower() == message.Id.ToLower());

            if (target == null)
            {
                await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Chat_Player_Offline, message.Id)));
                return;
            }

            await target.SendAsync(message);
        }
        #endregion

        [MessageHandler(typeof(CCloseWindow))]
        public static void CCloseWindow(GSSession session)
        {
            if (session.Player.Window == null)
                return;

            if (session.Player.Window.GetType() == typeof(Monster))
            {
                var mob = session.Player.Window as Monster;
                if (mob == null)
                    return;

                if (mob.Info.Monster == 229)
                    Marlon.RemRef();

                Logger.Debug("Player close window:NPC{0}", mob.Info.Name);
            } else if (session.Player.Window.GetType() == typeof(Character))
            {
                var @char = session.Player.Window as Character;
                Logger.Debug("Player close window:Character{0}", @char.Name);
            } else
            {
                Logger.Debug("Player close window:{0}", session.Player.Window.GetType());
            }
            session.Player.Window = null;
        }

        [MessageHandler(typeof(CClientClose))]
        public static async Task CClientClose(GSSession session, CClientClose message)
        {
            Logger
                .ForAccount(session)
                .Information(ServerMessages.GetMessage(Messages.Game_Close), message.Type);

            using (var db = new GameContext())
            {
                await session.Player.Save(db);
                await db.SaveChangesAsync();
            }

            for (int i = 1; i <= 5; i++)
            {
                SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5 - i), new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_Close_Message, i)));
            }

            SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5), new SCloseMsg { Type = message.Type });

            Program.client.SendAsync(new SCRem { Server = (byte)Program.ServerCode, List = new CliRemDto[] { new CliRemDto { btName = session.Player.Character?.Name.GetBytes() ?? Array.Empty<byte>() } } });
            session.Player.Status = message.Type switch
            {
                ClientCloseType.SelectChar => LoginStatus.Logged,
                _ => LoginStatus.NotLogged
            };
        }

        [MessageHandler(typeof(CMoveItem))]
        public async Task CMoveItem(GSSession session, CMoveItem message)
        {
            var msg = VersionSelector.CreateMessage<SMoveItem>();
            msg.Set("ItemInfo", message.ItemInfo);
            if (session.Player.Character.Inventory.Move(message.sFlag, message.Source, message.tFlag, message.Dest))
            {
                msg.Set("Position", message.Dest);
                msg.Set("Result", (byte)message.tFlag);
            }
            else
            {
                msg.Set("Position", message.Source);
                msg.Set("Result", (byte)message.sFlag);
            }
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CPointAdd))]
        public async Task CPointAdd(GSSession session, CPointAdd message)
        {
            var @char = session.Player.Character;
            var msg = new SPointAdd
            {
                Result = (byte)(0x10 | (byte)message.Type),
            };

            if (@char.LevelUpPoints == 0)
            {
                msg.Result = 0;
                await session.SendAsync(msg);
                return;
            }

            switch (message.Type)
            {
                case PointAdd.Strength:
                    if (@char.Strength + 1 <= short.MaxValue)
                    {
                        @char.LevelUpPoints--;
                        @char.Strength++;
                        msg.MaxStamina = (ushort)@char.MaxStamina;
                        msg.MaxShield = (ushort)@char.MaxShield;
                    } else
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

            await session.SendAsync(msg);
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public async Task CUseItem(GSSession session, CUseItem message)
        {
            var @char = session.Player.Character;
            var inv = @char.Inventory;

            var Source = inv.Get(message.Source);

            Logger.Debug("CUseItem Source:{0} Target:{1} Type:{2} ItemSource:{3}", message.Source, message.Dest, message.Type, Source);

            if (Source.BasicInfo.Skill != Spell.None)
            {
                if (await @char.Spells.TryAdd(Source.BasicInfo.Skill))
                {
                    await inv.Delete(message.Source);
                }
                @char.HPorSDChanged(RefillInfo.Update);
                return;
            }

            switch (Source.Number)
            {
                case 14 * 512 + 0:// Apple
                case 14 * 512 + 1:// Small HP Potion
                case 14 * 512 + 2:// Medium HP Potion
                case 14 * 512 + 3:// Big HP Potion
                    var AddLife = (Source.SellPrice * 10) - (@char.Level * 2);
                    if (AddLife < 0)
                        AddLife = 0;

                    float AddLifeRate = ((Source.Number.Index + 1) * 10.0f) + (Source.Plus * 5.0f);
                    AddLife += (long)(@char.MaxHealth * AddLifeRate / 100.0f);
                    if (Source.Durability <= 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Health += AddLife;
                    @char.HuntingRecord.HealingUse(AddLife);
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

                    @char.Mana += AddMana;
                    break;
                case 14 * 512 + 8: // Antidote
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;
                    await @char.Spells.ClearBuffByEffect(SkillStates.Ice);
                    await @char.Spells.ClearBuffByEffect(SkillStates.Poison);
                    break;
                case 14 * 512 + 46: // Haloween Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Spells.SetBuff(SkillStates.HAttackSpeed, TimeSpan.FromMilliseconds(1800));
                    break;
                case 14 * 512 + 47: // Haloween Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Spells.SetBuff(SkillStates.HAttackPower, TimeSpan.FromMilliseconds(1800));
                    break;
                case 14 * 512 + 48: // Haloween Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Spells.SetBuff(SkillStates.HDefensePower, TimeSpan.FromMilliseconds(1800));
                    break;
                case 14 * 512 + 49: // Haloween Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Spells.SetBuff(SkillStates.HMaxLife, TimeSpan.FromMilliseconds(1800));
                    break;
                case 14 * 512 + 50: // Haloween Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.Spells.SetBuff(SkillStates.HMaxMana, TimeSpan.FromMilliseconds(1800));
                    break;
                case 14 * 512 + 10: // Town Portal Scroll
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    if (@char.MapID == Maps.Davias)
                    {
                        await @char.WarpTo(22);
                    }
                    else if (@char.MapID == Maps.Noria)
                    {
                        await @char.WarpTo(27);
                    }
                    else if (@char.MapID == Maps.LostTower)
                    {
                        await @char.WarpTo(42);
                    }
                    else if (@char.MapID == Maps.Atlans)
                    {
                        await @char.WarpTo(49);
                    }
                    else if (@char.MapID == Maps.Tarkan)
                    {
                        await @char.WarpTo(57);
                    }
                    else if (@char.MapID == Maps.BloodCastle1)
                    {
                        await @char.WarpTo(22);
                    }
                    else if (@char.MapID == Maps.ChaosCastle1)
                    {
                        await @char.WarpTo(22);
                    }
                    else if (@char.MapID == Maps.Kalima1)
                    {
                        await @char.WarpTo(22);
                    }
                    else if (@char.MapID == Maps.Aida)
                    {
                        await @char.WarpTo(27);
                    }
                    else if (@char.MapID == Maps.Crywolf)
                    {
                        await @char.WarpTo(27);
                    }
                    else
                    {
                        await @char.WarpTo(17);
                    }
                    break;
                case 14 * 512 + 9: // Ale
                    if (Source.Durability <= 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;
                    break;
                case 14 * 512 + 20: // Remedy Of Love
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    await session.SendAsync(new SItemUseSpecialTime { Number = 1, Time = 90 });
                    break;
                case 14 * 512 + 7: // Siege Potion
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    var SS = Source.Plus == 0 ? SkillStates.PotionBless : SkillStates.PotionSoul;
                    var time = TimeSpan.FromSeconds(Source.Plus == 0 ? 120 : 60);
                    @char.Spells.SetBuff(SS, time);
                    if (Source.Plus == 1)
                    {
                        await session.SendAsync(new SItemUseSpecialTime { Number = 2, Time = 60 });
                    }
                    break;
                case 14 * 512 + 63: // Fireworks
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    @char.SendV2Message(new SCommand(ServerCommandType.Fireworks, (byte)@char.Position.X, (byte)@char.Position.Y));
                    break;
                case 14 * 512 + 35:// Small SD Potion
                case 14 * 512 + 36:// Medium SD Potion
                case 14 * 512 + 37:// Big SD Potion
                    float addSDRate = @char.MaxShield * (25.0f + (Source.Number.Index - 35) * 10.0f) / 100.0f;

                    @char.Shield += addSDRate;
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;
                    await session.SendAsync(new SEffect((ushort)session.ID, ClientEffect.RecoverShield));
                    break;
                case 14 * 512 + 13: //  Jewel of Bless
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Plus >= 7)
                            break;

                        var source = inv.Get(message.Source);
                        var left = (byte)Math.Min(6 - Target.Plus, source.Durability);

                        Target.Plus += left;
                        source.Durability -= left;

                        if(source.Durability == 0)
                            await inv.Delete(message.Source);
                    }
                    break;
                case 14 * 512 + 14: //  Jewel of Soul
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Plus >= 9)
                            break;

                        await inv.Delete(message.Source);
                        var soulRate = 50 + (Target.Luck ? 25 : 0);
                        if (Program.RandomProvider(100) < soulRate)
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
                        var lifeRate = 50 + (Target.Luck ? 25 : 0);
                        if (Program.RandomProvider(100) < lifeRate)
                        {
                            Target.Option28++;
                        }
                        else
                        {
                            Target.Option28--;
                        }

                    }
                    break;
                case 7210:// Jewel of Harmony
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Harmony.Option != 0)
                            break;

                        var joh = ResourceCache.Instance.GetJOH();

                        Target.Harmony = new JewelOfHarmony();
                        Target.Harmony.Item = Target;
                        switch (Target.Harmony.Type)
                        {
                            case 1:
                                Target.Harmony.Option = (byte)Program.RandomProvider(joh.Weapon.Length + 1, 1);
                                break;
                            case 2:
                                Target.Harmony.Option = (byte)Program.RandomProvider(joh.Staff.Length + 1, 1);
                                break;
                            case 3:
                                Target.Harmony.Option = (byte)Program.RandomProvider(joh.Defense.Length + 1, 1);
                                break;
                            default:
                                return;
                        }

                        Logger.ForAccount(session).Information("Item {0} added JOH Option {1}", Target, Target.Harmony.EffectName);
                        Target.OnItemChange();
                        await inv.Delete(message.Source);
                    }
                    break;
                case 7211: // Lower Refining Stone
                case 7212: // Higher Refining Stone
                    inv.Get(message.Dest).Harmony.UseRefiningStone(Source);
                    break;
                case 13 * 512 + 66: //Invitation of the Santa Town's

                    break;
                case 14 * 512 + 29: // Symbol of Kundun
                    {
                        var Target = inv.Get(message.Dest);
                        if (Target.Number.Number != 7197 || Target.Plus != Source.Plus)
                            return;

                        if (Target.Durability + Source.Durability <= 5)
                        {
                            Target.Durability += Source.Durability;
                        } else
                        {
                            Source.Durability -= (byte)(5 - Target.Durability);
                        }
                        var LostMap = new Item(new ItemNumber(7196), new { Source.Plus });
                        await inv.Delete(message.Dest);
                        inv.Add(LostMap);
                    }
                    break;
            }
        }

        private static Item[] ExecuteBag(Player plr, Item item)
        {
            var itemBags = ResourceCache.Instance.GetItemBags();
            var bag = (from b in itemBags
                       where b.Number == item.Number && (b.Plus == item.Plus || b.Plus == 0xffff)
                       select b).FirstOrDefault();

            if (bag != null)
            {
                if (bag.LevelMin <= plr.Character.Level)
                {
                    return bag.GetReward();
                }
            }
            return null;
        }

        [MessageHandler(typeof(CItemThrow))]
        public static async Task CItemThrow(GSSession session, CItemThrow message)
        {
            var logger = Logger.ForAccount(session);
            var plr = session.Player;
            var inv = plr.Character.Inventory;
            var item = inv.Get(message.Source);
            if (item == null)
                return;
            DateTimeOffset date = DateTimeOffset.Now;

            var bag = ExecuteBag(plr, item);
            if (bag != null)
            {
                foreach (var reward in bag)
                    date = plr.Character.Map.AddItem(message.MapX, message.MapY, reward, plr.Character);

                var msg = new SCommand(ServerCommandType.Fireworks, (byte)plr.Character.Position.X, (byte)plr.Character.Position.X);
                await plr.Session.SendAsync(msg);
                plr.SendV2Message(msg);
                await inv.Delete(message.Source);
            }
            else
            {
                switch (item.Number.Number)
                {
                    case 7196://lost map
                        logger.Information("[Kalima] Try to Create Kalima Gate");
                        if (plr.Character.Map.IsEvent)
                        {
                            logger.Error("[Kalima] Failed to Summon Kalima Gate - Called in {0}", plr.Character.MapID);
                            break;
                        }
                        if (plr.Character.Map.GetAttributes(message.MapX, message.MapY).Contains(MapAttributes.Safe))
                        {
                            logger.Error("[Kalima] Failed to Summon Kalima Gate - Called in Saftey Area (Map:{0}, X:{1}, Y:{2})", plr.Character.MapID, message.MapX, message.MapY);
                            break;
                        }
                        if (plr.Character.KalimaGate != null)
                        {
                            logger.Error("[Kalima] Failed to Summon Kalima Gate - Already Have Gate (SummonIndex:{0})", plr.Character.KalimaGate.Index);
                            break;
                        }
                        var minLevel = new List<int> { 0, 40, 131, 181, 231, 281, 331, 380 };
                        if (minLevel[item.Plus] > plr.Character.Level)
                        {
                            logger.Error("[Kalima] Failed to Summon Kalima Gate - Min Level: {0}, Player Level: {1}", minLevel[item.Plus], plr.Character.Level);
                            break;
                        }
                        plr.Character.KalimaGate = MonstersMng.Instance.CreateMonster((ushort)(151 + item.Plus), ObjectType.NPC, plr.Character.MapID, new Point(message.MapX, message.MapY), 1);
                        plr.Character.KalimaGate.Caller = plr;
                        plr.Character.KalimaGate.Params = 5;
                        await inv.Delete(message.Source);

                        goto throwItem;
                }
                date = await item.Drop(message.MapX, message.MapY);
            }
        throwItem:
            await session.SendAsync(new SItemThrow { Source = message.Source, Result = 1 });

            logger.Information("Drop item {0} at {1},{2} in {3} deleted at {4}", item, message.MapX, message.MapY, plr.Character.MapID, date);
        }

        [MessageHandler(typeof(CItemGet))]
        public static async Task CItemGet(GSSession session, CItemGet message)
        {
            var @char = session.Player.Character;
            byte pos = 0xff;
            Item pickup;

            try
            {
                pickup = @char.Map.ItemPickUp(@char, message.Number);
            }
            catch(Exception ex)
            {
                var msgex = VersionSelector.CreateMessage<SItemGet>((byte)0xff, Array.Empty<byte>(), message.Number);
                session.SendAsync(msgex).Wait();
                session.Exception(ex);
                return;
            }

            if(pickup.IsZen)
            {
                if(session.Player.Character.Money == uint.MaxValue)
                {
                    var msgex = VersionSelector.CreateMessage<SItemGet>((byte)0xff, Array.Empty<byte>(), message.Number);
                    session.SendAsync(msgex).Wait();
                    return;
                }

                if (session.Player.Character.Money <= uint.MaxValue - pickup.BuyPrice)
                    session.Player.Character.Money += pickup.BuyPrice;
                else
                    session.Player.Character.Money = uint.MaxValue;
            }
            else
            {
                switch(pickup.BasicInfo.Inventory)
                {
                    case StorageID.Inventory:
                        pos = @char.Inventory.Add(pickup);
                        //pickup = @char.Inventory.Get(pos);
                        var msg = VersionSelector.CreateMessage<SItemGet>(pos, pickup?.GetBytes() ?? Array.Empty<byte>(), message.Number);
                        await session.SendAsync(msg);
                        break;
                    case StorageID.EventInventory:
                        pos = @char.Inventory.AddEvent(pickup);
                        //pickup = @char.Inventory.GetEvent(pos);
                        await session.SendAsync(VersionSelector.CreateMessage<SEventItemGet>(pos, pickup?.GetBytes() ?? Array.Empty<byte>(), message.Number));
                        break;
                    case StorageID.MuunInventory:
                        pos = @char.Inventory.AddMuun(pickup);
                        //pickup = @char.Inventory.GetMuun(pos);
                        break;
                }
            }

            var msg2 = @char.Map.ItemGive(message.Number);
            _=session.SendAsync(msg2);
            session.Player.SendV2Message(msg2);
        }

        [MessageHandler(typeof(CEventEnterCount))]
        public async Task CEventEnterCount(GSSession session, CEventEnterCount message)
        {
            await session.SendAsync(new SEventEnterCount { Type = message.Type, Left = 10 });
        }

        [MessageHandler(typeof(CTalk))]
        public async Task CTalk(GSSession session, CTalk message)
        {
            var npcs = ResourceCache.Instance.GetNPCs();
            var ObjectIndex = message.Number.ShufleEnding();
            var obj = MonstersMng.Instance.GetMonster(ObjectIndex);
            var @char = session.Player.Character;

            if (obj == null)
                return;

            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                session.Player.Window = obj;
                switch (npc.Class)
                {
                    case NPCAttributeType.ShopRuud:
                        if (session.Player.Character.PKLevel >= PKLevel.Warning2)
                        {
                            await session.SendAsync(new SChatTarget(ObjectIndex, "I don't sell to killers"));
                            break;
                        }

                        if (npc.Data == 0xffff)
                            break;

                        var baseClass = (byte)session.Player.Character.BaseClass;
                        baseClass >>= 4;
                        var shop = ResourceCache.Instance.GetShops()[(ushort)(npc.Data + baseClass)];

                        await session.SendAsync(new STalk { Result = NPCWindow.RuudShop });
                        await session.SendAsync(new SShopItemList(shop.Storage.GetInventory()) { ListType = 0x17 });
                        await session.SendAsync(new SMonsterSoulShop { Result = 1 });
                        await session.SendAsync(new SMonsterSoulAvailableShop { Amount = 1 });
                        break;
                    case NPCAttributeType.Shop:
                        if(session.Player.Character.PKLevel >= PKLevel.Warning2)
                        {
                            if(npc.NPC == 254)
                            {
                                var left = (session.Player.Character.PKTimeEnds - DateTime.Now);
                                _= session.SendAsync(new SChatTarget(ObjectIndex, $"You have {left.Hours} hour {left.Minutes} minutes left on the outlaw status.")); 
                            }
                            else{
                                await session.SendAsync(new SChatTarget(ObjectIndex, "I don't sell to killers"));
                            }
                            
                            break;
                        }

                        if (npc.Data == 0xffff)
                            break;
                        await session.SendAsync(new STalk { Result = NPCWindow.Shop });
                        await session.SendAsync(new SShopItemList(npc.Shop.Storage.GetInventory()) { ListType = 0 });
                        await session.SendAsync(new STax { Type = TaxType.Shop, Rate = 4 });
                        break;
                    case NPCAttributeType.Warehouse:
                        session.Player.Character.Inventory.Lock = true;
                        await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_Vault_active, session.Player.Account.ActiveVault + 1)));
                        await session.SendAsync(new STalk { Result = NPCWindow.Warehouse });
                        await session.SendAsync(new SShopItemList(session.Player.Account.Vault.GetInventory()));
                        await session.SendAsync(new SWarehouseMoney(true, session.Player.Account.VaultMoney, session.Player.Character.Money));
                        break;
                    case NPCAttributeType.GuildMaster:
                        GuildManager.NPCTalk(session.Player, obj);
                        break;
                    case NPCAttributeType.EventChips:
                        EventChips.NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.MessengerAngel:
                        Program.EventManager.GetEvent<BloodCastles>()
                            .MessengerAngelTalk(session.Player);
                        break;
                    case NPCAttributeType.KingAngel:
                        Program.EventManager.GetEvent<BloodCastles>()
                            .AngelKingTalk(session.Player);
                        break;
                    case NPCAttributeType.Kanturu:
                        Program.EventManager
                            .GetEvent<Kanturu>()
                            .NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.DevilSquare:
                        Program.EventManager
                            .GetEvent<DevilSquares>()
                            .NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.ServerDivision:
                        await session.SendAsync(new SCommand(ServerCommandType.ServerDivision));
                        break;
                    case NPCAttributeType.Window:
                        await session.SendAsync(new STalk { Result = (NPCWindow)npc.Data });

                        switch ((NPCWindow)npc.Data)
                        {
                            case NPCWindow.ChaosMachine:
                                session.Player.Character.Inventory.Lock = true;
                                break;
                            case NPCWindow.GateKeeper:
                                DuelSystem.NPCTalk(session.Player);
                                break;
                        }
                        break;
                    case NPCAttributeType.Talk:
                        await session.SendAsync(new SNPCDialog { Contribution = 0, NPC = npc.NPC });
                        break;
                    case NPCAttributeType.Buff:
                        @char.Spells.SetBuff((SkillStates)npc.Data, TimeSpan.FromSeconds(120));
                        break;
                    case NPCAttributeType.Quest:
                        var quest = @char.Quests.Find(obj.Info.Monster);

                        if (quest == null)
                        {
                            await session.SendAsync(new SChatTarget(ObjectIndex, ServerMessages.GetMessage(Messages.Game_NoQuestAvailable)));
                            return;
                        }

                        var details = quest.Details;
                        Logger.ForAccount(session)
                            .Information("Talk to QuestNPC: {0}, Found Quest:{1}, State:{2}", obj.Info.Name, details.Name, quest.State);
                        await session.SendAsync(new SMonsterKillS16 { 
                            QuestId = (byte)quest.Index, 
                            KillCount = quest
                            .GetKillCount()
                            .Select(x => new MonsterKillCountDto { Monster = x.Key, Count = x.Value })
                            .ToArray()
                        });
                        await session.SendAsync(new SSetQuest { Index = (byte)quest.Index, State = quest.StateByte });
                        break;
                    case NPCAttributeType.Gens:
                        @char.Gens.NPCTalk(npc.NPC);
                        break;
                    case NPCAttributeType.CastleSiege:
                        Program.EventManager
                            .GetEvent<CastleSiege>()
                            .NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.CastleSiegeCrown:
                        Program.EventManager
                            .GetEvent<CastleSiege>()
                            .CrownTalk(session.Player);
                        break;
                    case NPCAttributeType.CastleSiegeCrownSwitch:
                        Program.EventManager
                            .GetEvent<CastleSiege>()
                            .CrownSwitchTalk(obj, session.Player);
                        break;
                    case NPCAttributeType.MossMerchant:
                        MossMerchant.Talk(session.Player);
                        break;
                }
            }
            else
            {
                Logger.ForAccount(session)
                    .Debug("Talk to unasigned NPC {0}", obj.Info.Monster);
                await session.SendAsync(new SChatTarget(ObjectIndex, ServerMessages.GetMessage(Messages.Game_DefaultNPCMessage)));
            }
        }

        [MessageHandler(typeof(CBuy))]
        public async Task CBuy(GSSession session, CBuy message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            var bResult = new SBuy
            {
                Result = 0xff,
                ItemInfo = Array.Empty<byte>(),
            };

            if (plr.Window == null)
            {
                await session.SendAsync(bResult);
                throw new ArgumentException("Player isn't in buy/trade/box/Quest", nameof(session.Player.Window));
            }

            if (plr.Window.GetType() != typeof(Monster))
            {
                await session.SendAsync(bResult);
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var npcs = ResourceCache.Instance.GetNPCs();
            var obj = plr.Window as Monster;
            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                if (npc.Class != NPCAttributeType.Shop)
                {
                    await session.SendAsync(bResult);
                    throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
                }

                var item = npc.Shop.Storage.Items[message.Position].Clone() as Item;
                
                bResult.ItemInfo = item.GetBytes();

                session.SendAsync(new SNotice(NoticeType.Blue, $"Item: {item.BasicInfo.Name} Price: {item.BuyPrice}zen")).Wait();

                if (item.BuyPrice > @char.Money)
                {
                    await session.SendAsync(bResult);
                    return;
                }

                bResult.Result = @char.Inventory.Add(item);
                if (bResult.Result == 0xff)
                {
                    await session.SendAsync(bResult);
                    return;
                }

                @char.Money -= item.BuyPrice;

                Logger
                    .ForAccount(session)
                    .Information("Buy {0} for {1}", item.ToString(), item.BuyPrice);

                await session.SendAsync(bResult);
            }
        }

        [MessageHandler(typeof(CSell))]
        public async Task CSell(GSSession session, CSell message)
        {
            var plr = session.Player;
            var result = new SSell { Result = 0, Money = session.Player.Character.Money };

            if (plr.Window == null)
            {
                await session.SendAsync(result);
                throw new ArgumentException("Player isn't in buy/trade/box", nameof(session.Player.Window));
            }

            if (plr.Window.GetType() != typeof(Monster))
            {
                await session.SendAsync(result);
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var npcs = ResourceCache.Instance.GetNPCs();
            var obj = plr.Window as Monster;
            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                if (npc.Class == NPCAttributeType.Shop)
                {
                    result.Result = 1;
                    var inve = plr.Character.Inventory;
                    var item = inve.Get(message.Position);

                    if(
                        item.ExcellentCount > 0 || 
                        item.Plus > 7 || 
                        item.Number.Number == 6159 || //Chaos
                        (item.Number >= 6174 && item.Number <= 6175) || //Compressed
                        (item.Number >= 6204 && item.Number <= 6209) || //Seed()
                        (item.Number >= 6288 && item.Number <= 6345) || item.WingType != 0
                        )
                    {
                        using (var db = new GameContext())
                        {
                            db.Sell.Add(new SellDto
                            {
                                CharacterId = plr.Character.Id,
                                Count = 1,
                                Date = DateTime.Now.AddDays(1),
                                Item = item.GetBytes(),
                                Price = (int)(item.SellPrice*1.1f)
                            });
                            db.SaveChanges();
                        }
                    }

                    plr.Character.Money += item.SellPrice;
                    result.Money = session.Player.Character.Money;
                    await inve.Delete(item, false);
                    await session.SendAsync(new SNotice(NoticeType.Blue, $"Item: {item.BasicInfo.Name} Price: {item.SellPrice}zen"));
                }
            }
            await session.SendAsync(result);
        }

        [MessageHandler(typeof(CWarp))]
        public async Task CWarp(GSSession session, CWarp message)
        {
            if (session.Player.Character.Duel != null)
                return;

            var gates = ResourceCache.Instance.GetGates();

            var gate = (from g in gates
                        where /*g.Value.GateType != GateType.Exit &&*/ g.Value.Move == message.MoveNumber
                        select g.Value).FirstOrDefault();

            var log = Logger.ForAccount(session);

            if (gate == null)
            {
                log.Error("Invalid Gate {0}", message.MoveNumber);

                await session.SendAsync(new SNotice(NoticeType.Blue, "You can't go there"));
                return;
            }

            log.Information("Warp request to {0}", gate.Name);
            var @char = session.Player.Character;

            if (gate.ReqLevel > @char.Level)
            {
                await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_Warp, gate.ReqLevel)));
                return;
            }

            var moneyRequ = gate.ReqZen * ((@char.PKLevel < PKLevel.Warning) ? 1u : 50u);

            if (moneyRequ > @char.Money)
            {
                await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_Warp, gate.ReqZen)));
                return;
            }

            @char.Money -= moneyRequ;
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
                session.SendAsync(new SJewelMix(0)).Wait();
                return;
            }

            if (result.Count() < neededJewels[message.JewelMix][0])
            {
                Logger.ForAccount(session)
                    .Error("JewelMix Insuficient Jewel count: {0} < {1}", result.Count(), neededJewels[message.JewelMix][0]);
                session.SendAsync(new SJewelMix(0)).Wait();
                return;
            }

            if (@char.Money < neededJewels[message.JewelMix][1])
            {
                Logger.ForAccount(session)
                    .Error("JewelMix Insuficient Money: {0} < {1}", @char.Money, neededJewels[message.JewelMix][1]);
                session.SendAsync(new SJewelMix(8)).Wait();
                return;
            }

            foreach (var i in result.Take(neededJewels[message.JewelMix][0]))
            {
                @char.Inventory.Delete(i).Wait();
            }

            @char.Inventory.Add(new Item(new ItemNumber(12, (ushort)(30 + message.JewelType)), new { Plus = message.JewelMix }));
            @char.Inventory.SendInventory();
            session.SendAsync(new SJewelMix(1)).Wait();
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
                session.SendAsync(new SJewelMix(4)).Wait();
                return;
            }

            if (target.Plus != message.JewelLevel)
            {
                Logger.ForAccount(session)
                    .Error("Item level no match: {0} != {1}", message.JewelLevel, target.Plus);
                session.SendAsync(new SJewelMix(3)).Wait();
                return;
            }

            if (@char.Money < 1000000)
            {
                Logger.ForAccount(session)
                    .Error("Insuficient money: {0} < 1000000", @char.Money);
                session.SendAsync(new SJewelMix(8)).Wait();
                return;
            }

            for (var i = 0; i < neededJewels[message.JewelLevel][0]; i++)
            {
                @char.Inventory.Add(new Item(new ItemNumber(14, (ushort)(13 + message.JewelType)), 0));
            }

            @char.Inventory.Delete(message.JewelPos).Wait();
            @char.Inventory.SendInventory();
            session.SendAsync(new SJewelMix(7)).Wait();
        }

        [MessageHandler(typeof(CChaosBoxItemMixButtonClick))]
        public async Task CChaosBoxItemMixButtonClick(GSSession session, CChaosBoxItemMixButtonClick message)
        {
            var @char = session.Player.Character;
            var cbMix = @char.Inventory.ChaosBox;

            var mixInfo = ResourceCache.Instance.GetChaosMixInfo();
            var mixMatched = mixInfo.FindMix(@char);

            Logger.Debug("Client aditional info {0},{1}", message.Type, message.Info);

            if (mixMatched == null)
            {
                Logger.ForAccount(session)
                    .Error("Invalid MIX");

                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.IncorrectItems, ItemInfo = Array.Empty<byte>() });
                return;
            }

            Logger.ForAccount(session)
                .Information("Mix found, match: {0}", mixMatched.Name);

            if (!@char.Inventory.TryAdd())
            {
                await session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_ChaosBoxMixError)));
                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.TooManyItems, ItemInfo = Array.Empty<byte>() });
                return;
            }

            var result = mixMatched.Execute(@char, message);

            if (result != ChaosBoxMixResult.Success)
            {
                await session.SendAsync(new SShopItemList(cbMix.GetInventory()) { ListType = 3 });
            }

            await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = result, ItemInfo = cbMix.Items.Values.FirstOrDefault()?.GetBytes() ?? Array.Empty<byte>() });
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

        [MessageHandler(typeof(CWarehouseMoney))]
        public async Task CWarehouseMoney(GSSession session, CWarehouseMoney message)
        {
            var r = false;
            if (message.Type == 0x00)
            {
                if (session.Player.Character.Money >= message.Money)
                {
                    session.Player.Account.VaultMoney += (int)message.Money;
                    session.Player.Character.Money -= message.Money;
                    r = true;
                }
            }
            else
            {
                if (session.Player.Account.VaultMoney >= message.Money)
                {
                    session.Player.Account.VaultMoney -= (int)message.Money;
                    session.Player.Character.Money += message.Money;
                    r = true;
                }
            }
            await session.SendAsync(new SWarehouseMoney(r, session.Player.Account.VaultMoney, session.Player.Character.Money));
        }

        [MessageHandler(typeof(CItemModify))]
        public async Task CItemModify(GSSession session, CItemModify message)
        {
            if (message.Position == 0xff)
            {
                session.Player.Character.Inventory
                    .MainInventory()
                    .ForEach(x => CItemModify(session, new CItemModify { Position = (byte)x.SlotId }));
                return;
            }

            var it = session.Player.Character.Inventory.Get(message.Position);
            var res = new SItemModify
            {
                Money = 0,
            };

            var cost = it.RepairPrice;

            if (cost <= 0 || cost > session.Player.Character.Money)
            {
                session.SendAsync(res).Wait();
                return;
            }

            await session.SendAsync(new SNotice(NoticeType.Blue, $"Item:{it.BasicInfo.Name} Repair:{it.RepairPrice}zen"));

            it.Durability = it.DurabilityBase;
            session.Player.Character.Money -= (uint)cost;
            res.Money = (int)session.Player.Character.Money;
            await session.SendAsync(res);
        }

        [MessageHandler(typeof(CTeleportS9))]
        public async Task CTeleportS9(GSSession session, CTeleportS9 message)
        {
            await CTeleport(session, new CTeleport { MoveNumber = message.MoveNumber, X = message.X, Y = message.Y });
        }

        [MessageHandler(typeof(CTeleport))]
        public async Task CTeleport(GSSession session, CTeleport message)
        {
            var log = Logger.ForAccount(session);
            var @char = session.Player.Character;

            var gates = ResourceCache.Instance.GetGates();

            if (message.MoveNumber != 0)
            {
                int target = message.MoveNumber;
                Gate gate = null;

                if (!gates.TryGetValue(target, out gate))
                {
                    log.Error("Invalid source gate {0}", message.MoveNumber);
                    await @char.WarpTo(@char.MapID, @char.Position, @char.Direction);
                    return;
                }

                if (gate.GateType == GateType.Entrance)
                {
                    target = gate.Target;
                    gate = null;
                    gates.TryGetValue(target, out gate);
                }

                if (gate == null)
                {
                    log.Error("Invalid source gate {0}", message.MoveNumber);
                    await @char.WarpTo(@char.MapID, @char.Position, @char.Direction);
                    return;
                }

                var ev = Program.EventManager.GetEvent<ImperialGuardian>();
                if (gate.Map == ev.Map)
                {
                    await ev.UsePortal(@char, (ushort)target);
                }
                else
                {
                    log.Information("Warp request to {1}:{0}", target, gate.Map);
                    await @char.WarpTo(target);
                }
            }
            else
            {
                var spell = ResourceCache.Instance.GetSkills()[Spell.Teleport];

                if (spell.Mana < @char.Mana && spell.BP < @char.Stamina)
                {
                    var msg = VersionSelector.CreateMessage<SMagicAttack>(Spell.Teleport, (ushort)session.ID, (ushort)session.ID);

                    await session.SendAsync(msg);
                    @char.SendV2Message(msg);

                    @char.Mana -= spell.Mana;
                    @char.Stamina -= spell.BP;

                    @char.PlayersVP.Clear();
                    @char.MonstersVP.Clear();
                    @char.ItemsVP.Clear();
                    @char.TeleportTo(new Point(message.X, message.Y));
                }


                //await @char.WarpTo(@char.MapID, @char.Position, @char.Direction);
                //@char.TeleportTo(@char.Position);
            }
        }

        #region Duel MessageHandlers
        [MessageHandler(typeof(CDuelRequest))]
        public void CDuelRequest(GSSession session, CDuelRequest message)
        {
            var targetId = message.wzNumber.ShufleEnding();
            var target = Program.server.Clients.FirstOrDefault(x => x.ID == targetId);

            if (session.Player.Character.Duel != null)
            {
                session.SendAsync(new SDuelAnsDuelInvite(DuelResults.AlreadyDuelling, 0, "")).Wait();
                return;
            }
            if (target.Player.Character.Duel != null)
            {
                session.SendAsync(new SDuelAnsDuelInvite(DuelResults.AlreadyDuelling1, 0, "")).Wait();
                return;
            }

            if (!DuelSystem.CreateDuel(session.Player, target.Player))
            {
                session.SendAsync(new SDuelAnsDuelInvite(DuelResults.DuelMax, 0, "")).Wait();
                return;
            }
            target.SendAsync(new SDuelAnswerReq((ushort)session.ID, session.Player.Character.Name)).Wait();
            //session.SendAsync(new SDuelAnsDuelInvite(DuelResults.NoError, 0, "")).Wait();
        }

        [MessageHandler(typeof(CDuelAnswer))]
        public void CDuelAnswer(GSSession session, CDuelAnswer message)
        {
            if (message.DuelOK == 0)
            {
                session.SendAsync(new SDuelAnsDuelInvite(DuelResults.RefuseInvitated, (ushort)session.ID, session.Player.Character.Name)).Wait();
                return;
            }

            session.Player.Character.Duel.Join();
        }

        [MessageHandler(typeof(CDuelLeave))]
        public void CDuelLeave(GSSession session)
        {
            try
            {
                session.Player.Character.Duel.Leave(session.Player);
            } catch (Exception)
            {
                session.SendAsync(new SDuelAnsExit(DuelResults.Failed)).Wait();
            }
        }

        [MessageHandler(typeof(CDuelJoinRoom))]
        public void CDuelJoinRoom(GSSession session, CDuelJoinRoom message)
        {
            var msg = new SDuelRoomJoin();
            msg.Results = DuelSystem.TryJoinRoom(session.Player, message.Room);
            session.SendAsync(msg).Wait();
        }

        [MessageHandler(typeof(CDuelLeaveRoom))]
        public void CDuelLeaveRoom(GSSession session, CDuelLeaveRoom message)
        {
            var msg = new SDuelRoomLeave();
            msg.Results = DuelSystem.LeaveRoom(session.Player, message.Room);
            session.SendAsync(msg).Wait();
        }
        #endregion

        #region MasterSystem
        [MessageHandler(typeof(CMasterSkill))]
        public async Task CMasterSkill(GSSession session, CMasterSkill message)
        {
            var si = ResourceCache.Instance.GetSkills()[message.MasterSkill];
            var @char = session.Player.Character;

            var canUse = si.Classes.Where(x => (x&(HeroClass)(0xF0)) == @char.BaseClass && x <= @char.Class).Any();
            if(!canUse)
            {
                return;
            }

            var skill = MasterLevel.MasterSkillTree
                .Trees
                .Where(x => x.ID == session.Player.Character.Class)
                .SelectMany(y => y.Skill)
                .Where(x => (Spell)x.MagicNumber == message.MasterSkill)
                .FirstOrDefault();

            if(skill.ReqMinPoint > @char.MasterLevel.Points)
            {
                return;
            }

            @char.MasterLevel.Points = (ushort)(@char.MasterLevel.Points-skill.ReqMinPoint);

            var baseSpell = @char
                .Spells
                .SpellList
                .Where(x => x.Number == (Spell)skill.ParentSkill1)
                .FirstOrDefault();

            var spell = @char.Spells.SpellList.Where(x => x.Number == message.MasterSkill).FirstOrDefault();

            if(skill.ParentSkill1 != 0)
            {
                if (baseSpell == null) {
                    Logger.Error("Don't have previus condition to use skill");
                    return;
                }

                //@char.Spells.Remove(baseSpell);
                @char.Spells.SendList();
            }

            if (spell == null)
            {
                if(!await @char.Spells.TryAdd(message.MasterSkill))
                {
                    throw new Exception("Invalid master skill");
                }

                spell = @char.Spells.SpellList.Where(x => x.Number == message.MasterSkill).First();
            }
            else
            {
                spell.Level++;
                @char.Spells.SetEffect(spell);
            }

            var curLevel = spell?.Level ?? 1;

            await session.SendAsync(new SMasterLevelSkillS9ENG
            {
                Result = 1,
                MasterLevelPoint = @char.MasterLevel.Points,
                MasterSkillUIIndex = (byte)skill.Index,
                dwMasterSkillIndex = (int)message.MasterSkill,
                dwMasterSkillLevel = curLevel,
                fMasterSkillCurValue = spell.MLSValue,
                fMasterSkillNextValue = skill.GetValue((short)(curLevel + 1)),
            });

            session.Player.Character.CalcStats();
        }

        #endregion

        [MessageHandler(typeof(CInventory))]
        public async Task CInventory(GSSession session)
        {
            session.Player.Character.Inventory.SendInventory();
            session.Player.Character.Inventory.SendJewelsInfo();
        }

        [MessageHandler(typeof(CTradeRequest))]
        public async Task CTradeRequest(GSSession session, CTradeRequest message)
        {
            var target = Program.server.Clients.FirstOrDefault(x=> x.ID == message.Number);
            var log = Logger.ForAccount(session);

            if (target == null || 
                target.ID == session.ID || 
                session.Player.Status != LoginStatus.Playing ||
                target.Player.Status != LoginStatus.Playing || 
                target.Player.Character.Shop.Open ||
                session.Player.Character.Shop.Open ||
                session.Player.Character.Map.IsEvent ||
                target.Player.Window != null ||
                session.Player.Window != null )
                return;

            await target.SendAsync(new STradeRequest { Id = session.Player.Character.Name });
            session.Player.Window = target;
            target.Player.Window = session;
        }

        [MessageHandler(typeof(CTradeResponce))]
        public async Task CTradeResponce(GSSession session, CTradeResponce message)
        {
            var tgt = session.Player.Window as GSSession;

            if (message.Result == 0)
            {
                session.Player.Window = null;
                tgt.Player.Window = null;
            }else
            {
                session.Player.Character.Inventory.TradeOpen = true;
                tgt.Player.Character.Inventory.TradeOpen = true;
            }

            await session.SendAsync(new STradeResponce { Result = message.Result, szId = tgt.Player.Character.Name.GetBytes(), Level = tgt.Player.Character.Level });
            await tgt.SendAsync(new STradeResponce { Result = message.Result, szId = session.Player.Character.Name.GetBytes(), Level = session.Player.Character.Level });
        }

        [MessageHandler(typeof(CTradeMoney))]
        public async Task CTradeMoney(GSSession session, CTradeMoney message)
        {
            var session2 = (session.Player.Window as GSSession);
            Logger.ForAccount(session).Information("[TRADE] Money set:{0}", message.Money);
            var @char = session.Player.Character;
            var @char2 = session2.Player.Character;
            if (message.Money > @char.Money)
            {
                return;
            }

            var modmoney = message.Money - @char.Inventory.TradeBox.Money;

            @char.Money -= modmoney;
            @char.Inventory.TradeBox.Money += modmoney;
            @char2.Inventory.TradeOk = false;
            await session.SendAsync(new STradeMoney { Result = 1 });
            await session2.SendAsync(new STradeOtherMoney { Money = message.Money });
            await session.SendAsync(new CTradeButtonOk { Flag = 0 });
            await session2.SendAsync(new CTradeButtonOk { Flag = 0 });
        }

        [MessageHandler(typeof(CTradeButtonOk))]
        public async Task CTradeButtonOk(GSSession session, CTradeButtonOk message)
        {
            var session2 = session.Player.Window as GSSession;
            var char1 = session.Player.Character;
            var char2 = session2.Player.Character;
            char1.Inventory.TradeOk = message.Flag == 1;
            await session2.SendAsync(message);

            if (!char1.Inventory.TradeOk || !char2.Inventory.TradeOk)
                return;

            char1.Inventory.TradeOk = false;
            char2.Inventory.TradeOk = false;

            var result = char1.Inventory.TryAdd(char2.Inventory.TradeBox.Items.Values);
            result &= char2.Inventory.TryAdd(char1.Inventory.TradeBox.Items.Values);

            char1.Player.Window = null;
            char2.Player.Window = null;

            if (!result)
            {
                var notice = new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.Game_ChaosBoxMixError));
                await session.SendAsync(notice);
                await session2.SendAsync(notice);

                if (char1.Inventory.TradeBox.Items.Count != 0)
                    char1.Inventory.TradeRollBack();

                if (char2.Inventory.TradeBox.Items.Count != 0)
                    char2.Inventory.TradeRollBack();

                char1.Inventory.SendInventory();
                char2.Inventory.SendInventory();

                var msg = new STradeResult { Result = TradeResult.InventoryFull };
                await session.SendAsync(msg);
                await session2.SendAsync(msg);
                return;
            }

            char1.Inventory.TradeOpen = false;
            char2.Inventory.TradeOpen = false;

            foreach (var it in char2.Inventory.TradeBox.Items.Values)
            {
                char1.Inventory.Add(it);
                if (it.IsPentagramItem || it.IsPentagramJewel)
                    char2.Inventory.TransferPentagram(it, char1.Inventory);
            }

            foreach (var it in char1.Inventory.TradeBox.Items.Values)
            {
                char2.Inventory.Add(it);
                if (it.IsPentagramItem || it.IsPentagramJewel)
                    char1.Inventory.TransferPentagram(it, char2.Inventory);
            }

            char1.Money += char2.Inventory.TradeBox.Money;
            char2.Money += char1.Inventory.TradeBox.Money;
            char1.Inventory.TradeBox.Clear();
            char2.Inventory.TradeBox.Clear();
            char1.Inventory.SendInventory();
            char2.Inventory.SendInventory();

            var msg2 = new STradeResult { Result = TradeResult.Ok };
            await session.SendAsync(msg2);
            await session2.SendAsync(msg2);
        }

        [MessageHandler(typeof(CTradeButtonCancel))]
        public async Task CTradeButtonCancel(GSSession session)
        {
            var session2 = session.Player.Window as GSSession;
            var char1 = session.Player.Character;
            var char2 = session2.Player.Character;

            session.Player.Window = null;
            session2.Player.Window = null;

            char1.Inventory.TradeOk = false;
            char2.Inventory.TradeOk = false;

            if (char1.Inventory.TradeBox.Items.Count != 0)
                char1.Inventory.TradeRollBack();

            if (char2.Inventory.TradeBox.Items.Count != 0)
                char2.Inventory.TradeRollBack();

            char1.Inventory.SendInventory();
            char2.Inventory.SendInventory();

            var msg = new STradeResult { Result = TradeResult.Error };
            await session.SendAsync(msg);
            await session2.SendAsync(msg);
        }

        [MessageHandler(typeof(CFriendAdd))]
        public void CFriendAdd(GSSession session, CFriendAdd message)
        {
            session.Player.Character.Friends
                .AddFriend(message.Name);
        }

        [MessageHandler(typeof(CWaitFriendAddReq))]
        public void CWaitFriendAddReq(GSSession session, CWaitFriendAddReq message)
        {
            session.Player.Character.Friends
                .AcceptFriend(message.Name, message.Result);
        }

        [MessageHandler(typeof(CUsePopUpType))]
        public async Task CUsePopUpType(GSSession session)
        {
            Logger.Information("Req PopUp Type");
            await session.SendAsync(new SUBFPopUpType { Type = 0 });
        }

        [MessageHandler(typeof(CMemberPosInfoStart))]
        public void CMemberPosInfoStart(GSSession session)
        {
            session.Player.Character.Map.SendMinimapInfo(session.Player.Character);
        }

        [MessageHandler(typeof(CMemberPosInfoStop))]
        public void CMemberPosInfoStop(GSSession session)
        {
        }

        [MessageHandler(typeof(CNPCJulia))]
        public void CNPCJulia(GSSession session)
        {
            if(session.Player.Character.MapID == Maps.LorenMarket)
            {
                session.Player.Character.WarpTo(17).Wait();
            }else
            {
                session.Player.Character.WarpTo(333).Wait();
            }
        }

        [MessageHandler(typeof(CMUBotData))]
        public void CMUBotData(GSSession session, CMUBotData message)
        {
            var muhelper = session.Player.Character.MuHelper;
            muhelper.Configuration = message;

            Logger.Debug(message.ToString());
            //message.Data[0x01]: 0x08: Jewel/Gem, 0x40: Zen, 0x10: Set Item, 0x20: Excellent Item, 0x80: Add Extra Item
            //message.Data[0x02]: 0x01-0x08: hunting Range, 0x10-0x80: Optaining Range
            //message.Data[0x04-0x05]: Basic Skill
            //message.Data[0x06-0x07]: Activation Skill
            //message.Data[0x08-0x09]: Delay Time
            //message.Data[0x0A-0x0B]: Activation Skill 2
            //message.Data[0x0C-0x0D]: Delay Time 2
            //message.Data[0x10-0x11]: Buff 1
            //message.Data[0x12-0x13]: Buff 2
            //message.Data[0x14-0x15]: Buff 3
            //message.Data[0x17]: 0x01-0x0A: 100% AutoPotion
            //message.Data[0x18]: 0x01-0x0A: 100% DrainLife
            //message.Data[0x19]: 0x08:Long Distance-C, 0x10:Original Position, 0x01:Enable AutoPotion, 0x04: Enable DrainLife, 0x20: Combo Enabled
            //message.Data[0x1A]: 0x08: Delay Enable, 0x10: Con, 0x04: Buff Duration
            //message.Data[0x1B]: 0x01: Delay Enable 2, 0x02: Con 2, 0x20: Repair, 0x80: Pick Selected items, 0x40: Pick all near items
            //message.Data[0x1C]: 0x40:Use Regular Attack Area, 0x20:Use skill closely, 0x04: Auto accept friend, 0x08: AutoAccept Guild, 0x10: Use elite potion
            //message.Data[0x1D-0x40]
            //message.Data[0x41]: Extra Item 1
            //message.Data[0x51]: Extra Item 2
            //message.Data[0x61]: Extra Item 3
            //message.Data[0x71]: Extra Item 4
            //message.Data[0x81]: Extra Item 5
            //message.Data[0x91]: Extra Item 6
            //message.Data[0xA1]: Extra Item 7
            //message.Data[0xB1]: Extra Item 8
            //message.Data[0xC1]: Extra Item 9
            //message.Data[0xD1]: Extra Item 10
            //message.Data[0xF1]: Extra Item 11
            /*using (var fp = File.Open($"mubot_{session.Player.Account.Nickname}_{DateTime.Now.Ticks}.txt", FileMode.OpenOrCreate))
            {
                fp.Write(message.Data, 0, message.Data.Length);
            }*/
        }

        [MessageHandler(typeof(CMuHelperState))]
        public void CMuHelperState(GSSession session, CMuHelperState message)
        {
            session.Player.Character.MuHelper.Enable(message.State == 0);
        }

        [MessageHandler(typeof(CQuestExp))]
        public void CQuestExp(GSSession session)
        {
            var npc = session.Player.Window as Monster;

            var list = session.Player.Character.Quests.EXPListNPC(npc.Info.Monster);
            _ = session.SendAsync(new SQuestSwitchListNPC
            {
                NPC = npc.Info.Monster,
                QuestList = list.Select(x => (uint)x).ToArray()
            });
        }

        [MessageHandler(typeof(CShadowBuff))]
        public void CShadowBuff(GSSession session)
        {
            var @char = session.Player.Character;
            @char.Spells.SetBuff(SkillStates.ShadowPhantom, TimeSpan.FromSeconds(120));
        }

        [MessageHandler(typeof(CGremoryCaseOpen))]
        public async Task CGremoryCaseOpen(GSSession session)
        {
            session.Player.Character.GremoryCase.SendList();
            await session.SendAsync(new SGremoryCaseOpen { Result = 0 });
        }

        [MessageHandler(typeof(CGremoryCaseOpenS16))]
        public async Task CGremoryCaseOpenS16(GSSession session, CGremoryCaseOpenS16 message)
        {
            session.Player.Character.GremoryCase.SendList();
            await session.SendAsync(new SGremoryCaseOpenS16 { Result = 3 });
        }

        [MessageHandler(typeof(CGremoryCaseUseItem))]
        public async Task CGremoryCaseUseItem(GSSession session, CGremoryCaseUseItem message)
        {
            var @char = session.Player.Character;
            Item item = @char.GremoryCase.GetItem(message.Inventory, message.Serial);
            if(item == null)
            {
                await session.SendAsync(new SGremoryCaseUseItem {
                    Result = SGremoryCaseUseItem.GCResult.DatabaseError
                });
                return;
            }
            byte pos = 0xff;
            switch ((item.BasicInfo.Inventory)
)
            {
                case StorageID.EventInventory:
                    pos = @char.Inventory.AddEvent(item);
                    @char.Inventory.SendEventInventory();
                    break;
                case StorageID.MuunInventory:
                    pos = @char.Inventory.AddMuun(item);
                    @char.Inventory.SendMuunInventory();
                    break;
                case StorageID.PersonalShop:
                    break;
                case StorageID.Inventory:
                default:
                    pos = @char.Inventory.Add(item);
                    @char.Inventory.SendInventory();
                    break;
            }
            await session.SendAsync(new SGremoryCaseUseItem
            {
                Result = pos==0xff ? SGremoryCaseUseItem.GCResult.NotEnoughtSpace : SGremoryCaseUseItem.GCResult.Success,
                Inventory = message.Inventory,
                Item = item.Number.Number,
                Serial = (uint)item.Serial,
                Slot = message.Slot,
            });
            var msg = VersionSelector.CreateMessage<SGremoryCaseDelete>();
            msg.Set("StorageType", message.Inventory);
            msg.Set("ItemNumber", (ushort)message.Item);
            msg.Set("ItemGUID", message.Serial);
            msg.Set("Slot", message.Slot);
            await session.SendAsync(msg);
            
            @char.GremoryCase.RemoveItem(message.Serial);
        }

        [MessageHandler(typeof(CAcheronEnterReq))]
        public async Task CAcheronEnterReq(GSSession session)
        {
            var it = session
                .Player
                .Character
                .Inventory
                .FindAllItems(ItemNumber.FromTypeIndex(13, 146))
                .FirstOrDefault();

            if(it== null)
            {
                await session.SendAsync(new SNeedSpiritMap());
                return;
            }

            await session
                .Player
                .Character
                .Inventory
                .Delete(it);

            await session
                .Player
                .Character
                .WarpTo(417);
        }

        [MessageHandler(typeof(CRefineJewelReq))]
        public async Task CRefineJewelReq(GSSession session, CRefineJewelReq message)
        {
            var plr = session.Player;
            var @char = plr.Character;
            var cbMix = @char.Inventory.ChaosBox;
            var mixInfos = ResourceCache.Instance.GetChaosMixInfo();
            var mixMatched = mixInfos.FindMix(@char);

            Logger.ForAccount(session)
                .Information("Mix Type {0}", message.Type);

            if(mixMatched == null)
            {
                Logger.ForAccount(session)
                    .Error("Invalid MIX");
                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.Fail, ItemInfo = Array.Empty<byte>() });
                return;
            }

            var element = cbMix.Items.Values.First(x => (x.IsPentagramItem || x.IsPentagramJewel) && x.PentagramaMainAttribute != Element.None).PentagramaMainAttribute;
            Logger.ForAccount(session)
                .Information("Mix found, match: {0} {1}", mixMatched.Name, element);

            var result = mixMatched.Execute(@char);

            if (result != ChaosBoxMixResult.Success)
            {
                await session.SendAsync(new SShopItemList(cbMix.GetInventory()) { ListType = 3 });
            }
            else
            {
                var it = cbMix.Items.Values.First();
                it.PentagramaMainAttribute = element;
                it.BonusSocket |= 0x10;

                switch (message.Type)
                {
                    case 3:
                        it.Slots = new SocketOption[1] { SocketOption.SocketWater };
                        break;
                }
            }

            switch (result)
            {
                case ChaosBoxMixResult.Fail:
                    result = ChaosBoxMixResult.PentagramaRefineFail;
                    break;
                case ChaosBoxMixResult.InsufficientMoney:
                    result = ChaosBoxMixResult.PentagramaInsufficientMoney;
                    break;
                case ChaosBoxMixResult.LackingItems:
                    result = ChaosBoxMixResult.PentagramaLackingItems;
                    break;
            }

            Logger.ForAccount(session)
                .Information("Mix Result: {0} : {1}", mixMatched.Name, result);
            await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = result, ItemInfo = cbMix.Items.Values.FirstOrDefault()?.GetBytes() ?? Array.Empty<byte>() });
        }
    
        [MessageHandler(typeof(CPentagramaJewelIn))]
        public async Task CPentagramaJewelIn(GSSession session, CPentagramaJewelIn message)
        {
            var plr = session.Player;
            var @char = plr.Character;
            var inv = @char.Inventory;

            var pItem = inv.Get((byte)message.PentagramPos);
            var pJewel = inv.Get((byte)message.JewelPos);
            var jAtt = pJewel.PentagramaMainAttribute;
            var iAtt = pItem.PentagramaMainAttribute;

            if (!pItem.IsPentagramItem || !pJewel.IsPentagramJewel || jAtt != iAtt)
            {
                await session.SendAsync(new SPentagramJewelIn { Result = 0 });
                return;
            }

            var slot = (pJewel.Number.Index - 221) / 10;

            if (pItem.Slots.Length <= slot || pItem.Slots[slot] != SocketOption.EmptySocket)
                return;

            pItem.Slots[slot] = (SocketOption)session.Player.Character.Inventory.AddPentagramJewel(pItem, pJewel, slot);

            var msg = new SPentagramJewelIn
            {
                Result = 1,
                Info = new PentagramJewelDto
                {
                    MainAttribute = pJewel.BonusSocket,
                    ItemIndex = pJewel.Number.Index,
                    ItemType = (byte)pJewel.Number.Type,
                    JewelIndex = 0,
                    JewelPos = 0,
                    Level = pJewel.Plus,
                    Rank1Level = 0xf,
                    Rank1OptionNum = 0xf,
                    Rank2Level = 0xf,
                    Rank2OptionNum = 0xf,
                    Rank3Level = 0xf,
                    Rank3OptionNum = 0xf,
                    Rank4Level = 0xf,
                    Rank4OptionNum = 0xf,
                    Rank5Level = 0xf,
                    Rank5OptionNum = 0xf,
                }
            };

            var n = 1;
            foreach(var a in pJewel.Slots)
            {
                msg.Info.Set($"Rank{n}OptionNum", (byte)((byte)a & 0x0F));
                msg.Info.Set($"Rank{n}Level", (byte)(((byte)a & 0xF0)>>4));
            }
            await session.SendAsync(msg);
            pItem.OnItemChange();

            await session.SendAsync(new SPentagramJewelInOut
            {
                Result = 1,
            });
        }

        [MessageHandler(typeof(CPetInfo))]
        public async Task CPetInfo(GSSession session, CPetInfo message)
        {
            Item item;

            switch(message.InvenType)
            {
                case 0:// Inventory
                    item = session.Player.Character.Inventory.Get(message.nPos);
                    break;
                case 1:// Warehouse
                    item = session.Player.Account.Vault.Get(message.nPos);
                    break;
                case 2:// Trade
                    item = session.Player.Character.Inventory.TradeBox.Get(message.nPos);
                    break;
                case 3:// Target Trade
                    item = (session.Player.Window as GSSession).Player.Character.Inventory.TradeBox.Get(message.nPos);
                    break;
                case 4:// Chaos
                    item = session.Player.Character.Inventory.ChaosBox.Get(message.nPos);
                    break;
                case 5:// Personal Shop
                    item = session.Player.Character.Inventory.PersonalShop.Get(message.nPos);
                    break;
                default:
                    return;
            }

            if (item.PetLevel == 0)
                item.PetLevel = 1;

            var responce = new SPetInfo
            {
                InvenType = message.InvenType,
                nPos = message.nPos,
                PetType = message.PetType,
                Dur = item.Durability,
                Exp = (int)item.PetEXP,
                Level = item.PetLevel,
            };

            await session.SendAsync(responce);
        }

        [MessageHandler(typeof(CPetCommand))]
        public async Task CPetCommand(GSSession session, CPetCommand message)
        {
            if (session.Player?.Character != null)
            {
                session.Player.Character.PetTarget = message.Number;
                session.Player.Character.PetMode = message.Command;
            }

            Logger.ForAccount(session).Information("Pet mode changed to {0}", message.Command);
            await session.SendAsync(message);
        }

        [MessageHandler(typeof(CInventoryEquipament))]
        public void CInventoryEquipament(GSSession session, CInventoryEquipament message)
        {
            var item = session.Player.Character.Inventory.Get(message.ItemPos);
            if(item.IsMount)
            {
                message.Type = session.Player.Character.ApplyMount(item, message.Type);
            }
            else
            {
                return;
            }

            var itemBytes = item.GetBytes();
            itemBytes[1] = (byte)(message.ItemPos << 4);
            itemBytes[1] |= item.SmallPlus;

            _ = session.SendAsync(message);
            _ = session.SendAsync(new SEquipamentChange
            {
                Element = session.Player.Character.Inventory.Get(Equipament.Pentagrama)?.PentagramaMainAttribute??Element.None,
                ItemInfo = itemBytes,
                wzNumber = ((ushort)session.ID).ShufleEnding()
            });
        }
        [MessageHandler(typeof(CSXInfo))]
        public async Task CSXInfo(GSSession session)
        {
            var @char = session.Player.Character;
            var inv = @char.Inventory;
            await session.SendAsync(new SXElementalData
            {
                AbsorbHP = inv.PentagramAbsorbHP,
                AbsorbShield = inv.PentagramAbsorbShield,
                AddAttackDamage = inv.PentagramAddAttackDamage,
                AddDefense = inv.PentagramAddDefense,
                PVMAbsorbDamage = inv.PentagramAbsorbDamagePVM,
                PVMAttackSuccessRate = inv.PentagramAttackSuccessRatePVM,
                PVPAbsorbDamage = inv.PentagramAbsorbDamagePVP,
                PVPAttackSuccessRate = inv.PentagramAttackSuccessRatePVP,
                Bind = inv.PentagramBind,
                BleedingDamage = inv.PentagramBleedingDamage,
                Blind = inv.PentagramBlind,
                CriticalDamageRate = inv.PentagramCriticalDamageRate,
                Paralyzing = inv.PentagramParalyzing,
                Punish = inv.PentagramPunish,
                PVMDamageMax = inv.PentagramDamageMaxPVM,
                PVMDamageMin = inv.PentagramDamageMinPVM,
                PVMDefense = inv.PentagramDefensePVM,
                PVMDefenseSuccessRate = inv.PentagramDefenseSuccessRatePVM,
                PVMIncreaseDamage = inv.PentagramIncreaseDamagePVM,
                PVPDamageMax = inv.PentagramDamageMaxPVP,
                PVPDamageMin = inv.PentagramDamageMinPVP,
                PVPDefense = inv.PentagramDefensePVM,
                PVPDefenseSuccessRate = inv.PentagramDefenseSuccessRatePVM,
                PVPIncreaseDamage = inv.PentagramIncreaseDamagePVM,
            });
            await session.SendAsync(new SXCharacterInfo
            {
                CriticalDamageRate = inv.CriticalRate,
                ExcellentDamageRate = inv.ExcellentRate,
                Defense = @char.Defense,
                Dex = @char.Agility,
                Vit = @char.Vitality,
                AddDex = @char.AgilityAdd,
                AddStr = @char.StrengthAdd,
                AddVit = @char.VitalityAdd,
                AddEnergy = @char.EnergyAdd,
                AddLeadership = @char.CommandAdd,
                Energy = @char.Energy,
                Leadership = @char.Command,
                Str = @char.Strength,
                fMonsterDieGetHP_info = inv.IncreaseLifeRate,
                fMonsterDieGetMana_info = inv.IncreaseManaRate,
                fWingDamageAbsorb_info = inv.WingDmgAbsorb,
                fWingDamageIncRate_info = inv.WingDmgIncrease,
                DamageReflect = (byte)(inv.Reflect*100.0f),
                SDRecovery = @char.Spells.IncreaseAutoSDRegeneration + inv.IncreaseSDRecovery,
                MoneyAmountDropRate = (ushort)(inv.DropZen*100.0f),
                CriticalDamage = @char.CriticalDamage,
                ExcellentDamage = @char.ExcellentDamage,
                fTripleDamageRationInfo = 0,
                AbsorbLife = 0,
                AbsorbSD = 0,
                SDAttack = 0,
                SDAttack1 = 0,
                BlockRate = 0,
                BPConsumptionRate = 0,
                BPRecovery = 0,
                DamageReduction = 0,
                DefensiveFullHPRestoreRate = 0,
                DefensiveFullMPRestoreRate = 0,
                DoubleDamageRate = 0,
                fMonsterDieGetSD_info = 0,
                FullDamageReflectRate = 0,
                HPRecovery = 0,
                IgnoreDefenseRate = 0,
                IgnoreShieldGaugeRate = 0,
                MPConsumptionRate = 0,
                MPRecovery = 0,
                OffensiveFullSDRestoreRate = 0,
                ParryRate = 0,
                ResistStunRate = 0,
                ShieldDamageReduction = 0,
                SkillDamageBonus = 0,
                StunRate = 0,
                AGUsageRate = 0,
                TripleDamageRate = 0,
                unk3a = 1,
                unk37 = 2,
                unk38 = 3,
                unk39 = 4,
            });
        }

        [MessageHandler(typeof(CFavoritesList))]
        public async Task CFavoritesList(GSSession session, CFavoritesList message)
        {
            using (var db = new GameContext())
            {
                var tmp = new int[5] { -1, -1, -1, -1, -1 };
                message.Region.CopyTo(tmp, 0);
                var fav = db.Favorites.FirstOrDefault(x => x.CharacterId == session.Player.Character.Id);
                bool newRow = false;
                if (fav == null)
                {
                    fav = new FavoritesDto
                    {
                        CharacterId = session.Player.Character.Id,
                    };
                    newRow = true;
                }

                fav.Fav01 = tmp[0];
                fav.Fav02 = tmp[1];
                fav.Fav03 = tmp[2];
                fav.Fav04 = tmp[3];
                fav.Fav05 = tmp[4];

                if (newRow)
                    db.Favorites.Add(fav);
                else
                    db.Favorites.Update(fav);

                await db.SaveChangesAsync();
            }
        }

        [MessageHandler(typeof(COpenBox))]
        public async Task COpenBox(GSSession session, COpenBox message)
        {
            Item it = null;
            var result = new SOpenBox { Result = OBResult.UnableToUse };
            switch(message.type)
            {
                case 0x00:
                    it = session.Player.Character.Inventory.Get(message.Slot);
                    result.Result = OBResult.OKInvent;
                    break;
                case 0x15:
                    it = session.Player.Character.Inventory.GetEvent(message.Slot);
                    result.Result = OBResult.OKEvent;
                    break;
                default:
                    break;
            }

            if(it==null)
            {
                result.Result = OBResult.UnableToUse;
                goto SendResult;
            }

            if(!session.Player.Character.Inventory.TryAdd(new Size(4, 4)))
            {
                result.Result = OBResult.FullInventory;
                goto SendResult;
            }

            var plr = session.Player;
            var bag = ExecuteBag(plr, it);
            if (bag != null)
            {
                var reward = bag.First();
                switch(message.type)
                {
                    case 0x00:
                        await plr.Character.Inventory.Delete(it);
                        break;
                    case 0x15:
                        plr.Character.Inventory.DeleteEvent(message.Slot);
                        plr.Character.Inventory.SendEventInventory();
                        break;
                }
                result.Result = OBResult.OKInvent;
                result.Slot = reward.Number.Number;

                if (!reward.IsZen)
                {
                    if(
                        (reward.Number >= 6868 && reward.Number <= 7005) || 
                        (reward.Number >= 7065 && reward.Number <= 7120) ||
                        (reward.Number >= 7136 && reward.Number <= 7159) ||
                        (reward.Number >= 8192 && reward.Number <= 8695)
                        )
                    {
                        plr.Character.Inventory.AddMuun(reward);
                        plr.Character.Inventory.SendMuunInventory();
                    }
                    else
                    {
                        plr.Character.Inventory.Add(reward);
                    }
                }
                else
                    plr.Character.Money += reward.BuyPrice;

                plr.Character.Inventory.SendInventory();

                var msg = new SCommand(ServerCommandType.Fireworks, (byte)plr.Character.Position.X, (byte)plr.Character.Position.X);
                await plr.Session.SendAsync(msg);
                plr.SendV2Message(msg);
            }else
            {
                result.Result = OBResult.UnableToUse;
            }

        SendResult:
            await session.SendAsync(result);
        }

        [MessageHandler(typeof(CItemSplit))]
        public async Task CItemSplit(GSSession session, CItemSplit message)
        {
            var msg = new SItemSplit();
            Item it = null;

            switch(message.Type)
            {
                case 0:
                    it = session.Player.Character.Inventory.Get(message.Slot);
                    break;
                case 1:
                    it = session.Player.Character.Inventory.GetEvent(message.Slot);
                    break;
                default:
                    await session.SendAsync(msg);
                    return;
            }

            if(it == null || it.Durability <= message.Amount)
            {
                await session.SendAsync(msg);
                return;
            }

            if(!session.Player.Character.Inventory.TryAdd(it.BasicInfo.Size))
            {
                msg.Result = 6;
                await session.SendAsync(msg);
                return;
            }

            it.Durability -= message.Amount;
            session.Player.Character.Inventory.Add(new Item(it.Number, new { Durability = message.Amount, it.Plus }));
            session.Player.Character.Inventory.SendInventory();
        }

        [MessageHandler(typeof(CHuntingRecordRequest))]
        public async Task CHuntingRecordRequest(GSSession session, CHuntingRecordRequest message)
        {
            var target = Program.server.Clients.First(x => x.ID == message.index);
            var hrDay = new SHuntingRecordDay();
            var tHR = target.Player.Character.HuntingRecord;

            var list = tHR.GetRecordList((Maps)message.Map);
            var today = list.SingleOrDefault(x => x.Value.DateTime.Date == DateTime.Now.Date);

            if (tHR.Hunting.Map == message.Map)
            {
                var tod = tHR.Hunting;
                hrDay.Id = (byte)today.Key;
                hrDay.Duration = tod.Duration;
                hrDay.KilledCount = tod.KilledMonsters;
                hrDay.Damage = tod.AttackPVM;
                hrDay.ElementalDamage = tod.ElementalAttackPVM;
                hrDay.Experience = tod.Experience.ShufleEnding();
                hrDay.Level = tod.Level;
                hrDay.SetDT(tod.DateTime);
            }

            await session.SendAsync(hrDay);
            var hrList = list.Select(x => new HuntingRecordListDto
            {
                Damage = x.Value.AttackPVM,
                Duration = (uint)x.Value.Duration,
                ElementalDamage = x.Value.ElementalAttackPVM,
                Experience = (ulong)(x.Value.Experience).ShufleEnding(),
                Healing = (uint)x.Value.HealingUse,
                KilledCount = (uint)x.Value.KilledMonsters,
                Level = x.Value.Level,
                Id = (uint)(x.Key),
                Year = (uint)x.Value.DateTime.Year,
                Month = (byte)x.Value.DateTime.Month,
                Day = (byte)x.Value.DateTime.Day,
            });

            await session.SendAsync(new SHuntingRecordList
            {
                List = hrList.ToArray(),
            });
        }

        [MessageHandler(typeof(CHuntingRecordClose))]
        public void CHuntingRecordClose(GSSession session)
        {
            session.Player.Character.HuntingRecord.Save();
        }

        [MessageHandler(typeof(CHuntingRecordVisibility))]
        public void CHuntingRecordVisibility(GSSession session, CHuntingRecordVisibility message)
        {
            session.Player.Character.HuntingRecord.Visibility = message.Visible == 1;
        }

        [MessageHandler(typeof(CMossMerchantOpenBox))]
        public async Task CMossMerchantOpenBox(GSSession session, CMossMerchantOpenBox message)
        {
            var @char = session.Player.Character;
            if(!@char.Inventory.TryAdd(new Size(2, 4)))
            {
                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.TooManyItems, ItemInfo = Array.Empty<byte>() });
                return;
            }
            switch (message.Section)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    if (@char.Money < 1000000)
                    {
                        Logger.Error("MossMerchantOpenBox 1000000zen > {0}zen", @char.Money);
                        return;
                    }
                    @char.Money -= 1000000;
                    break;
                case 5://Jewel of Bless
                case 8:
                case 9:
                    {
                        var jewelList = @char.Inventory.FindAllItems(7181);
                        if(!jewelList.Any())
                        {
                            Logger.Error("MossMerchantOpenBox needs 1JOB");
                            return;
                        }

                        var jewel = jewelList.First();
                        if(jewel.Durability > 1)
                        {
                            jewel.Durability--;
                        }
                        else
                        {
                            @char.Inventory.Delete(jewel);
                        }
                    }
                    break;
                case 6:// Jewel of Soul
                    {
                        var jewelList = @char.Inventory.FindAllItems(7182);
                        if (!jewelList.Any())
                        {
                            Logger.Error("MossMerchantOpenBox needs 1JOS");
                            return;
                        }
                        var jewel = jewelList.First();
                        if (jewel.Durability > 1)
                        {
                            jewel.Durability--;
                        }
                        else
                        {
                            @char.Inventory.Delete(jewel);
                        }
                    }
                    break;
                case 7:// Miracle Coin
                    {
                        var miracleCoinList = @char.Inventory.FindAllItems(7581);
                        if (!miracleCoinList.Any())
                        {
                            Logger.Error("MossMerchantOpenBox needs 1MC");
                            return;
                        }
                        var jewel = miracleCoinList.First();
                        if (jewel.Durability > 1)
                        {
                            jewel.Durability--;
                        }
                        else
                        {
                            @char.Inventory.Delete(jewel);
                        }
                    }
                    break;
                case 10://  Miracle Coinx3 && Jewel of Blessx10
                    {
                        var miracleCoinList = @char.Inventory.FindAllItems(7581);
                        var jewelList = @char.Inventory.FindAllItems(7181);
                        if (!jewelList.Any() || jewelList.Sum(x => x.Durability) < 10)
                        {
                            Logger.Error("MossMerchantOpenBox needs 10JOB");
                            return;
                        }
                        if (!miracleCoinList.Any() || miracleCoinList.Sum(x => x.Durability) < 3)
                        {
                            Logger.Error("MossMerchantOpenBox needs 3MC");
                            return;
                        }
                    }
                    break;
            }
            var reward = MossMerchant.Gamble(session.Player, message.Section);
            @char.Inventory.Add(reward);
            @char.Inventory.SendInventory();
            await session.SendAsync(new SMossMerchantOpenBox { ItemInfo = reward.GetBytes() });
        }

        [MessageHandler(typeof(CCancelItemSale))]
        public async Task CCancelItemSale(GSSession session)
        {
            using(var db = new GameContext())
            {
                var list = from s in db.Sell
                           where s.CharacterId == session.Player.Character.Id
                           select s;
                var outputList = new List<CancelItemSaleInfoDto>();

                byte index = 0;
                foreach(var it in list)
                {
                    if(it.Date < DateTime.Now)
                    {
                        db.Sell.Remove(it);
                    }
                    else
                    {
                        outputList.Add(new CancelItemSaleInfoDto
                        {
                            ExpireSec = (uint)(it.Date - DateTime.Now).TotalSeconds,
                            IndexCode = index++,
                            ItemCount = 1,
                            ItemInfo = it.Item,
                            RequireMoney = it.Price,
                        });
                    }
                }

                db.SaveChanges();
                await session.SendAsync(new SCancelItemSaleListS16
                {
                    //Result = 0,
                    ItemList = outputList.Take(5).ToArray()
                });
            }
        }

        [MessageHandler(typeof(CCancelItemSaleItem))]
        public async Task CCancelItemSaleItem(GSSession session, CCancelItemSaleItem message)
        {
            using(var db = new GameContext())
            {
                var list = (from s in db.Sell
                           where s.CharacterId == session.Player.Character.Id
                           select s).ToList();

                var item = list.Take(5).ElementAt(message.IndexCode);
                if (session.Player.Character.Money >= item.Price)
                {
                    if(session.Player.Character.Inventory.Add(new Item(item.Item)) == 0xff)
                    {
                        await session.SendAsync(new SCancelItemSaleResult { Result = 2 });
                        return;
                    }
                    session.Player.Character.Money -= (uint)item.Price;
                    db.Sell.Remove(item);
                    db.SaveChanges();
                    session.Player.Character.Inventory.SendInventory();
                    await CCancelItemSale(session);
                    await session.SendAsync(new SCancelItemSaleResult { Result = 0 });
                }
                else
                {
                    await session.SendAsync(new SCancelItemSaleResult { Result = 1 });
                }
            }
        }

        [MessageHandler(typeof(CChangeSkin))]
        public async Task CChangeSkin(GSSession session, CChangeSkin message)
        {
            session.Player.Character.Change = true;
            session.Player.Character.Transformation = message.Skin == 0;

            SubSystem.SelfUpdate(session.Player.Character);
            SubSystem.PlayerPlrViewport(session.Player.Character.Map, session.Player.Character);
        }

        [MessageHandler(typeof(CRuudBuy))]
        public async Task CRuudBuy(GSSession session, CRuudBuy message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            var bResult = new SRuudBuy { Result = 255 };

            if (plr.Window == null)
            {
                await session.SendAsync(bResult);
                throw new ArgumentException("Player isn't in buy/trade/box/Quest", nameof(session.Player.Window));
            }

            if (plr.Window.GetType() != typeof(Monster))
            {
                await session.SendAsync(bResult);
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var npcs = ResourceCache.Instance.GetNPCs();
            var ruud = npcs.First(x => x.Value.Class == NPCAttributeType.ShopRuud).Value;
            var baseClass = (byte)session.Player.Character.BaseClass;
            baseClass >>= 4;
            var shop = ResourceCache.Instance.GetShops()[(ushort)(ruud.Data + baseClass)];
            var it = shop.Storage.Get(message.Slot).Clone() as Item;

            if(it.BasicInfo.Ruud > @char.Ruud)
            {
                bResult.Result = 252;
                await session.SendAsync(bResult);
                return;
            }

            var result = @char.Inventory.Add(it);
            if(result == 0xff)
            {
                bResult.Result = 251;
                await session.SendAsync(bResult);
                return;
            }

            @char.Ruud -= (uint)it.BasicInfo.Ruud;
            await session.SendAsync(new SBuy { Result = result, ItemInfo = it.GetBytes() });
        }
    }
}
