using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CSEmu
{
    internal static class XmlManagement
    {
        public static T XmlLoader<T>(string file)
        {
            var s = new XmlSerializer(typeof(T));
            using (var ts = File.OpenText(file))
                return (T)s.Deserialize(ts);
        }
        public static void XmlSaver<T>(string file, T xml)
        {
            var s = new XmlSerializer(typeof(T));
            using (var ts = File.OpenWrite(file))
                s.Serialize(ts, xml);
        }
    }
}
