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
        public ushort ReqLevel { get; set; }
        public Point Damage { get; set; }
        public ushort Mana { get; set; }
        public ushort BP { get; set; }
        public byte Distance { get; set; }
        public uint Delay { get; set; }
        public ushort Energy { get; set; }
        public ushort Command { get; set; }
        public byte Attribute { get; set; }
        public ushort Type { get; set; }
        public byte UseType { get; set; }
        public int Brand { get; set; }
        public int KillCount { get; set; }
        public List<int> Status { get; set; }
        public List<HeroClass> Classes { get; set; }
        public int Rank { get; set; }
        public int Group { get; set; }
        public int MasterP { get; set; }
        public int AG { get; set; }
        public int SD { get; set; }
        public int Duration { get; set; }
        public ushort Str { get; set; }
        public ushort Agility { get; set; }
        public ushort Icon { get; set; }
        public byte UseType2 { get; set; }
        public ushort Item { get; set; }
        public byte IsDamage { get; set; }
    }
}
