using MU.DataBase;
using MU.Resources;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Monsters;
using MU.Network.Game;
using MuEmu.Resources;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Spells
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Spells));
        private Dictionary<Spell, SpellInfo> _spellList;
        private List<Buff> _buffs;
        private List<Spell> _newSpell = new List<Spell>();
        private List<Spell> _delSpell = new List<Spell>();

        public float PvMAttackSuccessRate { get; private set; }
        public float AdvancedAttackSuccessRate { get; private set; }
        public float AdvancedDefenseSuccessRate { get; private set; }
        public float RepairLevel1 { get; private set; }
        public float RepairLevel2 { get; private set; }
        public float PoisonResistance { get; private set; }
        public float LightningResistance { get; private set; }
        public float IceResistance { get; private set; }
        public float IncreaseAutoRegeneration { get; private set; }
        public float IncreaseZen { get; private set; }
        public float IncreaseDefense { get; private set; }
        public float IncreaseMaxHP { get; private set; }
        public float IncreaseMaxAG { get; private set; }
        public float IncreaseManaReduction { get; private set; }
        public float MonsterAttackLifeIncrease { get; private set; }
        public float MonsterAttackSDIncrease { get; private set; }
        public float IncreaseExperience { get; private set; }

        public Monster Monster { get; }
        public Player Player { get; }
        public Character Character { get; }

        public Spells(Monster monster)
        {
            Monster = monster;
            _spellList = new Dictionary<Spell, SpellInfo>();
            _buffs = new List<Buff>();
        }

        public Spells(Character @char, CharacterDto character)
        {
            _spellList = new Dictionary<Spell, SpellInfo>();
            Player = @char.Player;
            Character = @char;
            _buffs = new List<Buff>();


            var spells = ResourceCache.Instance.GetSkills();
            
            foreach (var skill in Character.BaseInfo.Spells)
            {
                var spell = spells[skill];
                _spellList.Add(skill, spell);
                Logger
                    .ForAccount(Player.Session)
                    .Information("Class Skill Added: {0}", spell.Name);
            }

            foreach (var skill in character.Spells.Select(x => (Spell)x.Magic))
            {
                var spell = spells[skill];
                _spellList.Add(skill, spell);
                SetEffect((int)skill);
                Logger
                    .ForAccount(Player.Session)
                    .Information("Learned {0} Skill Added", spell.Name);
            }
        }

        private void Add(Spell skill)
        {
            var spells = ResourceCache.Instance.GetSkills();

            if (!_spellList.ContainsKey(skill))
            {
                _spellList.Add(skill, spells[skill]);
                _newSpell.Add(skill);
                SetEffect((int)skill);
            }
        }

        public async Task<bool> TryAdd(Spell skill)
        {
            var spells = ResourceCache.Instance.GetSkills();
            var spell = spells
                .Where(x => x.Key == skill)
                .Select(x => x.Value)
                .FirstOrDefault();

            if(spell == null)
            {
                Logger.Error($"Can't find skill {skill}");
                return false;
            }

            if(spell.ReqLevel > Character.Level)
            {
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"You need reach Lv. {spell.ReqLevel}"));
                return false;
            }

            if(spell.Str > Character.Strength)
            {
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"You need {spell.Str} of Strength"));
                return false;
            }

            if(spell.Agility > Character.Agility)
            {
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"You need {spell.Agility} of Agility"));
                return false;
            }

            if (spell.Energy > Character.Energy)
            {
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"You need {spell.Energy} of Energy"));
                return false;
            }

            var classList = spell.Classes.Select(x => new { BaseClass = (HeroClass)(((byte)x) & 0xF0), Class = (byte)x });
            var canUse = classList
                .Where(x => x.BaseClass == Character.BaseClass && x.Class <= (byte)Character.Class)
                .Any();

            if (!canUse)
            {
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"Only {string.Join(", ", spell.Classes)} can use this skill"));
                return false;
            }

            if(_spellList.ContainsKey(skill))
            {
                return false;
            }

            var pos = _spellList.Count;

            Add(skill);

            if(Player.Status == LoginStatus.Playing)
            {
                await Player.Session.SendAsync(new SSpells(0, new MuEmu.Network.Data.SpellDto
                {
                    Index = (byte)pos,
                    Spell = (ushort)skill,
                }));
                await Player.Session.SendAsync(new SNotice(NoticeType.Blue, $"You have learned: {spell.Name}"));
            }

            return true;
        }

        public void Remove(Spell skill)
        {
            _spellList.Remove(skill);
            _delSpell.Add(skill);
        }

        public void Remove(SpellInfo skill)
        {
            Remove(skill.Number);
        }

        public List<SpellInfo> SpellList => _spellList.Select(x => x.Value).ToList();
        public IDictionary<Spell, SpellInfo> SpellDictionary => _spellList;

        public IEnumerable<Buff> BuffList => _buffs;

        public bool BufActive(SkillStates effect)
        {
            return _buffs.Any(x => x.State == effect);
        }

        public async void SendList()
        {
            var i = 0;
            var list = new List<MuEmu.Network.Data.SpellDto>();
            foreach(var magic in _spellList)
            {
                list.Add(new MuEmu.Network.Data.SpellDto
                {
                    Index = (byte)i,
                    Spell = (ushort)magic.Key,
                });
                i++;
            }
            await Player.Session.SendAsync(new SSpells(0, list.ToArray()));
        }

        internal void ItemSkillAdd(Spell skill)
        {
            if (_spellList.ContainsKey(skill))
                return;

            var pos = _spellList.Count;
            var spells = ResourceCache.Instance.GetSkills();
            _spellList.Add(skill, spells[skill]);

            if (Player.Status == LoginStatus.Playing)
            {
                Player.Session.SendAsync(new SSpells(0, new MuEmu.Network.Data.SpellDto
                {
                    Index = (byte)pos,
                    Spell = (ushort)skill,
                })).Wait();
            }
        }

        internal void ItemSkillDel(Spell skill)
        {
            _spellList.Remove(skill);
            SendList();
        }

        public async void SetBuff(SkillStates effect, TimeSpan time, Character source = null)
        {
            if (_buffs.Any(x => x.State == effect))
                return;

            var buff = new Buff { State = effect, EndAt = DateTimeOffset.Now.Add(time), Source = source };
            var @char = Player?.Character??null;

            switch (effect)
            {
                case SkillStates.ShadowPhantom:
                    if (@char == null)
                        break;
                    buff.AttackAdd = @char.Level / 3 + 45;
                    buff.DefenseAdd = @char.Level / 3 + 50;
                    break;
                case SkillStates.SoulBarrier:
                    buff.DefenseAddRate = 10 + source.AgilityTotal / 50 + source.EnergyTotal / 200;
                    break;
                case SkillStates.Defense:
                    buff.DefenseAdd = source.EnergyTotal / 8;
                    break;
                case SkillStates.Attack:
                    buff.AttackAdd = source.EnergyTotal / 7;
                    break;
                case SkillStates.SwellLife:
                    buff.LifeAdd = 12 + source.EnergyTotal / 10 + source.VitalityTotal / 100;
                    Character.MaxHealth += buff.LifeAdd;
                    break;
                case SkillStates.HAttackPower:
                    buff.AttackAdd = 25;
                    break;
                case SkillStates.HAttackSpeed:
                    break;
                case SkillStates.HDefensePower:
                    buff.DefenseAdd = 100;
                    break;
                case SkillStates.HMaxLife:
                    buff.LifeAdd = 500;
                    break;
                case SkillStates.HMaxMana:
                    buff.ManaAdd = 500;
                    break;
                case SkillStates.Poison:
                    buff.PoisonDamage = 12 + source.EnergyTotal / 10;
                    break;
                case SkillStates.SkillDamageDeflection:
                    buff.DamageDeflection = (30 + (source.EnergyTotal / 42))/100.0f;
                    break;
            }

            _buffs.Add(buff);
            if(Monster != null)
            {
                var m2 = new SViewSkillState(1, Monster.Index, (byte)effect);

                if(Monster.Info.ViewRange <= 0)
                {
                    await Monster.Map.SendAsync(m2);
                }
                else
                {
                    await Monster.ViewPort.Select(x => x.Session).SendAsync(m2);
                }
                return;
            }
            var m = new SViewSkillState(1, (ushort)Player.Session.ID, (byte)effect);

            await Player.Session.SendAsync(m);
            Player.SendV2Message(m);
        }

        public async Task ClearBuffByEffect(SkillStates effect)
        {
            var rem = _buffs.Where(x => x.State == effect);
            _buffs = _buffs.Except(rem).ToList();
            await DelBuff(rem.First());
        }

        public async void ClearBuffTimeOut()
        {
            var b = _buffs.Where(x => x.EndAt > DateTimeOffset.Now);
            var rem = _buffs.Except(b);
            _buffs = b.ToList();

            try
            {
                foreach (var r in rem)
                    await DelBuff(r);
            }catch(Exception)
            {
                _buffs.Clear();
                return;
            }

            var poison = _buffs.FirstOrDefault(x => x.State == SkillStates.Poison);
            if(poison != null)
            {
               Player?.Character.GetAttacked(poison.Source?.Player.ID??0xffff, 0x00, 0x00, poison.PoisonDamage, DamageType.Poison, Spell.Poison, 0).Wait();
               Monster?.GetAttacked(poison.Source.Player, poison.PoisonDamage + (int)(Monster.Life * 0.03f), DamageType.Poison, 0).Wait();
            }
        }

        public async void ClearAll()
        {
            try
            {
                foreach (var r in _buffs)
                    await DelBuff(r);
            }
            catch (Exception)
            { }
            _buffs.Clear();
        }

        public async Task DelBuff(Buff effect)
        {
            if(Monster != null)
            {
                var m2 = new SViewSkillState(0, Monster.Index, (byte)effect.State);

                await Monster.ViewPort.Select(x => x.Session).SendAsync(m2);
                return;
            }

            var m = new SViewSkillState(0, (ushort)Player.Session.ID, (byte)effect.State);

            switch(effect.State)
            {
                case SkillStates.SwellLife:
                    Character.MaxHealth -= effect.LifeAdd;
                    break;
            }

            await Player.Session.SendAsync(m);
            Player.SendV2Message(m);
        }

        public async void AttackSend(Spell spell, ushort Target, bool Success)
        {
            Target &= 0x7FFF;
            Target = Success ? (ushort)(Target | 0x8000) : Target;

            object message = null;

            switch (Program.Season)
            {
                case 9:
                    message = new SMagicAttackS9(spell, (ushort)Player.Session.ID, Target);
                    break;
                default:
                    message = new SMagicAttack(spell, (ushort)Player.Session.ID, Target);
                    break;
            }

            if (Monster == null)
            {
                await Player
                    .Session
                    .SendAsync(message);

                Player.SendV2Message(message);
            }
        }

        public SkillStates[] ViewSkillStates => _buffs.Select(x => x.State).ToArray();

        internal void SetEffect(int skill)
        {
            var spell = _spellList[(Spell)skill];
            if (skill >= 300 && skill < 305)
            {
                PvMAttackSuccessRate = spell.Damage.X/100.0f;
            }
            else if (skill >= 305 && skill < 310)
            {
                AdvancedAttackSuccessRate = spell.Damage.X/100.0f;
            }
            else if (skill >= 310 && skill < 315)
            {
                AdvancedDefenseSuccessRate = spell.Damage.X/100.0f;
            }
            else if (skill >= 315 && skill < 320)
            {
                RepairLevel1 = spell.Damage.X;
            }
            else if (skill >= 320 && skill < 325)
            {
                RepairLevel2 = spell.Damage.X;
            }
            else if (skill >= 325 && skill < 330)
            {
                PoisonResistance = spell.Damage.X; //%
            }
            else if (skill >= 330 && skill < 335)
            {
                LightningResistance = spell.Damage.X; //%
            }
            else if (skill >= 335 && skill < 340)
            {
                IceResistance = spell.Damage.X; //%
            }
            else if (skill >= 340 && skill < 345)
            {
                IncreaseAutoRegeneration = spell.Damage.X ;
            }
            else if (skill >= 345 && skill < 350)
            {
                IncreaseZen = spell.Damage.X/100.0f;
            }
            else if (skill >= 350 && skill < 355)
            {
                IncreaseDefense = spell.Damage.X;
            }
            else if (skill >= 355 && skill < 360)
            {
                IncreaseMaxHP = spell.Damage.X/100.0f;
            }
            else if (skill >= 360 && skill < 365)
            {
                IncreaseMaxAG = spell.Damage.X/100.0f;
            }
            else if (skill >= 365 && skill < 370)
            {
                IncreaseManaReduction = spell.Damage.X + spell.Damage.Y;
            }
            else if (skill >= 370 && skill < 375)
            {
                MonsterAttackLifeIncrease = spell.Damage.X; // HP/MonsterAttackLifeIncrease
            }
            else if (skill >= 375 && skill < 380)
            {
                MonsterAttackSDIncrease = spell.Damage.X; // SP/MonsterAttackSDIncrease
            }
            else if (skill >= 380 && skill < 385)
            {
                IncreaseExperience = spell.Damage.X;
            }
        }

        public async Task Save(GameContext db)
        {
            if (!_newSpell.Any())
                return;

            _delSpell.ForEach(x => _newSpell.Remove(x));

            await db.Spells.AddRangeAsync(_newSpell.Select(x => new SpellDto
            {
                CharacterId = Character.Id,
                Level = 1,
                Magic = (short)x,
            }));

            db.Spells.RemoveRange(db.Spells.Where(x => x.CharacterId == Character.Id && _delSpell.Contains((Spell)x.Magic)));

            _newSpell.Clear();
            _delSpell.Clear();
        }
    }
}
