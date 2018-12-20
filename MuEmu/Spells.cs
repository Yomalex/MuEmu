using MU.DataBase;
using MuEmu.Data;
using MuEmu.Network.Data;
using MuEmu.Network.Game;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public enum Spell
    {
        Poison = 1,
        Meteorite,
        Lighting,
        FireBall,
        Flame,
        Teleport,
        Ice,
        Twister,
        EvilSpirit,
        Hellfire,
        PowerWave,
        AquaBeam,
        Cometfall,
        Inferno,
        TeleportAlly,
        SoulBarrier,
        EnergyBall,
        Defense,
        Falling_Slash,
        Lunge,
        Uppercut,
        Cyclone,
        Slash,
        Triple_Shot,
        Heal,
        GreaterDefense,
        GreaterDamage,
        Summon_Goblin = 30,
        Summon_StoneGolem,
        Summon_Assassin,
        Summon_EliteYeti,
        Summon_DarkKnight,
        Summon_Bali,
        Summon_Soldier,
        Decay = 38,
        IceStorm,
        Nova,
        TwistingSlash,
        RagefulBlow,
        DeathStab,
        CrescentMoonSlash,
        ManaGlaive,
        Starfall,
        Impale,
        GreaterFortitude,
        FireBreath,
        FlameofEvilMonster,
        IceArrow,
        Penetration,
        InfinityArrow,
        FireSlash = 55,
        PowerSlash,
        SpiralSlash,
        Force = 60,
        FireBurst,
        Earthshake,
        Summon,
        IncreaseCriticalDmg,
        ElectricSpike,
        ForceWave,
        Stern,
        CancelStern,
        SwellMana,
        Transparency,
        CancelTransparency,
        CancelMagic,
        ManaRays,
        FireBlast,
        PlasmaStorm = 76,
        ShadowArrow,
        FireScream,
        DrainLife,

        Sahamutt = 223,
        Neil,
        GhostPhantom,
    }
    public class Spells
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceCache));
        private Dictionary<Spell, SpellInfo> _spellList;
        private List<Buff> _buffs;

        public Player Player { get; }
        public Character Character { get; }

        public Spells(Character @char, CharacterDto character)
        {
            _spellList = new Dictionary<Spell, SpellInfo>();
            Player = @char.Player;
            Character = @char;
            _buffs = new List<Buff>();

            foreach (var spell in Character.BaseInfo.Spells)
            {
                Add(spell);
            }
        }

        private void Add(Spell skill)
        {
            var spells = ResourceCache.Instance.GetSkills();

            if(!_spellList.ContainsKey(skill))
                _spellList.Add(skill, spells[skill]);
        }

        public bool TryAdd(Spell skill)
        {
            var spells = ResourceCache.Instance.GetSkills();
            var spell = spells.Where(x => x.Key == skill).Select(x => x.Value).FirstOrDefault();
            if(spell == null)
            {
                throw new ArgumentException($"Can't find skill {skill}");
            }

            if(spell.Energy > Character.Energy)
            {
                Logger.Error("Insuficient Energy");
                return false;
            }

            Add(skill);

            return true;
        }

        public void Remove(Spell skill)
        {
            _spellList.Remove(skill);
        }

        public void Remove(SpellInfo skill)
        {
            Remove(skill.Number);
        }

        public IEnumerable<SpellInfo> SpellList => _spellList.Select(x => x.Value);

        public IEnumerable<Buff> BuffList => _buffs;

        public async void SendList()
        {
            var i = 0;
            var list = new List<SpellDto>();
            foreach(var magic in _spellList)
            {
                list.Add(new SpellDto
                {
                    Index = (byte)i,
                    Spell = (ushort)magic.Key,
                });
            }
            await Player.Session.SendAsync(new SSpells(0, list.ToArray()));
        }

        public async void SetBuff(SkillStates effect, TimeSpan time)
        {
            _buffs.Add(new Buff { State = effect });

            var m = new SViewSkillState(1, (ushort)Player.Session.ID, (byte)effect);

            await Player.Session.SendAsync(m);
            await Player.SendV2Message(m);
        }

        public async void ClearBuffTimeOut()
        {
            var b = _buffs.Where(x => x.EndAt < DateTimeOffset.Now);
            var rem = _buffs.Except(b);
            _buffs = b.ToList();

            foreach (var r in rem)
                await DelBuff(r.State);
        }

        public async Task DelBuff(SkillStates effect)
        {
            var m = new SViewSkillState(0, (ushort)Player.Session.ID, (byte)effect);

            await Player.Session.SendAsync(m);
            await Player.SendV2Message(m);
        }

        public SkillStates[] ViewSkillStates => _buffs.Select(x => x.State).ToArray();
    }
}
