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
using MuEmu.Resources.XML;
using MU.Network;

namespace MuEmu
{
    public class SpellMagicInfo : SpellInfo
    {
        private short _level;

        public bool Changed { get; set; }
        public short Level { get => _level; set
            {
                _level = value;
                MLSValue = MLInfo?.GetValue(_level) ?? 0;
                Changed = true;
            }
        }
        public SkillTreeDto MLInfo { get; set; }
        public bool IsMLSkill => MLInfo != null;
        public float MLSValue { get; set; }
        public static SpellMagicInfo FromSpellInfo(HeroClass Class, SpellInfo si, short level = 1)
        {
            var res = new SpellMagicInfo();
            Extensions.AnonymousMap(res, si);
            res.MLInfo = MasterLevel.MasterSkillTree.Trees.Where(x => x.ID == Class)
                .SelectMany(x => x.Skill)
                .Where(x => (Spell)x.MagicNumber == si.Number)
                .Select(x => x)
                .FirstOrDefault();
            res.Level = level;
            res.Changed = false;

            return res;
        }
    }
    public class Spells
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Spells));
        private Dictionary<Spell, SpellMagicInfo> _spellList;
        private List<Buff> _buffs;
        private List<Spell> _newSpell = new List<Spell>();
        private List<Spell> _delSpell = new List<Spell>();


        // MasterLevel and other skills effect
        public float PvMAttackSuccessRate { get; private set; }
        public float PvPDefenceSuccessRate { get; private set; }
        public float AdvancedAttackSuccessRate { get; private set; }
        public float AdvancedDefenseSuccessRate { get; private set; }
        public float RepairLevel1 { get; private set; }
        public float RepairLevel2 { get; private set; }
        public float RepairLevel3 { get; private set; }
        public float PoisonResistance { get; private set; }
        public float LightningResistance { get; private set; }
        public float IceResistance { get; private set; }
        public float IncreaseAutoHPRegeneration { get; private set; }
        public float IncreaseAutoMPRegeneration { get; private set; }
        public float IncreaseAutoSDRegeneration { get; private set; }
        public float IncreaseAutoBPRegeneration { get; private set; }
        public float IncreaseZen { get; private set; }
        public float IncreaseDefense { get; private set; }
        public float IncreaseDefenseSuccessRate { get; private set; }
        public float IncreaseMaxHP { get; private set; }
        public float IncreaseMaxMP { get; private set; }
        public float IncreaseMaxAG { get; private set; }
        public float IncreaseMaxSD { get; private set; }
        public float IncreaseAgility { get; private set; }
        public float IncreaseStrength { get; private set; }
        public float IncreaseManaReduction { get; private set; }
        public float MonsterAttackLifeIncrease { get; private set; }
        public float MonsterAttackSDIncrease { get; private set; }
        public float IncreaseExperience { get; private set; }
        public float WingsDefensePowUp { get; private set; }
        public float WingsAttackPowUp { get; private set; }

        public Monster Monster { get; }
        public Player Player { get; }
        public Character Character { get; }

        public Spells(Monster monster)
        {
            Monster = monster;
            _spellList = new Dictionary<Spell, SpellMagicInfo>();
            _buffs = new List<Buff>();
        }

        public Spells(Character @char, CharacterDto character)
        {
            _spellList = new Dictionary<Spell, SpellMagicInfo>();
            Player = @char.Player;
            Character = @char;
            _buffs = new List<Buff>();


            var spells = ResourceCache.Instance.GetSkills();
            
            foreach (var skill in Character.BaseInfo.Spells)
            {
                var spell = SpellMagicInfo.FromSpellInfo(Character.Class, spells[skill]);
                _spellList.Add(skill, spell);
                Logger
                    .ForAccount(Player.Session)
                    .Information("Class Skill Added: {0}", spell.Name);
            }

            foreach (var skill in character.Spells)
            {
                var spell = SpellMagicInfo.FromSpellInfo(Character.Class, spells[(Spell)skill.Magic],skill.Level);
                _spellList.Add((Spell)skill.Magic, spell);
                SetEffect(spell);
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
                var info = SpellMagicInfo.FromSpellInfo(Character.Class, spells[skill]);
                _spellList.Add(skill, info);
                _newSpell.Add(skill);
                SetEffect(info);
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

        public List<SpellMagicInfo> SpellList => _spellList.Select(x => x.Value).ToList();
        public IDictionary<Spell, SpellMagicInfo> SpellDictionary => _spellList;

        public IEnumerable<Buff> BuffList => _buffs;

        public bool BufActive(SkillStates effect)
        {
            return _buffs.Any(x => x.State == effect);
        }

        public async void SendList()
        {
            var list = _spellList
                .Where(x => !x.Value.IsMLSkill)
                .Select((x,i) => new MuEmu.Network.Data.SpellDto { 
                    Index = (byte)i, 
                    Spell = (ushort)x.Key 
                })
                .ToArray();
            var list2 = _spellList
                .Where(x => x.Value.IsMLSkill)
                .Select((x, i) => new MasterSkillInfoDto
                {
                    MasterSkillLevel = (byte)x.Value.Level,
                    MasterSkillUIIndex = (byte)x.Value.MLInfo.Index,
                    MasterSkillCurValue = x.Value.MLSValue,
                    MasterSkillNextValue = x.Value.MLInfo.GetValue((short)(x.Value.Level + 1)),
                })
                .ToArray();
            
            await Player.Session.SendAsync(new SMasterLevelSkillListS9ENG { Skills = list2 });
            await Player.Session.SendAsync(new SSpells(0, list));
        }

        internal void ItemSkillAdd(Spell skill)
        {
            if (_spellList.ContainsKey(skill))
                return;

            var pos = _spellList.Count;
            var spells = ResourceCache.Instance.GetSkills();
            _spellList.Add(skill, SpellMagicInfo.FromSpellInfo(Character.Class, spells[skill]));

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

            var message = VersionSelector.CreateMessage<SMagicAttack>(spell, (ushort)Player.Session.ID, Target);

            if (Monster == null)
            {
                await Player
                    .Session
                    .SendAsync(message);

                Player.SendV2Message(message);
            }
        }

        public SkillStates[] ViewSkillStates => _buffs.Select(x => x.State).ToArray();

        internal void SetEffect(SpellMagicInfo skill)
        {
            if (!skill.IsMLSkill)
                return;

            if (string.IsNullOrWhiteSpace(skill.MLInfo.Property))
                return;

            this.Set(skill.MLInfo.Property, skill.MLSValue);
        }

        public async Task Save(GameContext db)
        {
            var changed = _spellList.Where(x => x.Value.Changed).Select(x => x.Value);
            if (!_newSpell.Any() && !_delSpell.Any() && !changed.Any())
                return;

            _delSpell.ForEach(x => _newSpell.Remove(x));

            await db.Spells.AddRangeAsync(_newSpell.Select(x => new SpellDto
            {
                CharacterId = Character.Id,
                Level = 1,
                Magic = (short)x,
            }));

            var del = _delSpell.Select(x => (short)x);
            var toDel = db.Spells.Where(x => x.CharacterId == Character.Id && del.Contains(x.Magic));
            db.Spells.RemoveRange(toDel);

            var array = changed.Select(x => (short)x.Number);
            var toUpdate = db.Spells.Where(x => x.CharacterId == Character.Id && array.Contains(x.Magic)).ToList();
            foreach(var dto in toUpdate)
            {
                var newDto = changed.First(x => x.Number == (Spell)dto.Magic);
                dto.Level = newDto.Level;
            }
            db.Spells.UpdateRange(toUpdate);

            _newSpell.Clear();
            _delSpell.Clear();
        }
    }
}
