using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Items")]
    public class ItemDbDto
    {
        [XmlElement("Item")]
        public ItemDto[] items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemDto
    {
        [XmlAttribute] public ushort Number { get; set; }

        [XmlAttribute] public string Name { get; set; }

        [XmlAttribute] public string Slot { get; set; }

        [XmlAttribute] public string Skill { get; set; }

        [XmlAttribute] public string Size { get; set; }

        [XmlAttribute] public string Option { get; set; }

        [XmlAttribute] public string Drop { get; set; }

        [XmlAttribute] public ushort Level { get; set; }

        [XmlAttribute] public ushort Defense { get; set; }

        [XmlAttribute] public ushort DefenseRate { get; set; }

        [XmlAttribute] public string Dmg { get; set; }

        [XmlAttribute] public int Speed { get; set; }

        [XmlAttribute] public byte Durability { get; set; }

        [XmlAttribute] public byte MagicDur { get; set; }

        [XmlAttribute] public byte MagicPower { get; set; }

        [XmlAttribute] public ushort NeededLevel { get; set; }

        [XmlAttribute] public ushort NeededStr { get; set; }

        [XmlAttribute] public ushort NeededAgi { get; set; }

        [XmlAttribute] public ushort NeededVit { get; set; }

        [XmlAttribute] public ushort NeededEne { get; set; }

        [XmlAttribute] public ushort NeededCmd { get; set; }

        [XmlAttribute] public string ReqClass { get; set; }
        [XmlAttribute] public int Zen { get; set; }
        
        [XmlAttribute] public string Attributes { get; set; }        
    }
}
