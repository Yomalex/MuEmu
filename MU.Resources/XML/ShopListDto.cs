using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("Shops")]
    public class ShopListDto
    {
        [XmlElement("Shop")] public ShopDto[] Shops { get; set; }
    }

    public class ShopDto
    {
        [XmlAttribute] public ushort Shop { get; set; }
        [XmlAttribute] public string ItemList { get; set; }
    }
}
