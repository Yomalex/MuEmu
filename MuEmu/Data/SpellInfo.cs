using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Data
{
    public class SpellInfo
    {
        public Spell Number { get; set; }
        public string Name { get; set; }
        public ushort Mana { get; set; }
        public Point Damage { get; set; }
        public ushort Energy { get; set; }
        public List<HeroClass> Classes { get; set; }
    }
}
