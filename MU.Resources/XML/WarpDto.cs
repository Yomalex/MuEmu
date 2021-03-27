using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Warp")]
    public class WarpDto
    {
        [XmlElement("Gate")]
        public GateDto[] Gates { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class GateDto
    {
        [XmlAttribute]
        public int Number { get; set; }

        [XmlAttribute]
        public string GateType { get; set; }

        [XmlAttribute]
        public string Map { get; set; }

        [XmlAttribute]
        public byte X1 { get; set; }

        [XmlAttribute]
        public byte Y1 { get; set; }

        [XmlAttribute]
        public byte X2 { get; set; }

        [XmlAttribute]
        public byte Y2 { get; set; }

        [XmlAttribute]
        public int Target { get; set; }

        [XmlAttribute]
        public int Move { get; set; } = -1;

        [XmlAttribute]
        public string Name { get; set; } = "";

        [XmlAttribute]
        public byte Dir { get; set; }

        [XmlAttribute]
        public ushort ReqLevel { get; set; }

        [XmlAttribute]
        public ushort ReqZen { get; set; } = 0;
    }
}
