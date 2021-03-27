using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("MapServer")]
    public class MapServerDto
    {
        [XmlElement("Group")] public MSGroupDto[] Groups { get; set; } = Array.Empty<MSGroupDto>();
    }

    [XmlType(AnonymousType = true)]
    public class MSGroupDto
    {
        [XmlElement("GameServer")] public MSGameServerDto[] GameServers { get; set; } = Array.Empty<MSGameServerDto>();
    }

    [XmlType(AnonymousType = true)]
    public class MSGameServerDto
    {
        [XmlAttribute] public ushort Code { get; set; }
        [XmlAttribute] public string IP { get; set; }
        [XmlAttribute] public ushort Port { get; set; }
        [XmlAttribute] public int Default { get; set; }

        [XmlElement("Map")] public MSMapDto[] Maps { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MSMapDto
    {
        [XmlAttribute] public Maps ID { get; set; }
        [XmlAttribute] public bool MoveAbleOption { get; set; }
        [XmlAttribute] public int Dest { get; set; }
    }
}
