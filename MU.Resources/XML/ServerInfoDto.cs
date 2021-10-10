using MU.Resources;
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
        [XmlElement] public string Name { get; set; } = "GameServer";
        [XmlElement] public int Code { get; set; } = 0;
        [XmlElement] public int Show { get; set; } = 1;
        [XmlElement] public string Lang { get; set; } = "es";
        [XmlElement] public bool AutoRegister { get; set; } = true;
        [XmlElement] public ServerSeason Season { get; set; } = ServerSeason.Season9Eng;

        [XmlElement("Connection")] public ConnectionInfoDto Connection { get; set; } = new ConnectionInfoDto();
        [XmlElement("Database")] public DatabaseInfoDto Database { get; set; } = new DatabaseInfoDto();
        [XmlElement("Client")] public ClientInfoDto Client { get; set; } = new ClientInfoDto();
        [XmlElement("GamePlay")] public GamePlayInfoDto GamePlay { get; set; } = new GamePlayInfoDto();
        [XmlElement("Files")] public FilesInfoDto Files { get; set; } = new FilesInfoDto();
        [XmlElement("Event")] public EventDto[] Events { get; set; } = Array.Empty<EventDto>();
    }

    [XmlType(AnonymousType = true)]
    public class ConnectionInfoDto
    {
        [XmlElement] public string IP { get; set; } = "127.0.0.1";
        [XmlElement] public int Port { get; set; } = 55901;
        [XmlElement] public string ConnectServerIP { get; set; } = "127.0.0.1";
        [XmlElement] public string APIKey { get; set; } = "2020110116";
    }

    [XmlType(AnonymousType = true)]
    public class DatabaseInfoDto
    {
        [XmlElement] public string DBIp { get; set; } = "127.0.0.1";
        [XmlElement] public string DataBase { get; set; } = "MuOnline";
        [XmlElement] public string BDUser { get; set; } = "root";
        [XmlElement] public string DBPassword { get; set; } = "1234";
    }

    [XmlType(AnonymousType = true)]
    public class ClientInfoDto
    {
        [XmlElement] public string Version { get; set; } = "10635";
        [XmlElement] public string Serial { get; set; } = "fughy683dfu7teqg";
        [XmlElement] public string CashShopVersion { get; set; } = "512.2014.124";
    }

    [XmlType(AnonymousType = true)]
    public class GamePlayInfoDto
    {
        [XmlElement] public float Experience { get; set; } = 10.0f;
        [XmlElement] public float Zen { get; set; } = 10.0f;
        [XmlElement] public int DropRate { get; set; } = 60;
        [XmlElement] public ushort MaxPartyLevelDifference { get; set; } = 400;
    }

    [XmlType(AnonymousType = true)]
    public class FilesInfoDto
    {
        [XmlElement] public string Monsters { get; set; } = "./Data/Monsters/Monster";
        [XmlElement] public string MonsterSetBase { get; set; } = "./Data/Monsters/MonsterSetBase";
        [XmlElement] public string MapServer { get; set; } = "./Data/MapServer.xml";
    }

    [XmlType(AnonymousType = true)]
    public class EventDto
    {
        [XmlAttribute] public string name { get; set; }
        [XmlAttribute] public byte rate { get; set; }
        [XmlAttribute] public bool active { get; set; }
        [XmlElement("Condition")] public EConditionDto[] Conditions { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class EConditionDto
    {
        [XmlAttribute] public int item { get; set; }
        [XmlAttribute] public byte itemLevel { get; set; }
        [XmlAttribute] public ushort mobMinLevel { get; set; }
        [XmlAttribute] public ushort mobMaxLevel { get; set; }
        [XmlAttribute] public Maps map { get; set; }
    }
}
