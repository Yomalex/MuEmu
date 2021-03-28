using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.Game
{
    [XmlRoot("JewelOfHarmony")]
    public class JOHDto
    {
        [XmlElement] public JOHSectionDto[] Section0 { get; set; }
        [XmlElement] public JOHSectionDto[] Weapon { get; set; }
        [XmlElement] public JOHSectionDto[] Staff { get; set; }
        [XmlElement] public JOHSectionDto[] Defense { get; set; }
    }

    public class JOHSectionDto
    {
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public int Weighted { get; set; }
        [XmlAttribute] public int Level { get; set; }
        [XmlAttribute] public int Level0 { get; set; }
        [XmlAttribute] public int Zen0 { get; set; }
        [XmlAttribute] public int Level1 { get; set; }
        [XmlAttribute] public int Zen1 { get; set; }
        [XmlAttribute] public int Level2 { get; set; }
        [XmlAttribute] public int Zen2 { get; set; }
        [XmlAttribute] public int Level3 { get; set; }
        [XmlAttribute] public int Zen3 { get; set; }
        [XmlAttribute] public int Level4 { get; set; }
        [XmlAttribute] public int Zen4 { get; set; }
        [XmlAttribute] public int Level5 { get; set; }
        [XmlAttribute] public int Zen5 { get; set; }
        [XmlAttribute] public int Level6 { get; set; }
        [XmlAttribute] public int Zen6 { get; set; }
        [XmlAttribute] public int Level7 { get; set; }
        [XmlAttribute] public int Zen7 { get; set; }
        [XmlAttribute] public int Level8 { get; set; }
        [XmlAttribute] public int Zen8 { get; set; }
        [XmlAttribute] public int Level9 { get; set; }
        [XmlAttribute] public int Zen9 { get; set; }
        [XmlAttribute] public int Level10 { get; set; }
        [XmlAttribute] public int Zen10 { get; set; }
        [XmlAttribute] public int Level11 { get; set; }
        [XmlAttribute] public int Zen11 { get; set; }
        [XmlAttribute] public int Level12 { get; set; }
        [XmlAttribute] public int Zen12 { get; set; }
        [XmlAttribute] public int Level13 { get; set; }
        [XmlAttribute] public int Zen13 { get; set; }
    }
}
