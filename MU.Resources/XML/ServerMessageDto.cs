using MuEmu.Resources.Game;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("ServerMessage")]
    public class ServerMessagesDto
    {

        [XmlElement("Message")]
        public ServerMessageDto[] Messages { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ServerMessageDto
    {
        [XmlAttribute] public string ID { get; set; }
        [XmlAttribute] public string Message { get; set; }
    }
}
