using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Data
{
    public class ItemInfo
    {
        public ushort Number { get; set; }
        public Point Size { get; set; }
        public byte Durability { get; set; }
        public bool Option { get; set; }
        public bool Drop { get; set; }
        public ushort Level { get; set; }
        public Point Damage { get; set; }
        public int Speed { get; set; }
        public ushort Str { get; set; }
        public ushort Agi { get; set; }
        public ushort Vit { get; set; }
        public ushort Ene { get; set; }
        public ushort Cmd { get; set; }
        public List<HeroClass> Classes { get; set; }
    }
}
