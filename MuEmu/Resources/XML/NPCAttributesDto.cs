using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("NPCs")]
    public class NPCAttributesDto
    {
        [XmlElement("NPC")]
        public NPCAttributeDto[] NPCs { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class NPCAttributeDto
    {
        [XmlAttribute] public ushort NPC { get; set; }
        [XmlAttribute] public string Type { get; set; }
        [XmlAttribute] public string Data { get; set; }
        [XmlAttribute] public MiniMapTag Icon { get; set; } = MiniMapTag.Shield;
    }
}
