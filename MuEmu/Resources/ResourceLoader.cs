using MuEmu.Data;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using MuEmu.Resources.XML;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MuEmu.Resources
{
    public class ResourceLoader
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceLoader));
        private string _root;
        public ResourceLoader(string root)
        {
            _root = root;
        }

        public IEnumerable<ItemInfo> LoadItems()
        {
            var result = new List<ItemInfo>();
            try
            {
                var xml = XmlLoader<ItemDbDto>(Path.Combine(_root, "Items2.xml"));

                foreach (var i in xml.items)
                {
                    var Size = i.Size.Split(",").Select(x => int.Parse(x)).ToArray();
                    var Dmg = i.Dmg.Split("-").Select(x => int.Parse(x)).ToArray();
                    var tmp = new ItemInfo
                    {
                        Number = i.Number,
                        Size = new Size(Size[0], Size[1]),
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
            }
            catch (Exception)
            { 
                using (var tr = File.OpenText(Path.Combine(_root, "Item.txt")))
                {
                    var ItemRegex = new Regex(@"([0-9]+)\s*\n+(?s)(.*?)\nend");
                    var Item0Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    var Item6Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    var Item12Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    var Item13Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    var Item14Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)");
                    var Item15Regex = new Regex(@"([0-9]+)\s+([\-0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    foreach (Match m in ItemRegex.Matches(tr.ReadToEnd()))
                    {
                        var type = byte.Parse(m.Groups[1].Value);
                        switch(type)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                foreach (Match sm in Item0Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[24].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[24].Value) - 1));
                                    if (byte.Parse(sm.Groups[25].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[25].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[26].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[26].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[27].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[27].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[28].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[28].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[29].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[29].Value) - 1 + 0x50));

                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Damage = new Point(ushort.Parse(sm.Groups[11].Value), ushort.Parse(sm.Groups[12].Value)),
                                        Speed = ushort.Parse(sm.Groups[13].Value),
                                        Durability = byte.Parse(sm.Groups[14].Value),
                                        Str = ushort.Parse(sm.Groups[18].Value),
                                        Agi = ushort.Parse(sm.Groups[19].Value),
                                        Vit = ushort.Parse(sm.Groups[20].Value),
                                        Ene = ushort.Parse(sm.Groups[21].Value),
                                        Cmd = ushort.Parse(sm.Groups[22].Value),
                                        Classes = c
                                    });
                                }
                                break;
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                foreach (Match sm in Item6Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[20].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[20].Value) - 1));
                                    if (byte.Parse(sm.Groups[21].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[21].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[22].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[22].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[23].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[23].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[24].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[24].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[25].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[25].Value) - 1 + 0x50));

                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Def = ushort.Parse(sm.Groups[11].Value),
                                        DefRate = ushort.Parse(sm.Groups[12].Value),
                                        Durability = byte.Parse(sm.Groups[13].Value),
                                        ReqLevel = ushort.Parse(sm.Groups[14].Value),
                                        Str = ushort.Parse(sm.Groups[15].Value),
                                        Agi = ushort.Parse(sm.Groups[16].Value),
                                        Vit = ushort.Parse(sm.Groups[17].Value),
                                        Ene = ushort.Parse(sm.Groups[18].Value),
                                        Cmd = ushort.Parse(sm.Groups[19].Value),
                                        Classes = c
                                    });
                                }
                                    break;
                            case 12:
                                foreach (Match sm in Item12Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[19].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[19].Value) - 1));
                                    if (byte.Parse(sm.Groups[20].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[20].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[21].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[21].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[22].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[22].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[23].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[23].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[24].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[24].Value) - 1 + 0x50));

                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Def = ushort.Parse(sm.Groups[11].Value),
                                        Durability = byte.Parse(sm.Groups[12].Value),
                                        ReqLevel = ushort.Parse(sm.Groups[13].Value),
                                        Ene = ushort.Parse(sm.Groups[14].Value),
                                        Str = ushort.Parse(sm.Groups[15].Value),
                                        Agi = ushort.Parse(sm.Groups[16].Value),
                                        Cmd = ushort.Parse(sm.Groups[17].Value),
                                        Zen = int.Parse(sm.Groups[18].Value),
                                        Classes = c
                                    });
                                }
                                break;
                            case 13:
                                foreach (Match sm in Item13Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[20].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[20].Value) - 1 + 0x00));
                                    if (byte.Parse(sm.Groups[21].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[21].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[22].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[22].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[23].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[23].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[24].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[24].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[25].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[25].Value) - 1 + 0x50));

                                    var a = new List<AttributeType>();
                                    for(var i=0; i < 7; i++)
                                    {
                                        if (byte.Parse(sm.Groups[12 + i].Value) != 0)
                                            a.Add((AttributeType)i);
                                    }

                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Durability = byte.Parse(sm.Groups[11].Value),
                                        Attributes = a,
                                        Classes = c
                                    });
                                }
                                break;
                            case 14:
                                foreach (Match sm in Item14Regex.Matches(m.Groups[2].Value))
                                {
                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Zen = int.Parse(sm.Groups[10].Value),
                                        Level = ushort.Parse(sm.Groups[11].Value)
                                    });
                                }
                                break;
                            case 15:
                                foreach (Match sm in Item15Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[14].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[14].Value) - 1));
                                    if (byte.Parse(sm.Groups[15].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[15].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[16].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[16].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[17].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[17].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[18].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[18].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[19].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[19].Value) - 1 + 0x50));

                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        ReqLevel = ushort.Parse(sm.Groups[11].Value),
                                        Ene = ushort.Parse(sm.Groups[12].Value),
                                        Zen = int.Parse(sm.Groups[13].Value),
                                        Classes = c
                                    });
                                }
                                break;
                        }
                    }
                }
            }

            return result;            
        }

        public IEnumerable<SpellInfo> LoadSkills()
        {
            var xml = XmlLoader<SpellDbDto>(Path.Combine(_root, "Skills.xml"));

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
            var xml = XmlLoader<MapsDbDto>(Path.Combine(_root, "Maps.xml"));
            foreach(var m in xml.maps)
            {
                yield return new MapInfo(m.Map, m.AttributteFile);
            }
        }

        public IEnumerable<CharacterInfo> LoadDefCharacter()
        {
            var xml = XmlLoader<CharactersInfoDto>(Path.Combine(_root, "Characters.xml"));
            foreach(var @char in xml.Character)
            {
                var eq = new Dictionary<ushort, Item>();
                foreach(var e in @char.Equipament)
                {
                    eq.Add((ushort)e.Slot, new Item(new ItemNumber((byte)e.Type, (byte)e.Index), 0, new { e.Level }));
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

        public IEnumerable<ShopInfo> LoadShops()
        {
            var xml = XmlLoader<ShopListDto>(Path.Combine(_root, "ShopList.xml"));
            foreach(var shop in xml.Shops)
            {
                var result = new ShopInfo
                {
                    Shop = shop.Shop,
                    Storage = new Storage(Storage.ShopSize),
                };
                var count = 0;
                using (var tr = File.OpenText(Path.Combine(_root, shop.ItemList)))
                {
                    var contents = tr.ReadToEnd();
                    var ShopRegex = new Regex(@"(\/*[0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s*(.*)\n+");
                    foreach(Match m in ShopRegex.Matches(contents))
                    {
                        if (m.Groups[1].Value.StartsWith("//"))
                            continue;

                        var type = byte.Parse(m.Groups[1].Value);
                        var index = ushort.Parse(m.Groups[2].Value);
                        var Plus = byte.Parse(m.Groups[3].Value);
                        var Durability = byte.Parse(m.Groups[4].Value);
                        var Skill = byte.Parse(m.Groups[5].Value) !=0;
                        var Luck = byte.Parse(m.Groups[6].Value) != 0;
                        var Option28 = byte.Parse(m.Groups[7].Value);
                        var OptionExe = byte.Parse(m.Groups[7].Value);
                        try
                        {
                            result.Storage.Add(new Item(new ItemNumber(type, index), 0, new { Plus, Durability, Skill, Luck, Option28, OptionExe }));
                        }catch(Exception e)
                        {
                            Logger.Error(e.Message);
                        }
                        count++;
                    }
                }

                Logger.Information("Shop {0}, {1} Items loaded", shop.Shop, count);

                yield return result;
            }
        }

        public IEnumerable<NPCInfo> LoadNPCs()
        {
            var xml = XmlLoader<NPCAttributesDto>(Path.Combine(_root, "NPCs.xml"));
            foreach(var npc in xml.NPCs)
            {
                var type = (NPCAttributeType)Enum.Parse(typeof(NPCAttributeType), npc.Type);
                NPCInfo info = new NPCInfo
                {
                    NPC = npc.NPC,
                };
                switch (type)
                {
                    case NPCAttributeType.Quest:
                        info.Quest = ushort.Parse(npc.Data);
                        break;
                    case NPCAttributeType.Shop:
                        var shops = ResourceCache.Instance.GetShops();
                        var shopNum = ushort.Parse(npc.Data);
                        if (shops.ContainsKey(shopNum))
                        {
                            info.Shop = shops[shopNum];
                        }
                        else
                        {
                            Logger.Error("Shop {0} no exists", shopNum);
                        }
                        break;
                    case NPCAttributeType.Warehouse:
                        info.Warehouse = true;
                        break;
                }
                yield return info;
            }
        }

        public static T XmlLoader<T>(string file)
        {
            var s = new XmlSerializer(typeof(T));
            using (var ts = File.OpenText(file))
                return (T)s.Deserialize(ts);
        }
    }
}
