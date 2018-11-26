using MuEmu.Data;
using MuEmu.Resources.Game;
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

        public IEnumerable<CharacterInfo> LoadDefCharacter()
        {
            var xml = XmlLoader<CharactersInfoDto>(Path.Combine(_root, "Data\\Characters.xml"));
            foreach(var @char in xml.Character)
            {
                var eq = new Dictionary<ushort, Item>();
                foreach(var e in @char.Equipament)
                {
                    eq.Add((ushort)e.Slot, new Item((ushort)(e.Type * 256 + e.Index), 0, new { e.Level }));
                }

                yield return new CharacterInfo
                {
                    Level = (ushort)@char.Level,
                    Class = (HeroClass)Enum.Parse(typeof(HeroClass), @char.BaseClass),
                    Map = (Maps)Enum.Parse(typeof(Maps), @char.Map),
                    Spells = @char.Skill?.Select(x => (Spell)x).ToArray()??Array.Empty<Spell>(),
                    Stats = new StatsInfo
                    {
                        Str = @char.Stats.Str,
                        Agi = @char.Stats.Agi,
                        Vit = @char.Stats.Vit,
                        Ene = @char.Stats.Ene,
                        Cmd = @char.Stats.Cmd,
                    },
                    Attributes = new AttriInfo
                    {
                        Life = @char.Attributes.Life,
                        Mana = @char.Attributes.Mana,
                        LevelLife = @char.Attributes.LevelLife,
                        LevelMana = @char.Attributes.LevelMana,
                        EnergyToMana = @char.Attributes.EnergyToMana,
                        VitalityToLife = @char.Attributes.VitalityToLife,
                        StrToBP = @char.Attributes.StrToBP,
                        AgiToBP = @char.Attributes.AgiToBP,
                        EneToBP = @char.Attributes.EneToBP,
                        VitToBP = @char.Attributes.VitToBP,
                        CmdToBP = @char.Attributes.CmdToBP,
                    },
                    Equipament = eq
                };
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
