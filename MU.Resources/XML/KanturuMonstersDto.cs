using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Stages")]
    public class KanturuStagesDto
    {
        [XmlElement("Stage")] public KanturuStageDto[] Stages { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class KanturuStageDto
    {
        [XmlAttribute] public int Number { get; set; }
        [XmlElement("Monster")] public KanturuMonsterDto[] Monsters { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class KanturuMonsterDto
    {
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public int Map { get; set; }
        [XmlAttribute] public byte Radio { get; set; }
        [XmlAttribute] public byte PosX { get; set; }
        [XmlAttribute] public byte PosY { get; set; }
        [XmlAttribute] public sbyte Dir { get; set; }
    }
}
