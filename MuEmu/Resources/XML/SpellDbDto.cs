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
        [XmlAttribute] public ushort ReqLevel { get; set; }
        [XmlAttribute] public string Dmg { get; set; }
        [XmlAttribute] public ushort Mana { get; set; }
        [XmlAttribute] public ushort BP { get; set; }
        [XmlAttribute] public byte Distance { get; set; }
        [XmlAttribute] public uint Delay { get; set; }
        [XmlAttribute] public ushort Energy { get; set; }
        [XmlAttribute] public ushort Command { get; set; }
        [XmlAttribute] public byte Attribute { get; set; }
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public byte UseType { get; set; }
        [XmlAttribute] public int Brand { get; set; }
        [XmlAttribute] public int KillCount { get; set; }
        [XmlAttribute] public string Status { get; set; }
        [XmlAttribute] public string Classes { get; set; }
        [XmlAttribute] public int Rank { get; set; }
        [XmlAttribute] public int Group { get; set; }
        [XmlAttribute] public int MasterP { get; set; }
        [XmlAttribute] public int AG { get; set; }
        [XmlAttribute] public int SD { get; set; }
        [XmlAttribute] public int Duration { get; set; }
        [XmlAttribute] public ushort Str { get; set; }
        [XmlAttribute] public ushort Agility { get; set; }
        [XmlAttribute] public ushort Icon { get; set; }
        [XmlAttribute] public byte UseType2 { get; set; }
        [XmlAttribute] public ushort Item { get; set; }
        [XmlAttribute] public byte IsDamage { get; set; }
    }
}
