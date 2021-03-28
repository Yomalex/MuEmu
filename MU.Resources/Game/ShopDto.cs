using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.Game
{
    [XmlRoot("Shop")]
    [XmlType(AnonymousType = true)]
    public class ShopInfoDto
    {
        [XmlAttribute] public string Description { get; set; }
        [XmlElement("Item")] public ShopRowDto[] Item { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ShopRowDto
    {
        [XmlAttribute] public byte Type { get; set; }
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public byte Plus { get; set; }
        [XmlAttribute] public byte Durability { get; set; }
        [XmlAttribute] public bool Skill { get; set; }
        [XmlAttribute] public bool Luck { get; set; }
        [XmlAttribute] public byte Option { get; set; }
        [XmlAttribute] public byte Excellent { get; set; }
    }
}
