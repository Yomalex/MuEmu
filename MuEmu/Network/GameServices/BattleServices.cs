using MU.Network;
using MU.Network.Game;
using MU.Resources;
using MuEmu.Monsters;
using MuEmu.Resources;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network.GameServices
{
    public partial class GameServices
    {
        [MessageHandler(typeof(CAttackS5E2))]
        public async Task CAttackS5E2(GSSession session, CAttackS5E2 message)
        {
            await CAttack(session, new CAttack { AttackAction = message.AttackAction, DirDis = message.DirDis, wzNumber = message.Number });
        }

        [MessageHandler(typeof(CAttack))]
        public async Task CAttack(GSSession session, CAttack message)
        {
            var targetId = message.Number;
            var Dir = message.DirDis & 0x0F;
            var Dis = (message.DirDis & 0xF0) >> 4;

            session.Player.Character.Direction = message.DirDis;

            if (message.Number >= MonstersMng.MonsterStartIndex) // Is Monster
            {
                try
                {
                    var monster = MonstersMng.Instance.GetMonster(targetId);

                    if (monster.Life <= 0)
                        return;

                    session.Player.SendV2Message(new SAction((ushort)session.ID, message.DirDis, message.AttackAction, targetId));
                    if (monster.Type == ObjectType.NPC)
                    {
                        return;
                    }

                    DamageType type;
                    var attack = session.Player.Character.Attack(monster, out type);
                    var eAttack = await session.Player.Character.PentagramAttack(monster);
                    await monster.GetAttacked(session.Player, attack, type, eAttack);
                }
                catch (Exception ex)
                {
                    Logger.ForAccount(session)
                        .Error(ex, "Invalid monster #{0}", targetId);
                }
            }
            else
            {
                try
                {
                    var target = Program.server.Clients.First(x => x.ID == targetId);
                    if (target.Player.Character.Health <= 0.0f)
                        return;

                    var rivals = session.Player.Character.Duel == target.Player.Character.Duel;

                    if (!(Program.XMLConfiguration.GamePlay.PVP || rivals))
                        return;

                    /*await session.Player
                        .SendV2Message(new SAction((ushort)session.ID, message.DirDis, message.AttackAction, targetId));*/

                    var attack = session.Player.Character
                        .Attack(target.Player.Character, out DamageType type);

                    var eattack = await session.Player.Character
                        .PentagramAttack(target.Player.Character);

                    await target.Player.Character
                        .GetAttacked((ushort)session.ID, message.DirDis, message.AttackAction, attack, type, Spell.None, eattack);
                }
                catch (Exception ex)
                {
                    Logger.ForAccount(session)
                        .Error(ex, "Invalid player #{0}", targetId);
                }
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
            Point pos;
            int defense = 0;

            if (!@char.Spells.SpellDictionary.ContainsKey(message.MagicNumber))
            {
                Logger.Error("Invalid Magic, user don't own this spell {0}", message.MagicNumber);
                return;
            }

            spell = @char.Spells.SpellDictionary[message.MagicNumber];
            int eDmg = 0;

            try
            {
                if (target >= MonstersMng.MonsterStartIndex) // Is Monster
                {
                    monster = MonstersMng.Instance.GetMonster(target);
                    spells = monster.Spells;
                    defense = monster.Defense;
                    pos = monster.Position;
                    eDmg = @char.PentagramAttack(monster).Result;
                }
                else
                {
                    player = Program.server.Clients.First(x => x.ID == target).Player;
                    spells = player.Character.Spells;
                    defense = player.Character.Defense;
                    pos = player.Character.Position;
                    eDmg = @char.PentagramAttack(player.Character).Result;
                }
            }
            catch (Exception)
            {
                Logger.Error("MagicAttack: Invalid target");
                return;
            }

            var mana = @char.Mana - spell.Mana;
            var bp = @char.Stamina;

            if (mana >= 0 && bp >= 0)
            {
                switch (spell.Number)
                {
                    case Spell.Poison:
                        @char.Spells.AttackSend(spell.Number, message.Target, false);
                        spells.SetBuff(SkillStates.Poison, TimeSpan.FromSeconds(60), @char);
                        break;
                    case Spell.Ice:
                        @char.Spells.AttackSend(spell.Number, message.Target, false);
                        spells.SetBuff(SkillStates.Ice, TimeSpan.FromSeconds(60), @char);
                        break;
                    case Spell.InfinityArrow:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;
                        spells.SetBuff(SkillStates.InfinityArrow, TimeSpan.FromSeconds(1800), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.Heal:
                    case Spell.Heal1:
                    case Spell.Heal2:
                    case Spell.Heal3:
                    case Spell.Heal4:
                    case Spell.Heal5:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        var addLife = @char.EnergyTotal / 5;

                        if (spells.Character == null)
                        {
                            return;
                        }

                        spells.Character.Health += addLife;
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.GreaterDefense:
                    case Spell.GreaterDefense1:
                    case Spell.GreaterDefense2:
                    case Spell.GreaterDefense3:
                    case Spell.GreaterDefense4:
                    case Spell.GreaterDefense5:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        spells.SetBuff(SkillStates.Defense, TimeSpan.FromSeconds(1800), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.GreaterDamage:
                    case Spell.GreaterDamage1:
                    case Spell.GreaterDamage2:
                    case Spell.GreaterDamage3:
                    case Spell.GreaterDamage4:
                    case Spell.GreaterDamage5:
                        if (@char.BaseClass != HeroClass.FaryElf)
                            return;

                        spells.SetBuff(SkillStates.Attack, TimeSpan.FromSeconds(1800), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;

                    case Spell.SoulBarrier:
                    case Spell.SoulBarrier1:
                    case Spell.SoulBarrier2:
                    case Spell.SoulBarrier3:
                    case Spell.SoulBarrier4:
                    case Spell.SoulBarrier5:
                        spells.SetBuff(SkillStates.SoulBarrier, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 40), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;

                    case Spell.GreaterFortitude:
                    case Spell.GreatFortitude1:
                    case Spell.GreatFortitude2:
                    case Spell.GreatFortitude3:
                    case Spell.GreatFortitude4:
                    case Spell.GreatFortitude5:
                        spells.SetBuff(SkillStates.SwellLife, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 10), @char);
                        if (@char.Party != null)
                        {
                            foreach (var a in @char.Party.Members)
                                a.Character.Spells.SetBuff(SkillStates.SwellLife, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 10), @char);
                        }
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.Reflex:
                        spells.SetBuff(SkillStates.SkillDamageDeflection, TimeSpan.FromSeconds(60 + @char.EnergyTotal / 40), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    case Spell.Sleep:
                        spells.SetBuff(SkillStates.SkillSleep, TimeSpan.FromSeconds(30 + @char.EnergyTotal / 25), @char);
                        @char.Spells.AttackSend(spell.Number, message.Target, true);
                        break;
                    default:
                        @char.Spells.AttackSend(spell.Number, message.Target, false);
                        break;
                }

                @char.Mana = mana;
                DamageType type = DamageType.Regular;
                var attack = 0.0f;
                switch (spell.Number)
                {
                    case Spell.Falling_Slash:
                    case Spell.Lunge:
                    case Spell.Uppercut:
                    case Spell.Cyclone:
                    case Spell.Slash:
                    case Spell.TwistingSlash:
                    case Spell.TwistingSlash1:
                    case Spell.TwistingSlash2:
                    case Spell.TwistingSlash3:
                    case Spell.TwistingSlash4:
                    case Spell.TwistingSlash5:
                    case Spell.RagefulBlow:
                    case Spell.RagefulBlow1:
                    case Spell.RagefulBlow2:
                    case Spell.RagefulBlow3:
                    case Spell.RagefulBlow4:
                    case Spell.RagefulBlow5:
                    case Spell.DeathStab:
                    case Spell.DeathStab1:
                    case Spell.DeathStab2:
                    case Spell.DeathStab3:
                    case Spell.DeathStab4:
                    case Spell.DeathStab5:
                    case Spell.CrescentMoonSlash:
                    case Spell.Impale:
                    case Spell.FireBreath:
                        attack = @char.SkillAttack(spell, defense, out type);
                        //else
                        //    @char.SkillAttack(spell, player, out type);
                        break;
                    case Spell.Heal:
                    case Spell.Heal1:
                    case Spell.Heal2:
                    case Spell.Heal3:
                    case Spell.Heal4:
                    case Spell.Heal5:
                    case Spell.GreaterDamage:
                    case Spell.GreaterDamage1:
                    case Spell.GreaterDamage2:
                    case Spell.GreaterDamage3:
                    case Spell.GreaterDamage4:
                    case Spell.GreaterDamage5:
                    case Spell.GreaterDefense:
                    case Spell.GreaterDefense1:
                    case Spell.GreaterDefense2:
                    case Spell.GreaterDefense3:
                    case Spell.GreaterDefense4:
                    case Spell.GreaterDefense5:
                    case Spell.GreaterFortitude:
                    case Spell.GreatFortitude1:
                    case Spell.GreatFortitude2:
                    case Spell.GreatFortitude3:
                    case Spell.GreatFortitude4:
                    case Spell.GreatFortitude5:
                    case Spell.SoulBarrier:
                    case Spell.SoulBarrier1:
                    case Spell.SoulBarrier2:
                    case Spell.SoulBarrier3:
                    case Spell.SoulBarrier4:
                    case Spell.SoulBarrier5:
                    case Spell.Teleport:
                    case Spell.InfinityArrow:
                        return;
                    default:
                        if (spell.IsDamage == 0)
                            return;

                        if (@char.BaseClass == HeroClass.Summoner || @char.BaseClass == HeroClass.DarkWizard || @char.BaseClass == HeroClass.MagicGladiator)
                        {
                            attack = @char.MagicAttack(spell, defense, out type);
                        }
                        else
                        {
                            attack = @char.SkillAttack(spell, defense, out type);
                        }

                        if (attack <= 0)
                        {
                            attack = 0;
                            type = DamageType.Miss;
                        }
                        break;
                }

                if (@char.Spells.CheckCombo(spell.Number))
                {
                    attack += (@char.StrengthTotal + @char.AgilityTotal + @char.EnergyTotal)/2.0f;
                    @char.Spells.AttackSend(Spell.Combo, target, true);
                }

                player?.Character.GetAttacked((ushort)@char.Player.Session.ID, @char.Direction, 0, (int)attack, type, spell.Number, eDmg);
                monster?.GetAttackedDelayed(@char.Player, (int)attack, type, TimeSpan.FromMilliseconds(500));
            }
        }

        [MessageHandler(typeof(CMagicAttackS9))]
        public void CMagicAttackS9(GSSession session, CMagicAttackS9 message) => CMagicAttack(session, new CMagicAttack { MagicNumber = message.MagicNumber, Target = message.Target });

        [MessageHandler(typeof(CMagicDurationS9))]
        public async Task CMagicDurationS9(GSSession session, CMagicDurationS9 message) => await CMagicDuration(session, new CMagicDuration
        {
            MagicNumber = message.MagicNumber,
            Target = message.Target,
            Dir = message.Dir,
            Dis = message.Dis,
            MagicKey = message.MagicKey,
            TargetPos = message.TargetPos,
            X = message.X,
            Y = message.Y,
        });

        [MessageHandler(typeof(CMagicDuration))]
        public async Task CMagicDuration(GSSession session, CMagicDuration message)
        {
            var @char = session.Player.Character;

            if (!@char.Spells.SpellDictionary.ContainsKey(message.MagicNumber))
            {
                Logger.Error("Invalid Magic, user don't own this spell {0}", message.MagicNumber);
                return;
            }

            var magic = @char.Spells.SpellDictionary[message.MagicNumber];

            if (@char.Mana < magic.Mana || @char.Stamina < magic.BP)
                return;

            if ((magic.Number == Spell.Triple_Shot ||
                magic.Number == Spell.Penetration ||
                magic.Number == Spell.IceArrow ||
                magic.Number == Spell.MultiShot))
            {
                if (@char.Inventory.Arrows == null)
                    return;

                if (!@char.Spells.BufActive(SkillStates.InfinityArrow))
                {
                    var durDown = magic.Number == Spell.Triple_Shot ? 3 : (magic.Number == Spell.MultiShot ? 5 : 0);
                    if (@char.Inventory.Arrows.Durability > durDown)
                        @char.Inventory.Arrows.Durability -= (byte)durDown;
                    else
                        @char.Inventory.Arrows.Durability--;

                    if (@char.Inventory.Arrows.Durability == 0)
                        await @char.Inventory.Delete(@char.Inventory.Arrows);
                }
            }

            @char.Mana -= magic.Mana;
            @char.Stamina -= magic.BP;

            object msgdef = null;
            msgdef = VersionSelector.CreateMessage<SMagicDuration>(magic.Number, (ushort)session.ID, message.X, message.Y, message.Dir);
            await session.SendAsync(msgdef);
            session.Player.SendV2Message(msgdef);

            var dir = (message.Dir & 0xF0) >> 4;
            var dis = (message.Dir & 0x0F);

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
            DamageType type = DamageType.Regular;
            int attack = 0;
            Point pos = new Point();
            Monster mom = null;
            Player plr;

            if (message.Target != 0 && message.Target != 0xffff)
            {
                if (message.Target < MonstersMng.MonsterStartIndex)
                {
                    plr = Program.server.Clients.First(x => x.ID == message.Target).Player;
                    attack = @char.SkillAttack(magic, plr.Character.Defense, out type);
                    pos = plr.Character.Position;
                    var eDmg = await @char.PentagramAttack(plr.Character);
                    await plr.Character.GetAttacked(@char.Player.ID, message.Dir, 0, attack, type, message.MagicNumber, eDmg);
                }
                else
                {
                    mom = MonstersMng.Instance.GetMonster(message.Target);
                    attack = @char.SkillAttack(magic, mom.Defense, out type);
                    pos = mom.Position;
                    var eDmg = await @char.PentagramAttack(mom);
                    await mom.GetAttacked(@char.Player, attack, type, eDmg);
                }
            }

            if (@char.Spells.CheckCombo(message.MagicNumber))
            {
                attack += (@char.StrengthTotal + @char.AgilityTotal + @char.EnergyTotal) / 2;
                @char.Spells.AttackSend(Spell.Combo, message.Target, true);
            }

            switch (message.MagicNumber)
            {
                case Spell.DrainLife:
                    {
                        if (mom != null)
                        {
                            @char.Health += (@char.EnergyTotal / 15.0f) + (mom.Info.Level / 2.5f);
                        }
                        else
                        {
                            @char.Health += (@char.EnergyTotal / 23.0f) + (attack * 0.1f);
                        }
                        var mg = VersionSelector.CreateMessage<SMagicAttack>(message.MagicNumber, @char.Player.ID, message.Target);
                        @char.SendV2Message(mg);
                        session.SendAsync(mg).Wait();
                    }
                    break;
                case Spell.ChainLighting:
                    {
                        var mvpcopy = @char.MonstersVP.ToList();
                        var t1 = mvpcopy
                            .FirstOrDefault(
                            x => (MonstersMng
                            .Instance
                            .GetMonster(x)?
                            .Position
                            .Substract(pos)
                            .Length()??100) < 2);

                        var t2 = mvpcopy
                            .Except(new[] { t1 })
                            .FirstOrDefault(
                            x => (MonstersMng
                            .Instance
                            .GetMonster(x)?
                            .Position
                            .Substract(pos)
                            .Length() ?? 100) < 4);

                        var l = new List<ushort>() { message.Target };

                        if (t1 != 0)
                        {
                            l.Add(t1);
                            var mob = MonstersMng.Instance.GetMonster(t1);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                        }

                        if (t2 != 0)
                        {
                            l.Add(t2);
                            var mob = MonstersMng.Instance.GetMonster(t2);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                        }

                        var obj = new SChainMagic
                        {
                            wzMagic = ((ushort)Spell.ChainLighting).ShufleEnding(),
                            UserIndex = (ushort)session.ID,
                            Targets = l.ToArray(),
                        };
                        var mg = VersionSelector.CreateMessage<SMagicAttack>(message.MagicNumber, @char.Player.ID, message.Target);
                        @char.SendV2Message(mg);
                        session.SendAsync(mg).Wait();
                        session.SendAsync(obj).Wait();
                        session.Player.SendV2Message(obj);
                    }
                    break;
                case Spell.RagefulBlow:
                case Spell.RagefulBlow1:
                case Spell.RagefulBlow2:
                case Spell.RagefulBlow3:
                case Spell.RagefulBlow4:
                case Spell.RagefulBlow5:
                    {
                        var mp = new Point(message.X, message.Y);
                        var vp = @char.MonstersVP
                            .ToList() // clone for preveen collection changes
                            .Select(x => MonstersMng.Instance.GetMonster(x))
                            .Where(x => x.Position.Substract(mp).Length() <= 2.0 && x.Type != ObjectType.NPC);

                        foreach (var mob in vp)
                        {
                            attack = @char.SkillAttack(magic, mob.Defense, out type);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                        }
                    }
                    break;
                case Spell.TwistingSlash:
                case Spell.TwistingSlash1:
                case Spell.TwistingSlash2:
                case Spell.TwistingSlash3:
                case Spell.TwistingSlash4:
                case Spell.TwistingSlash5:
                    {
                        var vp = @char.MonstersVP
                            .ToList() // clone for preveen collection changes
                            .Select(x => MonstersMng.Instance.GetMonster(x))
                            .Where(x => x.Position.Substract(@char.Position).Length() <= 2.0 && x.Type != ObjectType.NPC);

                        foreach (var mob in vp)
                        {
                            attack = @char.SkillAttack(magic, mob.Defense, out type);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                        }
                    }
                    break;
                case Spell.Decay:
                    {
                        var mp = new Point(message.X, message.Y);
                        var vp = @char.MonstersVP
                            .ToList() // clone for preveen collection changes
                            .Select(x => MonstersMng.Instance.GetMonster(x))
                            .Where(x => x.Position.Substract(mp).Length() <= 2.0 && x.Type != ObjectType.NPC);
                        foreach (var mob in vp)
                        {
                            attack = @char.SkillAttack(magic, mob.Defense, out type);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                            mob.Spells.SetBuff(SkillStates.Poison, TimeSpan.FromSeconds(60), @char);
                        }
                    }
                    break;
                case Spell.IceStorm:
                case Spell.IceStorm1:
                case Spell.IceStorm2:
                case Spell.IceStorm3:
                case Spell.IceStorm4:
                case Spell.IceStorm5:
                    {
                        var mp = new Point(message.X, message.Y);
                        var vp = @char.MonstersVP
                            .ToList() // clone for preveen collection changes
                            .Select(x => MonstersMng.Instance.GetMonster(x))
                            .Where(x => x.Position.Substract(mp).Length() <= 2.0 && x.Type != ObjectType.NPC);
                        foreach (var mob in vp)
                        {
                            attack = @char.SkillAttack(magic, mob.Defense, out type);
                            var eDmg = await @char.PentagramAttack(mob);
                            await mob.GetAttacked(@char.Player, attack, type, eDmg);
                            mob.Spells.SetBuff(SkillStates.Ice, TimeSpan.FromSeconds(60), @char);
                        }
                    }
                    break;
                case Spell.Neil:
                case Spell.Sahamutt:
                    {
                        var mp = new Point(message.X, message.Y);
                        var vp = @char.MonstersVP
                            .ToList() // clone for preveen collection changes
                            .Select(x => MonstersMng.Instance.GetMonster(x))
                            .Where(x => x.Position.Substract(mp).Length() <= 2.0 && x.Type != ObjectType.NPC);
                        foreach (var mob in vp)
                        {
                            attack = @char.MagicAttack(magic, mob.Defense, out type);
                            mob.GetAttackedDelayed(@char.Player, attack, type, TimeSpan.FromMilliseconds(300));
                            //mob.Spells.SetBuff(SkillStates.f, TimeSpan.FromSeconds(60), @char);
                        }
                    }
                    break;
            }
        }

        [MessageHandler(typeof(CBeattack))]
        public async Task CBeattack(GSSession session, CBeattack message)
        {
            var spell = ResourceCache.Instance.GetSkills()[message.MagicNumber];

            var mobList = from id in message.Beattack
                          select MonstersMng.Instance.GetMonster(id.Number);

            var mobInRange = from mob in mobList
                             where mob.MapID == session.Player.Character.MapID && spell.Distance >= mob.Position.Substract(message.Position).LengthSquared()
                             select mob;

            foreach (var mob in mobInRange)
            {
                DamageType dmgType;
                var dmg = session.Player.Character.MagicAttack(spell, mob.Defense, out dmgType);
                var eDmg = await session.Player.Character.PentagramAttack(mob);
                await mob.GetAttacked(session.Player, dmg, dmgType, eDmg);
            }
        }

        [MessageHandler(typeof(CBeattackS9))]
        public async Task CBeattackS9(GSSession session, CBeattackS9 message) => await CBeattack(session, new CBeattack
        {
            wzMagicNumber = ((ushort)message.MagicNumber).ShufleEnding(),
            Beattack = message.Beattack.Take(message.Count).Select(x => new CBeattackDto { MagicKey = x.MagicKey, wzNumber = x.Number.ShufleEnding() }).ToArray(),
            Serial = message.Serial,
            X = message.X,
            Y = message.Y,
        });
    }
}
