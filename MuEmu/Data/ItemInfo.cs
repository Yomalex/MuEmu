using MU.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Data
{
    public class ItemInfo
    {
        public ushort Number { get; set; }
        public Spell Skill { get; set; }
        public Size Size { get; set; }
        public byte Durability { get; set; }
        public bool Option { get; set; }
        public bool Drop { get; set; }
        public string Name { get; set; }
        public ushort Level { get; set; }
        public Point Damage { get; set; }
        public byte MagicDur { get; set; }
        public byte MagicPower { get; set; }
        public ushort Def { get; set; }
        public ushort DefRate { get; set; }
        public int Speed { get; set; }
        public ushort ReqLevel { get; set; }
        public ushort Str { get; set; }
        public ushort Agi { get; set; }
        public ushort Vit { get; set; }
        public ushort Ene { get; set; }
        public ushort Cmd { get; set; }
        public List<HeroClass> Classes { get; set; }
        public int Zen { get; set; }
        public List<AttributeType> Attributes { get; set; }
        public byte MaxStack { get; set; }
        public ItemNumber OnMaxStack { get; internal set; } = ItemNumber.Invalid;
        public bool IsMount { get; set; }
        public ushort Skin { get; set; }
        public StorageID Inventory { get; internal set; } = StorageID.Inventory;
    }
}
