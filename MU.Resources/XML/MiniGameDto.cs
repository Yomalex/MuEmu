using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("MiniGame")]
    public class MiniGameDto
    {
        [XmlElement] public bool Active { get; set; }
        [XmlElement] public int NoneTime { get; set; }
        [XmlElement] public int OpenTime { get; set; }
        [XmlElement] public int ClosedTime { get; set; }
        [XmlElement] public ushort ItemDrop { get; set; }
        [XmlElement] public float DropRate { get; set; }
    }
}
