using MU.Resources;
using MuEmu.Resources.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources
{
    public class CharacterInfo
    {
        public HeroClass Class { get; set; }
        public Maps Map { get; set; }
        public ushort Level { get; set; }
        public StatsInfo Stats { get; set; }
        public AttriInfo Attributes { get; set; }
        public Dictionary<ushort, Item> Equipament { get; set; }
        public Spell[] Spells { get; set; }
    }
}
