using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("ItemBags")]
    public class ItemBagsDto
    {
        [XmlElement("ItemBag")]
        public ItemBagDto[] ItemBags { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemBagDto
    {
        [XmlAttribute]
        public ushort Item { get; set; }
        [XmlAttribute]
        public ushort Plus { get; set; } = 0xffff;
        [XmlAttribute]
        public ushort LevelMin { get; set; } = 0;
        [XmlAttribute]
        public string Bag { get; set; }
    }

    [XmlRoot("ItemList")]
    [XmlType(AnonymousType = true)]
    public class BagDto
    {
        [XmlAttribute] public ushort Number { get; set; }
        [XmlAttribute] public ushort Plus { get; set; } = 0xffff;
        [XmlAttribute] public ushort LevelMin { get; set; } = 0;
        [XmlAttribute] public ushort DropItemCount { get; set; }
        [XmlAttribute] public ushort DropItemRate { get; set; }
        [XmlAttribute] public ushort DropZenRate { get; set; }
        [XmlAttribute] public int MinZen { get; set; }
        [XmlAttribute] public int MaxZen { get; set; }

        [XmlElement("Item")] public ItemInBagDto[] Item { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemInBagDto
    {
        [XmlAttribute] public ushort Number { get; set; }
        [XmlAttribute] public byte MinLevel { get; set; }
        [XmlAttribute] public byte MaxLevel { get; set; }
        [XmlAttribute] public bool Luck { get; set; }
        [XmlAttribute] public bool Skill { get; set; }
        [XmlAttribute] public byte MinOption { get; set; }
        [XmlAttribute] public byte MaxOption { get; set; }
        [XmlAttribute] public byte MinExcellent { get; set; }
        [XmlAttribute] public byte MaxExcellent { get; set; }
    }
}
