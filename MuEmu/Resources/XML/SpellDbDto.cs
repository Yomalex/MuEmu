using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Skills")]
    public class SpellDbDto
    {
        [XmlElement("Skill")]
        public SkillDto[] skills { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class SkillDto
    {
        [XmlAttribute] public ushort Number { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string Dmg { get; set; }
        [XmlAttribute] public ushort Mana { get; set; }
        [XmlAttribute] public ushort Skill { get; set; }
        [XmlAttribute] public ushort Energy { get; set; }
        [XmlAttribute] public string Classes { get; set; }
    }
}
