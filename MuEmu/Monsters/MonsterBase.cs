using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Monsters
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Monster")]
    public class XmlMonsterInfo
    {
        [XmlElement("Info")] public MonsterBase[] Monsters { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MonsterBase
    {
        [XmlAttribute] public ushort Monster { get; set; }
        [XmlAttribute] public int Rate { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public ushort Level { get; set; }
        [XmlAttribute] public int HP { get; set; }
        [XmlAttribute] public int MP { get; set; }
        [XmlAttribute] public int DmgMin { get; set; }
        [XmlAttribute] public int DmgMax { get; set; }
        [XmlAttribute] public int Defense { get; set; }
        [XmlAttribute] public int MagicDefense { get; set; }
        [XmlAttribute] public int Attack { get; set; }
        [XmlAttribute] public int Success { get; set; }
        [XmlAttribute] public int MoveRange { get; set; }
        [XmlAttribute] public Spell Spell { get; set; }
        [XmlAttribute] public int AttackRange { get; set; }
        [XmlAttribute] public int ViewRange { get; set; }
        [XmlAttribute] public int MoveSpeed { get; set; }
        [XmlAttribute] public int AttackSpeed { get; set; }
        [XmlAttribute] public int RegenTime { get; set; }
        [XmlAttribute] public int Attribute { get; set; }
        [XmlAttribute] public int ItemRate { get; set; }
        [XmlAttribute] public int M_Rate { get; set; }
        [XmlAttribute] public int MaxItem { get; set; }
        [XmlAttribute] public int Skill { get; set; }
        [XmlAttribute] public int Ice_Resistance { get; set; }
        [XmlAttribute] public int Possion_Resistance { get; set; }
        [XmlAttribute] public int Lightning_Resistance { get; set; }
        [XmlAttribute] public int Fire_Resistance { get; set; }
        public int MainAttribute { get; set; } = 0;
        public int AttributePattern { get; set; } = 0;
        public int PentagramDamageMin { get; set; } = 0;
        public int PentagramDamageMax { get; set; } = 0;
        public int PentagramAttackRate { get; set; } = 0;
        public int PentagramDefenseRate { get; set; } = 0;
        public int PentagramDefense { get; set; } = 7;
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot("SetBase")]
    public class XmlMonsterSetBase
    {
        [XmlElement("NPC")] public MonsterSet[] NPCs { get; set; }
        [XmlElement("Spot")] public MonsterSpot[] Spots { get; set; }
        [XmlElement("Normal")] public MonsterSet[] Normal { get; set; }
        [XmlElement("Golden")] public MonsterSpot[] Golden { get; set; }
        [XmlElement("BloodCastle")] public MonsterSet[] BloodCastles { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MonsterSet
    {
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public Maps Map { get; set; }
        [XmlAttribute] public byte Radio { get; set; }
        [XmlAttribute] public byte PosX { get; set; }
        [XmlAttribute] public byte PosY { get; set; }
        [XmlAttribute] public sbyte Dir { get; set; }

        [XmlText]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MonsterSpot
    {
        [XmlAttribute] public ushort Type { get; set; }
        [XmlAttribute] public Maps Map { get; set; }
        [XmlAttribute] public byte Radio { get; set; }
        [XmlAttribute] public byte PosX { get; set; }
        [XmlAttribute] public byte PosY { get; set; }
        [XmlAttribute] public byte PosX2 { get; set; }
        [XmlAttribute] public byte PosY2 { get; set; }
        [XmlAttribute] public sbyte Dir { get; set; }
        [XmlAttribute] public byte Quant { get; set; }

        [XmlText]
        public string Name { get; set; }

        public Element Element { get; set; }
    }
}
