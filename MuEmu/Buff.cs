using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class Buff
    {
        public DateTimeOffset EndAt { get; set; }

        public int DefenseAdd { get; set; }
        public float DefenseAddRate { get; set; }

        public int DefenseRed { get; set; }
        public float DefenseRedRate { get; set; }

        public int AttackAdd { get; set; }
        public float AttackAddRate { get; set; }

        public int LifeAdd { get; set; }
        public float LifeAddRate { get; set; }

        public float IgnoreDefenseRate { get; set; }

        public Item Source { get; set; }

        public SkillStates State { get; set; }
    }
}
