using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.Game
{
    [XmlRoot("root")]
    [XmlType(AnonymousType = true)]
    public class DSItemBagDto
    {
        [XmlElement] public DSIBDropInfoDto[] Section0 { get; set; }
        [XmlElement] public DSIBEventInfoDto[] Section1 { get; set; }
        [XmlElement] public DSIBBagInfoDto[] Section2 { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DSIBDropInfoDto
    {
        [XmlAttribute] public byte MapIndex { get; set; }
        [XmlAttribute] public byte DropFlag { get; set; }
        [XmlAttribute] public ushort MinMonsterLevel { get; set; }
        [XmlAttribute] public ushort MaxMonsterLevel { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DSIBEventInfoDto
    {
        [XmlAttribute] public string EventName { get; set; }
        [XmlAttribute] public int DropZen { get; set; }
        [XmlAttribute] public byte BoxType { get; set; }
        [XmlAttribute] public ushort BoxIndex { get; set; }
        [XmlAttribute] public byte BoxLevel { get; set; }
        [XmlAttribute] public short BoxDropRate { get; set; }
        [XmlAttribute] public short ItemDropRate { get; set; }
        [XmlAttribute] public short ExRate { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DSIBBagInfoDto
    {
        //minLvl	maxLvl	Skill	Luck	maxZ28	maxExOpt
        [XmlAttribute] public byte Type { get; set; }
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public byte minLvl { get; set; }
        [XmlAttribute] public byte maxLvl { get; set; }
        [XmlAttribute] public byte Skill { get; set; }
        [XmlAttribute] public byte Luck { get; set; }
        [XmlAttribute] public byte maxZ28 { get; set; }
        [XmlAttribute] public byte maxExOpt { get; set; }
    }
}
