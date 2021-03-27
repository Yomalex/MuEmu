using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("PCPointShop")]
    public class PCPointShopDto
    {
        [XmlElement("Item")] public ItemXmlDto[] Items { get; set; }
    }
}
