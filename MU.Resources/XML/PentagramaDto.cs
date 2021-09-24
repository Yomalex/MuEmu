using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Pentagrama")]
    public class PentagramaDto
    {
        [XmlElement("Monster")]
        public PentagramaMonsterDto[] Monsters { get; set; }

        [XmlElement("Socket")]
        public PentagramaSocketDto[] Sockets { get; set; }

        [XmlElement("Item")]
        public PentagramaItemDto[] Items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaItemDto
    {
        [XmlAttribute] public int Number { get; set; }
        [XmlAttribute] public PIGrade Grade { get; set; }
        [XmlAttribute] public string Option { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaOptionDto
    {
        [XmlAttribute] public int Number { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlElement] public PentagramaErrtelDto[] Errtel { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaErrtelDto
    {
        [XmlAttribute] public int Type { get; set; }
        [XmlAttribute] public int Rank { get; set; }
        [XmlAttribute] public int Level { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaSocketDto
    {
        [XmlAttribute] public int Count { get; set; }
        [XmlAttribute] public int OpenRate { get; set; } = 0;
        [XmlElement("Rate")]
        public PentagramaSocketRatesDto[] Rates { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaSocketRatesDto
    {
        [XmlAttribute] public int Set { get; set; } = 0;
        [XmlAttribute] public int Slot1 { get; set; } = 0;
        [XmlAttribute] public int Slot2 { get; set; } = 0;
        [XmlAttribute] public int Slot3 { get; set; } = 0;
        [XmlAttribute] public int Slot4 { get; set; } = 0;
        [XmlAttribute] public int Slot5 { get; set; } = 0;
    }

    [XmlType(AnonymousType = true)]
    public class PentagramaMonsterDto
    {
        [XmlAttribute]
        public ushort Number { get; set; }

        [XmlElement("Item")]
        public PentagramaMonsterItemDto[] Items { get; set; }

        [XmlAttribute]
        public int Rate { get; set; }
    }

    public class PentagramaMonsterItemDto
    {
        [XmlAttribute]
        public ushort Number { get; set; }

        [XmlAttribute]
        public ushort Rate { get; set; }
    }
}
