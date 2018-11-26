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
        [XmlElement] public string Name { get; set; }
        [XmlElement] public int Code { get; set; }
        [XmlElement] public int Show { get; set; }
        [XmlElement] public string IP { get; set; }
        [XmlElement] public int Port { get; set; }
        [XmlElement] public string ConnectServerIP { get; set; }
        [XmlElement] public float Experience { get; set; }
    }
}
