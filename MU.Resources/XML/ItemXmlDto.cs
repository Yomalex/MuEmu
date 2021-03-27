using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    public class ItemXmlDto
    {
        [XmlAttribute] public int Number { get; set; }
        [XmlAttribute] public byte Plus { get; set; }
        [XmlAttribute] public bool Luck { get; set; }
        [XmlAttribute] public bool Skill { get; set; }
        [XmlAttribute] public byte Option28 { get; set; }
        [XmlAttribute] public byte Durability { get; set; }
        [XmlAttribute] public byte OptionExe { get; set; }
        [XmlAttribute] public byte FreeSockets { get; set; }
        [XmlAttribute] public uint BuyPrice { get; set; }
    }
}
