using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MU.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Patterns")]
    public class PatternsDto
    {
        [XmlAttribute] public int Monster { get; set; }
        [XmlElement] public PatternDto[] Pattern { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class PatternDto
    {
        [XmlAttribute] public int Number { get; set; }
        [XmlElement] public MonsterSpell[] Skill { get; set; }
    }
}
