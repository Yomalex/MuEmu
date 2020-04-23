using MuEmu.Entity;
using MuEmu.Events.BloodCastle;
using MuEmu.Events.EventChips;
using MuEmu.Monsters;
using MuEmu.Network.QuestSystem;
using MuEmu.Resources;
using MuEmu.Util;
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

        // 0xC1 0x00
        [MessageHandler(typeof(CChatNickname))]
        public async Task CChatNickname(GSSession session, CChatNickname message)
        {
            if(!Program.Handler.ProcessCommands(session, message.Message.MakeString()))
            {
                await session.Player.SendV2Message(message);
                await session.SendAsync(message);
            }            
        }

        // 0xC1 0x01
        [MessageHandler(typeof(CChatNumber))]
        public async Task CChatNumber(GSSession session, CChatNumber message)
        {
            message.Number = (ushort)session.ID;
            await session.Player.SendV2Message(message);
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

            if(target == null)
            {
                return;
            }

            await target.SendAsync(message);
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
        public async Task CClinetClose(GSSession session, CClientClose message)
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

            using (var db = new GameContext())
            {
                await session.Player.Save(db);
                await db.SaveChangesAsync();
            }
        }

        [MessageHandler(typeof(CMoveItem))]
        public async Task CMoveItem(GSSession session, CMoveItem message)
        {
            Logger.Debug("Move item {0}:{1} to {2}:{3}", message.sFlag, message.Source, message.tFlag, message.Dest);

            if (session.Player.Character.Inventory.Move(message.sFlag, message.Source, message.tFlag, message.Dest))
            {
                await session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = message.Dest,
                    Result = (byte)message.tFlag
                });
            }
            else
            {
                await session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = 0xff,
                    Result = (byte)message.tFlag
                });
            }
        }

        [MessageHandler(typeof(CPointAdd))]
        public async Task CPointAdd(GSSession session, CPointAdd message)
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

            await session.SendAsync(msg);
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public async Task CUseItem(GSSession session, CUseItem message)
        {
            var @char = session.Player.Character;
            var inv = @char.Inventory;

            var Source = inv.Get(message.Source);

            Logger.Debug("CUseItem {0} {1} {2} {3}", message.Source, message.Dest, message.Type, Source);

            if(Source.BasicInfo.Skill != Spell.None)
            {
                if(await @char.Spells.TryAdd(Source.BasicInfo.Skill))
                {
                    await inv.Delete(message.Source);
                }
                @char.HPorSDChanged(RefillInfo.Update);
                return;
            }

            switch(Source.Number)
            {
                case 12 * 512 + 7:// Orb of Twisting Slash
                    if (await @char.Spells.TryAdd(Spell.TwistingSlash))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 8:// Orb of Healing
                    if (await @char.Spells.TryAdd(Spell.Heal))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 9:// Orb of Greater Defense
                    if (await @char.Spells.TryAdd(Spell.GreaterDefense))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 10:// Orb of Greater Damage
                    if (await @char.Spells.TryAdd(Spell.GreaterDamage))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 11:// Orb of Summoning
                    if (await @char.Spells.TryAdd(Spell.Summon))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 12:// Orb of Rageful Blow
                    if (await @char.Spells.TryAdd(Spell.RagefulBlow))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 13:// Orb of Impale
                    if (await @char.Spells.TryAdd(Spell.Impale))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
                case 12 * 512 + 14:// Orb of Greater Fortitude
                    if (await @char.Spells.TryAdd(Spell.GreaterFortitude))
                    {
                        await inv.Delete(message.Source);
                    }
                    @char.HPorSDChanged(RefillInfo.Update);
                    break;
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

                    @char.Health += AddLife;
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
                    else if (@char.MapID==Maps.ChaosCastle1)
                    {
                        await @char.WarpTo(22);
                    }
                    else if (@char.MapID==Maps.Kalima1)
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
                    if (Source.Durability == 1)
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
                    if(Source.Plus == 1)
                    {
                        await session.SendAsync(new SItemUseSpecialTime { Number = 2, Time = 60 });
                    }
                    break;
                case 14 * 512 + 63: // Fireworks
                    if (Source.Durability == 1)
                        await inv.Delete(message.Source);
                    else
                        Source.Durability--;

                    await session.SendAsync(new SCommand(ServerCommandType.Fireworks, (byte)@char.Position.X, (byte)@char.Position.Y));
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
                        var lifeRate = 50 + (Target.Luck ? 25 : 0);
                        if (new Random().Next(100) < lifeRate)
                        {
                            Target.Option28++;
                        }
                        else
                        {
                            Target.Option28--;
                        }

                    }
                    break;
                case 13 * 512 + 66: //Invitation of the Santa Town's

                    break;
            }
        }

        [MessageHandler(typeof(CItemThrow))]
        public async Task CItemThrow(GSSession session, CItemThrow message)
        {
            var logger = Logger.ForAccount(session);
            var plr = session.Player;
            var inv = plr.Character.Inventory;
            var item = inv.Get(message.Source);
            await inv.Delete(message.Source);

            var date = plr.Character.Map.AddItem(message.MapX, message.MapY, item);
            await session.SendAsync(new SItemThrow { Source = message.Source, Result = 1 });
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
            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                switch(npc.Class)
                {
                    case NPCAttributeType.Shop:
                        if (npc.Data == 0xffff)
                            break;                        
                        session.Player.Window = npc.Shop.Storage;
                        await session.SendAsync(new STalk { Result = 0 });
                        await session.SendAsync(new SShopItemList(npc.Shop.Storage.GetInventory()) { ListType = 0 });
                        await session.SendAsync(new STax { Type = TaxType.Shop, Rate = 4 });                        
                        break;
                    case NPCAttributeType.Warehouse:
                        session.Player.Window = session.Player.Account.Vault;
                        session.Player.Character.Inventory.Lock = true;

                        await session.SendAsync(new SNotice(NoticeType.Blue, $"Active Vault: " + (session.Player.Account.ActiveVault + 1)));
                        await session.SendAsync(new STalk { Result = NPCWindow.Warehouse });
                        await session.SendAsync(new SShopItemList(session.Player.Account.Vault.GetInventory()));
                        await session.SendAsync(new SWarehouseMoney(session.Player.Account.VaultMoney, session.Player.Character.Money));
                        break;
                    case NPCAttributeType.GuildMaster:
                        GuildManager.NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.EventChips:
                        EventChips.NPCTalk(session.Player);
                        break;
                    case NPCAttributeType.MessengerAngel:
                        BloodCastles.MessengerAngelTalk(session.Player);
                        break;
                    case NPCAttributeType.KingAngel:
                        BloodCastles.AngelKingTalk(session.Player);
                        break;
                    case NPCAttributeType.Window:
                        await session.SendAsync(new STalk { Result = (NPCWindow)npc.Data });

                        if ((NPCWindow)npc.Data == NPCWindow.ChaosMachine) // ChaosMachine
                        {
                            session.Player.Character.Inventory.Lock = true;
                            session.Player.Window = session.Player.Character.Inventory.ChaosBox;
                        }
                        break;
                    case NPCAttributeType.Buff:
                        @char.Spells.SetBuff((SkillStates)npc.Data, TimeSpan.FromSeconds(120));
                        break;
                    case NPCAttributeType.Quest:
                        var quest = @char.Quests.Find(obj.Info.Monster);

                        if (quest == null)
                        {
                            await session.SendAsync(new SChatTarget(ObjectIndex, "I don't have Quest for you"));
                            return;
                        }

                        var details = quest.Details;
                        Logger.ForAccount(session)
                            .Information("Talk to QuestNPC: {0}, Found Quest:{1}, State:{2}", obj.Info.Name, details.Name, quest.State);
                        await session.SendAsync(new SSetQuest { Index = (byte)quest.Index, State = quest.StateByte });
                        break;
                }
            }
            else
            {
                Logger.ForAccount(session)
                    .Debug("Talk to unasigned NPC {0}", obj.Info.Monster);
                await session.SendAsync(new SChatTarget(ObjectIndex, "Have a good day"));
            }
        }

        [MessageHandler(typeof(CBuy))]
        public async Task CBuy(GSSession session, CBuy message)
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
                await session.SendAsync(bResult);
                return;
            }

            bResult.Result = @char.Inventory.Add(item);
            if(bResult.Result == 0xff)
            {
                Logger
                    .ForAccount(session)
                    .Information("Insuficient Space");
                await session.SendAsync(bResult);
                return;
            }

            @char.Money -= item.BuyPrice;

            Logger
                .ForAccount(session)
                .Information("Buy {0} for {1}", item.ToString(), item.BuyPrice);

            await session.SendAsync(bResult);
        }

        [MessageHandler(typeof(CSell))]
        public async Task CSell(GSSession session, CSell message)
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

            await session.SendAsync(result);
        }

        [MessageHandler(typeof(CAttackS5E2))]
        public async Task CAttackS5E2(GSSession session, CAttackS5E2 message)
        {
            await CAttack(session, new CAttack { AttackAction = message.AttackAction, DirDis = message.DirDis, Number = message.Number });
        }

        [MessageHandler(typeof(CAttack))]
        public async Task CAttack(GSSession session, CAttack message)
        {
            var target = message.Number.ShufleEnding();
            var Dir = message.DirDis & 0x0F;
            var Dis = (message.DirDis & 0xF0) >> 4;

            //Logger.ForAccount(session)
            //    .Debug("Attack {0} {1}:{2} {3}", message.AttackAction, Dir, Dis, target);

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

                player?.Character.GetAttackedDelayed(@char.Player, (int)attack, type, TimeSpan.FromMilliseconds(500));
                monster?.GetAttackedDelayed(@char.Player, (int)attack, type, TimeSpan.FromMilliseconds(500));
            }
        }

        [MessageHandler(typeof(CMagicDuration))]
        public void CMagicDuration(GSSession session, CMagicDuration message)
        {
            var @char = session.Player.Character;

            if(!@char.Spells.SpellDictionary.ContainsKey(message.MagicNumber))
            {
                Logger.Error("Invalid Magic, user don't own this spell {0}", message.MagicNumber);
                return;
            }

            var magic = @char.Spells.SpellDictionary[message.MagicNumber];

            if (@char.Mana < magic.Mana || @char.Stamina < magic.BP)
                return;

            @char.Mana -= magic.Mana;
            @char.Stamina -= magic.BP;

            switch (message.MagicNumber)
            {
                case Spell.TwistingSlash:
                    var vp = @char.MonstersVP
                        .ToList() // clone for preveen collection changes
                        .Select(x => MonstersMng.Instance.GetMonster(x))
                        .Where(x => x.Position.Substract(@char.Position).Length() <= 2.0 && x.Type == ObjectType.Monster);

                    foreach (var mob in vp)
                    {
                        DamageType type;
                        var attack = @char.SkillAttack(magic, mob, out type);
                        mob.GetAttacked(@char.Player, attack, type);
                    }
                    session.SendAsync(new SMagicDuration(magic.Number, (ushort)session.ID, message.X, message.Y, message.Dis));

                    break;
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
                    .Error("Invalid MIX");

                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.Fail });
                return;
            }

            Logger.ForAccount(session)
                .Information("Mix found, match: {0}", mixMatched.Name);

            if (!@char.Inventory.TryAdd())
            {
                await session.SendAsync(new SNotice(NoticeType.Blue, "Organize your inventory before mix"));
                await session.SendAsync(new SChaosBoxItemMixButtonClick { Result = ChaosBoxMixResult.Fail });
                return;
            }

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

        [MessageHandler(typeof(CItemModify))]
        public void CItemModify(GSSession session, CItemModify message)
        {
            if(message.Position == 0xff)
            {
                return;
            }

            var it = session.Player.Character.Inventory.Get(message.Position);
            var res = new SItemModify
            {
                Money = 0,
            };

            var cost = it.RepairPrice;

            if(cost <= 0 || cost > session.Player.Character.Money)
            {
                session.SendAsync(res);
                return;
            }

            res.Money = cost;
            it.Durability = it.BasicInfo.Durability;
            session.Player.Character.Money -= cost;
            session.SendAsync(res);
        }

        [MessageHandler(typeof(CPartyRequest))]
        public async Task CPartyRequest(GSSession session, CPartyRequest message)
        {
            var trg = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);
            if(trg == null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.PlayerOffline));
                return;
            }

            if(trg.Player.Character.Party != null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.InAnotherParty));
                return;
            }

            var party = session.Player.Character.Party;

            if((party != null && party.Master != session.Player) || session.Player.Window != null || trg.Player.Window != null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.Fail));
                return;
            }

            if(Math.Abs(session.Player.Character.Level - trg.Player.Character.Level) > PartyManager.MaxLevelDiff)
            {
                await session.SendAsync(new SPartyResult(PartyResults.RestrictedLevel));
                return;
            }

            message.Number = (ushort)session.ID;
            await trg.SendAsync(message);
        }

        [MessageHandler(typeof(CPartyRequestResult))]
        public async Task CPartyRequestResult(GSSession session, CPartyRequestResult message)
        {
            var trg = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);
            if(trg == null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.Fail));
                return;
            }
            PartyManager.CreateLink(session.Player, trg.Player);
        }

        [MessageHandler(typeof(CPartyList))]
        public async Task CPartyList(GSSession session)
        {
            var partyInfo = new SPartyList();
            var party = session.Player.Character.Party;

            if (party == null)
            {
                await session.SendAsync(partyInfo);
                return;
            }

            partyInfo.Result = PartyResults.Success;
            partyInfo.PartyMembers = party.List();
            await session.SendAsync(partyInfo);
        }

        [MessageHandler(typeof(CPartyDelUser))]
        public void CPartyDelUser(GSSession session, CPartyDelUser message)
        {
            var party = session.Player.Character.Party;
            if(party == null)
            {
                return;
            }

            var memb = party.Members.ElementAtOrDefault(message.Index);
            if(memb == null)
            {
                return;
            }

            if(memb != party.Master && memb != session.Player)
            {
                return;
            }

            PartyManager.Remove(memb);
        }
    }
}
