using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.Game
{
    [XmlRoot("root")]
    [XmlType(AnonymousType = true)]
    public class WZItemBagDto
    {
        [XmlElement] public WZIBBagDto[] BagDtos { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class WZIBBagDto
    {
        [XmlAttribute] public byte Type { get; set; }
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public byte Lvl { get; set; }
        [XmlAttribute] public byte Skill { get; set; }
        [XmlAttribute] public byte Luck { get; set; }
        [XmlAttribute] public byte Option { get; set; }
    }
}
