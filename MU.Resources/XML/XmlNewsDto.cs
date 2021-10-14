using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.XML
{
    [XmlRoot("News")]
    [XmlType(AnonymousType =true)]
    public class XmlNewsDto
    {
        [XmlAttribute] public int Interval { get; set; }
        [XmlElement] public string[] New { get; set; }
    }
}
