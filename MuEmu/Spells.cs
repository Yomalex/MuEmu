using MuEmu.Data;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
    public class Spells
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceCache));
        public Character Character { get; }
        private Dictionary<Spell, SpellInfo> _spellList;

        public Spells(Character @char)
        {
            Character = @char;

            switch(Character.Class)
            {
                case HeroClass.DarkWizard:
                case HeroClass.SoulMaster:
                case HeroClass.GranMaster:
                    Add(Spell.EnergyBall);
                    break;
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
    }
}
