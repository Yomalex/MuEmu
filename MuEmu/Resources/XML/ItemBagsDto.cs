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
}
