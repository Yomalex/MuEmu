using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Item")]
    public class WZItem
    {
        [XmlElement] public WZItemGroupWeapons[] Sword { get; set; }
        [XmlElement] public WZItemGroupWeapons[] Axe { get; set; }
        [XmlElement] public WZItemGroupWeapons[] Scepter { get; set; }
        [XmlElement] public WZItemGroupWeapons[] Spear { get; set; }
        [XmlElement] public WZItemGroupWeapons[] BowOrCrossbow { get; set; }
        [XmlElement] public WZItemGroupWeapons[] Staff { get; set; }
        [XmlElement] public WZItemGroupSet[] Shield { get; set; }
        [XmlElement] public WZItemGroupSet[] Helm { get; set; }
        [XmlElement] public WZItemGroupSet[] Armor { get; set; }
        [XmlElement] public WZItemGroupSet[] Pant { get; set; }
        [XmlElement] public WZItemGroupSet[] Gloves { get; set; }
        [XmlElement] public WZItemGroupSet[] Boots { get; set; }
        [XmlElement] public WZItemGroupWingsOrbsSeeds[] WingsOrbsSeeds { get; set; }
        [XmlElement] public WZItemGroupMisc[] Miscs  { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class WZItemGroupBasic
    {
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public byte Slot { get; set; }
        [XmlAttribute] public ushort Skill { get; set; }
        [XmlAttribute] public byte X { get; set; }
        [XmlAttribute] public byte Y { get; set; }
        [XmlAttribute] public byte Serial { get; set; }
        [XmlAttribute] public byte Option { get; set; }
        [XmlAttribute] public byte Drop { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public ushort Level { get; set; }

        [XmlIgnore] public Size Size => new Size(X, Y);
    }

    [XmlType(AnonymousType = true)]
    public class WZItemGroupWeapons : WZItemGroupBasic
    {
        [XmlAttribute] public ushort DmgMin { get; set; }
        [XmlAttribute] public ushort DmgMax { get; set; }
        [XmlAttribute] public ushort Speed { get; set; }
        [XmlAttribute] public ushort Durability { get; set; }
        [XmlAttribute] public ushort MagicDur { get; set; }
        [XmlAttribute] public ushort MagicPower { get; set; }
        [XmlAttribute] public ushort RequiredLvl { get; set; }
        [XmlAttribute] public ushort Str { get; set; }
        [XmlAttribute] public ushort Agi { get; set; }
        [XmlAttribute] public ushort Ene { get; set; }
        [XmlAttribute] public ushort Vit { get; set; }
        [XmlAttribute] public ushort Cmd { get; set; }
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public byte DW { get; set; }
        [XmlAttribute] public byte DK { get; set; }
        [XmlAttribute] public byte Elf { get; set; }
        [XmlAttribute] public byte MG { get; set; }
        [XmlAttribute] public byte DL { get; set; }
        [XmlAttribute] public byte SM { get; set; }

        [XmlIgnore] public Point Damage => new Point(DmgMin, DmgMax);
        [XmlIgnore] public List<HeroClass> Classes => GetClasses();
        private List<HeroClass> GetClasses()
        {
            var result = new List<HeroClass>();
            var list = new byte[] { DW, DK, Elf, MG, DL, SM };
            for(var i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0)
                    continue;

                var @class = (HeroClass)((i << 8) + (list[i] - 1));
                result.Add(@class);
            }

            return result;
        }
    }

    [XmlType(AnonymousType = true)]
    public class WZItemGroupSet : WZItemGroupBasic
    {
        [XmlAttribute] public ushort Def { get; set; }
        [XmlAttribute] public ushort DefRate { get; set; }
        [XmlAttribute] public ushort Durability { get; set; }
        [XmlAttribute] public ushort ReqLevel { get; set; }
        [XmlAttribute] public ushort Str { get; set; }
        [XmlAttribute] public ushort Agi { get; set; }
        [XmlAttribute] public ushort Ene { get; set; }
        [XmlAttribute] public ushort Vit { get; set; }
        [XmlAttribute] public ushort Command { get; set; }
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public byte DW { get; set; }
        [XmlAttribute] public byte DK { get; set; }
        [XmlAttribute] public byte Elf { get; set; }
        [XmlAttribute] public byte MG { get; set; }
        [XmlAttribute] public byte DL { get; set; }
        [XmlAttribute] public byte SM { get; set; }

        [XmlIgnore] public List<HeroClass> Classes => GetClasses();
        private List<HeroClass> GetClasses()
        {
            var result = new List<HeroClass>();
            var list = new byte[] { DW, DK, Elf, MG, DL, SM };
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0)
                    continue;

                var @class = (HeroClass)((i << 8) + (list[i] - 1));
                result.Add(@class);
            }

            return result;
        }
    }

    [XmlType(AnonymousType = true)]
    public class WZItemGroupWingsOrbsSeeds : WZItemGroupBasic
    {
        [XmlAttribute] public ushort Defense { get; set; }
        [XmlAttribute] public ushort Durability { get; set; }
        [XmlAttribute] public ushort RequiredLvl { get; set; }
        [XmlAttribute] public ushort Ene { get; set; }
        [XmlAttribute] public ushort Strength { get; set; }
        [XmlAttribute] public ushort Agi { get; set; }
        [XmlAttribute] public ushort Command { get; set; }
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public byte DW { get; set; }
        [XmlAttribute] public byte DK { get; set; }
        [XmlAttribute] public byte Elf { get; set; }
        [XmlAttribute] public byte MG { get; set; }
        [XmlAttribute] public byte DL { get; set; }
        [XmlAttribute] public byte SM { get; set; }
        [XmlIgnore] public List<HeroClass> Classes => GetClasses();
        private List<HeroClass> GetClasses()
        {
            var result = new List<HeroClass>();
            var list = new byte[] { DW, DK, Elf, MG, DL, SM };
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0)
                    continue;

                var @class = (HeroClass)((i << 8) + (list[i] - 1));
                result.Add(@class);
            }

            return result;
        }
    }

    [XmlType(AnonymousType = true)]
    public class WZItemGroupMisc : WZItemGroupBasic
    {
        [XmlAttribute] public ushort Durability { get; set; }
        [XmlAttribute] public ushort Ice { get; set; }
        [XmlAttribute] public ushort Poison { get; set; }
        [XmlAttribute] public ushort Light { get; set; }
        [XmlAttribute] public ushort Fire { get; set; }
        [XmlAttribute] public ushort Earth { get; set; }
        [XmlAttribute] public ushort Wind { get; set; }
        [XmlAttribute] public ushort Water { get; set; }
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public byte DW { get; set; }
        [XmlAttribute] public byte DK { get; set; }
        [XmlAttribute] public byte Elf { get; set; }
        [XmlAttribute] public byte MG { get; set; }
        [XmlAttribute] public byte DL { get; set; }
        [XmlAttribute] public byte SM { get; set; }
        [XmlIgnore] public List<HeroClass> Classes => GetClasses();
        private List<HeroClass> GetClasses()
        {
            var result = new List<HeroClass>();
            var list = new byte[] { DW, DK, Elf, MG, DL, SM };
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0)
                    continue;

                var @class = (HeroClass)((i << 8) + (list[i] - 1));
                result.Add(@class);
            }

            return result;
        }
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot("Item")]
    public class ItemBMDS16
    {
        [XmlElement] public ItemBMDS16GroupBasic[] Items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemBMDS16GroupBasic
    {
        [XmlAttribute] public ushort Number { get; set; }
        [XmlAttribute] public ushort Group { get; set; }
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public string ModelFolder { get; set; }//[260];
        [XmlAttribute] public string ModelName { get; set; }//[260];
        [XmlAttribute] public string Name { get; set; }//[64];
        [XmlAttribute] public byte KindA { get; set; }
        [XmlAttribute] public byte KindB { get; set; }
        [XmlAttribute] public byte Type { get; set; }
        [XmlAttribute] public byte TwoHands { get; set; }
        [XmlAttribute] public ushort Level { get; set; }
        [XmlAttribute] public byte Slot { get; set; }
        //[XmlAttribute] public byte GAP_564 { get; set; }
        [XmlAttribute] public Spell Skill { get; set; }
        [XmlAttribute] public byte Width { get; set; }
        [XmlAttribute] public byte Height { get; set; }
        [XmlIgnore] public Size Size => new Size(Width, Height);
        [XmlAttribute] public ushort DamageMin { get; set; }
        [XmlAttribute] public ushort DamageMax { get; set; }
        [XmlIgnore] public Point Damage => new Point(DamageMin, DamageMax);
        [XmlAttribute] public byte DefenseRate { get; set; }
        //[XmlAttribute] public byte GAP_575 { get; set; }
        [XmlAttribute] public ushort DefRate { get; set; }
        [XmlAttribute] public ushort MagicResistance { get; set; }
        [XmlAttribute] public byte Speed { get; set; }
        [XmlAttribute] public byte WalkSpeed { get; set; }
        [XmlAttribute] public byte Durability { get; set; }
        [XmlAttribute] public byte MagicDur { get; set; }
        [XmlAttribute] public ushort MagicPower { get; set; }
        [XmlAttribute] public ushort Str { get; set; }
        [XmlAttribute] public ushort Agi { get; set; }
        [XmlAttribute] public ushort Ene { get; set; }
        [XmlAttribute] public ushort Vit { get; set; }
        [XmlAttribute] public ushort Cmd { get; set; }
        [XmlAttribute] public ushort Lvl { get; set; }
        [XmlAttribute] public byte ItemValue { get; set; }
        //[XmlAttribute] public byte GAP3_601 { get; set; }
        [XmlAttribute] public int Zen { get; set; }
        [XmlAttribute] public byte SetAttr { get; set; }
        [XmlAttribute] public byte DW { get; set; }
        [XmlAttribute] public byte DK { get; set; }
        [XmlAttribute] public byte FE { get; set; }
        [XmlAttribute] public byte MG { get; set; }
        [XmlAttribute] public byte DL { get; set; }
        [XmlAttribute] public byte SU { get; set; }
        [XmlAttribute] public byte RF { get; set; }
        [XmlAttribute] public byte GL { get; set; }
        [XmlAttribute] public byte RW { get; set; }
        [XmlAttribute] public byte SL { get; set; }
        [XmlAttribute] public byte GC { get; set; }
        [XmlIgnore] public List<HeroClass> Classes => GetClasses();
        private List<HeroClass> GetClasses()
        {
            var result = new List<HeroClass>();
            var list = new byte[] { DW, DK, FE, MG, DL, SU, RF, GL, RW, SL, GC };
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0)
                    continue;

                var @class = (HeroClass)((i * 0x10) + (list[i] - 1));
                result.Add(@class);
            }

            return result;
        }
        [XmlAttribute] public byte Resist0 { get; set; } //[7]
        [XmlAttribute] public byte Resist1 { get; set; } //[7]
        [XmlAttribute] public byte Resist2 { get; set; } //[7]
        [XmlAttribute] public byte Resist3 { get; set; } //[7]
        [XmlAttribute] public byte Resist4 { get; set; } //[7]
        [XmlAttribute] public byte Resist5 { get; set; } //[7]
        [XmlAttribute] public byte Resist6 { get; set; } //[7]
        public List<AttributeType> Attributes => GetAttributes();
        private List<AttributeType> GetAttributes()
        {
            var result = new List<AttributeType>();
            var list = new byte[] { Resist0, Resist1, Resist2, Resist3, Resist4, Resist5, Resist6 };

            for(var i = 0; i < list.Length; i++)
            {
                if (list[i] != 0)
                    result.Add((AttributeType)i);
            }

            return result;
        }
        [XmlAttribute] public byte Dump { get; set; }
        [XmlAttribute] public byte Transaction { get; set; }
        [XmlAttribute] public byte PersonalStore { get; set; }
        [XmlAttribute] public byte Warehouse { get; set; }
        [XmlAttribute] public byte SellNpc { get; set; }
        [XmlAttribute] public byte Expensive { get; set; }
        [XmlAttribute] public byte Repair { get; set; }
        [XmlAttribute] public byte MaxStack { get; set; }
        [XmlAttribute] public byte PcFlag { get; set; }
        [XmlAttribute] public byte MuunFlag { get; set; }
        [XmlAttribute] public byte UnkFlag0 { get; set; }
        [XmlAttribute] public byte UnkFlag1 { get; set; }
        [XmlAttribute] public byte UnkFlag2 { get; set; }
        //[XmlAttribute] public byte GAP_673 { get; set; }
        //[XmlAttribute] public ushort Unk_End[3] { get; set; }
    }
}
