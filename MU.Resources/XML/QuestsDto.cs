using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Quests")]
    public class QuestsDto
    {
        [XmlElement] public QuestDto[] Quest { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class QuestDto
    {
        [XmlAttribute] public int Type { get; set; }
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public int SubType { get; set; }
        [XmlAttribute] public ushort NPC { get; set; }
        [XmlAttribute] public string Name { get; set; }

        [XmlElement("Detail")] public DetailDto[] Details { get; set; }
        [XmlElement("Condition")] public ConditionDto[] Conditions { get; set; }
    }

    [XmlType(AnonymousType =true)]
    public class DetailDto
    {
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public string Classes { get; set; }
        [XmlElement] public NeededItemDto[] NeededItem { get; set; }
        [XmlElement] public NeededMonsterDto NeededMonster { get; set; }
        [XmlElement] public RewardDto Reward { get; set; }
        [XmlElement] public MessageDto Message { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class NeededItemDto
    {
        [XmlAttribute] public int Type { get; set; }
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public int Level { get; set; }
        [XmlAttribute] public int Count { get; set; }
        [XmlAttribute] public string Monster { get; set; }
        [XmlAttribute] public int Drop { get; set; }
    }

    public class NeededMonsterDto
    {
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public int Count { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RewardDto
    {
        [XmlAttribute] public string Type { get; set; }
        [XmlAttribute] public byte SubType { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MessageDto
    {
        [XmlAttribute] public int LinkConditionIndex { get; set; }
        [XmlAttribute] public ushort BeforeReg { get; set; }
        [XmlAttribute] public ushort AfterReg { get; set; }
        [XmlAttribute] public ushort CompleteQuest { get; set; }
        [XmlAttribute] public ushort ClearQuest { get; set; }
    }

    [XmlType(AnonymousType =true)]
    public class ConditionDto
    {
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public int NeededQuest { get; set; }
        [XmlAttribute] public ushort MinLevel { get; set; }
        [XmlAttribute] public ushort MaxLevel { get; set; }
        [XmlAttribute] public int NeedStr { get; set; }
        [XmlAttribute] public int Cost { get; set; }
        [XmlAttribute] public int Message { get; set; }
    }
}
