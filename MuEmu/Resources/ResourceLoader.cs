using MuEmu.Data;
using MuEmu.Resources.Map;
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

                yield return tmp;
            }
        }

        public IEnumerable<SpellInfo> LoadSkills()
        {
            var xml = XmlLoader<SpellDbDto>(Path.Combine(_root, "Data\\Skills.xml"));

            foreach (var i in xml.skills)
            {
                var Dmg = i.Dmg.Split("-").Select(x => int.Parse(x)).ToArray();
                var tmp = new SpellInfo
                {
                    Number = (Spell)i.Number,
                    Name = i.Name,
                    Mana = i.Mana,
                    Energy = i.Energy,
                    Damage = new Point(Dmg[0], Dmg[1]),
                    Classes = i.Classes.Split(",").Select(x => (HeroClass)Enum.Parse(typeof(HeroClass), x)).ToList()
                };

                yield return tmp;
            }
        }

        public IEnumerable<MapInfo> LoadMaps()
        {
            var xml = XmlLoader<MapsDbDto>(Path.Combine(_root, "Data\\Maps.xml"));
            foreach(var m in xml.maps)
            {
                yield return new MapInfo(m.Map, m.AttributteFile);
            }
        }

        private T XmlLoader<T>(string file)
        {
            var s = new XmlSerializer(typeof(T));
            using (var ts = File.OpenText(file))
                return (T)s.Deserialize(ts);
        }
    }
}
