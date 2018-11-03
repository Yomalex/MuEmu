using MuEmu.Resources.XML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources
{
    public class ResourceLoader
    {
        private string _root;
        public ResourceLoader(string root)
        {
            _root = root;
        }

        public IEnumerable<ItemInfo> LoadItems()
        {
            var xml = XmlLoader<ItemDbDto>(Path.Combine(_root, "Data\\Items.xml"));

            var result = new List<ItemInfo>();

            foreach(var i in xml.items)
            {
                var Size = i.Size.Split(",").Select(x => int.Parse(x)).ToArray();
                var Dmg = i.Dmg.Split("-").Select(x => int.Parse(x)).ToArray();
                var tmp = new ItemInfo
                {
                    Number = i.Number,
                    Size = new Point(Size[0], Size[1]),
                    Option = bool.Parse(i.Option),
                    Drop = bool.Parse(i.Drop),
                    Damage = new Point(Dmg[0], Dmg[1]),
                    Speed = i.Speed,
                    Str = i.NeededStr,
                    Agi = i.NeededAgi,
                    Vit = i.NeededVit,
                    Ene = i.NeededEne,
                    Cmd = i.NeededCmd,
                    Level = i.Level,
                    Classes = i.ReqClass.Split(",").Select(x => (HeroClass)Enum.Parse(typeof(HeroClass), x)).ToList()
                };

                result.Add(tmp);
            }

            return result;
        }

        private T XmlLoader<T>(string file)
        {
            var s = new XmlSerializer(typeof(T));
            using (var ts = File.OpenText(file))
                return (T)s.Deserialize(ts);
        }
    }
}
