using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Server")]
    public class ServerInfoDto
    {
        [XmlElement] public string Version { get; set; } = "10635";
        [XmlElement] public string Serial { get; set; } = "fughy683dfu7teqg";
        [XmlElement] public string Name { get; set; } = "GameServer";
        [XmlElement] public int Code { get; set; }
        [XmlElement] public int Show { get; set; } = 1;
        [XmlElement] public string IP { get; set; } = "127.0.0.1";
        [XmlElement] public int Port { get; set; } = 55901;
        [XmlElement] public string ConnectServerIP { get; set; } = "127.0.0.1";
        [XmlElement] public float Experience { get; set; } = 10.0f;
        [XmlElement] public bool AutoRegistre { get; set; } = true;
        [XmlElement] public float Zen { get; set; } = 10.0f;
        [XmlElement] public int DropRate { get; set; } = 60;
        [XmlElement] public string Lang { get; set; } = "es";
        [XmlElement] public string MonsterSetBase { get; set; } = "/Monsters/MonsterSetBase.txt";

        [XmlElement] public string DBIp { get; set; } = "127.0.0.1";
        [XmlElement] public string DataBase { get; set; } = "MuOnline";
        [XmlElement] public string BDUser { get; set; } = "root";
        [XmlElement] public string DBPassword { get; set; } = "1234";
        [XmlElement] public bool Rijndael { get; set; } = false;
        [XmlElement] public EventDto BoxOfRibbon { get; set; } = new EventDto();
        [XmlElement] public EventDto Medals { get; set; } = new EventDto();
        [XmlElement] public EventDto HeartOfLove { get; set; } = new EventDto();
        [XmlElement] public EventDto EventChip { get; set; } = new EventDto();
        [XmlElement] public EventDto FireCracker { get; set; } = new EventDto();
        [XmlElement] public EventDto Heart { get; set; } = new EventDto();
        [XmlElement] public EventDto StarOfXMas { get; set; } = new EventDto();
    }

    [XmlType(AnonymousType = true)]
    public class EventDto
    {
        [XmlAttribute] public byte rate { get; set; }
        [XmlAttribute] public bool active { get; set; }
    }
}
