using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Characters")]
    public class CharactersInfoDto
    {
        [XmlElement()] public CharacterInfoDto[] Character { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CharacterInfoDto
    {
        [XmlAttribute] public string BaseClass { get; set; }
        [XmlAttribute] public string Map { get; set; }
        [XmlAttribute] public int Level { get; set; }
        [XmlElement] public StatsInfoDto Stats { get; set; }
        [XmlElement] public AttriInfoDto Attributes { get; set; }
        [XmlElement] public EquipamentInfoDto[] Equipament { get; set; } = Array.Empty<EquipamentInfoDto>();
        [XmlElement] public int[] Skill { get; set; } = Array.Empty<int>();
    }

    [XmlType(AnonymousType = true)]
    public class StatsInfoDto
    {
        [XmlAttribute] public int Str { get; set; }
        [XmlAttribute] public int Agi { get; set; }
        [XmlAttribute] public int Vit { get; set; }
        [XmlAttribute] public int Ene { get; set; }
        [XmlAttribute] public int Cmd { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class AttriInfoDto
    {
        [XmlAttribute] public float Life { get; set; }
        [XmlAttribute] public float Mana { get; set; }
        [XmlAttribute] public float LevelLife { get; set; }
        [XmlAttribute] public float LevelMana { get; set; }
        [XmlAttribute] public float VitalityToLife { get; set; }
        [XmlAttribute] public float EnergyToMana { get; set; }

        [XmlAttribute] public float StrToBP { get; set; } = 0.2f;
        [XmlAttribute] public float AgiToBP { get; set; } = 0.4f;
        [XmlAttribute] public float VitToBP { get; set; } = 0.3f;
        [XmlAttribute] public float EneToBP { get; set; } = 0.2f;
        [XmlAttribute] public float CmdToBP { get; set; } = 0.0f;
    }

    [XmlType(AnonymousType = true)]
    public class EquipamentInfoDto
    {
        [XmlAttribute] public int Slot { get; set; }
        [XmlAttribute] public int Type { get; set; }
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public int Level { get; set; }
    }
}
