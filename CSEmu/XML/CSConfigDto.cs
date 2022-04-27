using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSEmu.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("ConnectServer")]
    public class CSConfigDto
    {
        [XmlElement] public string apiKey { get; set; } = "api-20220426";

        [XmlElement] public string IP { get; set; } = "127.0.0.1";

        [XmlElement] public string IPChat { get; set; } = "127.0.0.1";
        
        [XmlElement] public CSDataBaseDto DataBase { get; set; } = new CSDataBaseDto();
    }
    [XmlType(AnonymousType = true)]
    public class CSDataBaseDto
    {
        [XmlElement] public string IP { get; set; } = "127.0.0.1";
        [XmlElement] public string Name { get; set; } = "MuOnline";
        [XmlElement] public string User { get; set; } = "root";
        [XmlElement] public string Password { get; set; } = "";
    }
}
