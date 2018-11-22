using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Maps")]
    public class MapsDbDto
    {
        [XmlElement("Map")]
        public MapDto[] maps { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MapDto
    {
        [XmlAttribute] public ushort Map { get; set; }
        [XmlAttribute] public string AttributteFile { get; set; }
    }
}
