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
        [XmlElement] public string Version { get; set; }
        [XmlElement] public string Serial { get; set; }
        [XmlElement] public string Name { get; set; }
        [XmlElement] public int Code { get; set; }
        [XmlElement] public int Show { get; set; }
        [XmlElement] public string IP { get; set; }
        [XmlElement] public int Port { get; set; }
        [XmlElement] public string ConnectServerIP { get; set; }
        [XmlElement] public float Experience { get; set; }
        [XmlElement] public bool AutoRegistre { get; set; }
        [XmlElement] public float Zen { get; set; }
        [XmlElement] public int DropRate { get; set; }
        [XmlElement] public string Lang { get; set; } = "es";

        [XmlElement] public string DBIp { get; set; }
        [XmlElement] public string DataBase { get; set; }
        [XmlElement] public string BDUser { get; set; }
        [XmlElement] public string DBPassword { get; set; }
        [XmlElement] public bool Rijndael { get; set; }
        [XmlElement] public EventDto BoxOfRibbon { get; set; }
        [XmlElement] public EventDto Medals { get; set; }
        [XmlElement] public EventDto HeartOfLove { get; set; }
        [XmlElement] public EventDto EventChip { get; set; }
        [XmlElement] public EventDto FireCracker { get; set; }
        [XmlElement] public EventDto Heart { get; set; }        
        [XmlElement] public EventDto StarOfXMas { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class EventDto
    {
        [XmlAttribute] public byte rate { get; set; }
        [XmlAttribute] public bool active { get; set; }
    }
}
