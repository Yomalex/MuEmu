using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Quests")]
    public class QuestEXPDto
    {
        [XmlElement] public QuestNPCDto[] QuestList { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class QuestNPCDto
    {
        [XmlAttribute] public ushort Index { get; set; }

        [XmlElement] public QuestNPCInfoDto[] QuestInfo { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class QuestNPCInfoDto
    {
        [XmlAttribute] public uint Episode { get; set; }
        [XmlAttribute] public uint ReqEpisode { get; set; }
        [XmlAttribute] public ushort MinLevel { get; set; }
        [XmlAttribute] public ushort MaxLevel { get; set; }
        [XmlElement] public QuestNPCStateDto[] QuestState { get; set; }
        [XmlAttribute] public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class QuestNPCStateDto
    {
        [XmlAttribute] public uint State { get; set; }
        [XmlAttribute] public AskType Type { get; set; } = AskType.None;
        [XmlAttribute] public uint RewardEXP { get; set; } = 0;
        [XmlAttribute] public uint RewardZEN { get; set; } = 0;
        [XmlAttribute] public uint RewardGENS { get; set; } = 0;
        [XmlAttribute] public HeroClass Class { get; set; } = HeroClass.End;
        [XmlAttribute] public ushort Select1 { get; set; } = ushort.MaxValue;
        [XmlAttribute] public ushort Select2 { get; set; } = ushort.MaxValue;
        [XmlAttribute] public ushort Select3 { get; set; } = ushort.MaxValue;
        [XmlElement] public QuestItemDto[] Item { get; set; }
        [XmlElement] public QuestItemDto[] RewardItem { get; set; }
        [XmlElement] public QuestItemDto[] Monster { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class QuestItemDto
    {
        [XmlAttribute] public byte Type { get; set; }
        [XmlAttribute] public ushort Index { get; set; }
        [XmlAttribute] public byte Level { get; set; }
        [XmlAttribute] public bool Skill { get; set; }
        [XmlAttribute] public byte Option { get; set; }
        [XmlAttribute] public byte Excellent { get; set; }
        [XmlAttribute] public byte Count { get; set; }
    }
}
