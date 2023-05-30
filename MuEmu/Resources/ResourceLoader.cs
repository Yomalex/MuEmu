using Google.Protobuf.WellKnownTypes;
using MU.Resources;
using MU.Resources.Game;
using MU.Resources.XML;
using MuEmu.Data;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using MuEmu.Resources.XML;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private IEnumerable<ItemInfo> LoadItemsXML()
        {
            var result = new List<ItemInfo>();
            var dir = Path.Combine(_root, "Items.xml");            
            var xml = XmlLoader<ItemDbDto>(dir);

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
                    Def = i.Defense,
                    DefRate = i.DefenseRate,
                    Attributes = i.Attributes.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => System.Enum.Parse<AttributeType>(x)).ToList(),
                    Zen = i.Zen,
                    Ruud = i.Ruud,
                    Classes = i.ReqClass.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => System.Enum.Parse<HeroClass>(x)).ToList(),
                    Skill = System.Enum.Parse<Spell>(i.Skill),
                    Durability = i.Durability,
                    MagicDur = i.MagicDur,
                    MagicPower = i.MagicPower,
                    Name = i.Name,
                    ReqLevel = i.NeededLevel,
                    MaxStack = i.MaxStack,
                    OnMaxStack = i.OnMaxStack,
                    IsMount = bool.Parse(i.IsMount),
                    Skin = i.Skin,
                    Inventory = System.Enum.Parse<StorageID>(i.Inventory),
                };

                result.Add(tmp);
            }

            return result;
        }
        public IEnumerable<ItemInfo> LoadItems()
        {
            try
            {
                return LoadItemsXML();
            }
            catch (FileNotFoundException)
            {
                var result = new List<ItemInfo>();
                if(File.Exists(Path.Combine(_root, "ItemS16.txt")))
                {
                    var loaderS16 = new LoadWZSectionTXT<ItemBMDS16GroupBasic>();
                    var info = loaderS16.Load(File.ReadAllText(Path.Combine(_root, "ItemS16.txt")));
                    result = info.Select(x => Extensions.AnonymousMap(new ItemInfo(), x)).ToList();
                }
                else
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
                        var type = (ItemType)byte.Parse(m.Groups[1].Value);
                        switch(type)
                        {
                            case ItemType.Sword:
                            case ItemType.Axe:
                            case ItemType.Scepter:
                            case ItemType.Spear:
                            case ItemType.BowOrCrossbow:
                            case ItemType.Staff:
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
                                        Skill = (Spell)ushort.Parse(sm.Groups[3].Value),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Name = sm.Groups[9].Value,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Damage = new Point(ushort.Parse(sm.Groups[11].Value), ushort.Parse(sm.Groups[12].Value)),
                                        Speed = ushort.Parse(sm.Groups[13].Value),
                                        Durability = byte.Parse(sm.Groups[14].Value),
                                        MagicDur = byte.Parse(sm.Groups[15].Value),
                                        MagicPower = byte.Parse(sm.Groups[16].Value),
                                        ReqLevel = ushort.Parse(sm.Groups[17].Value),
                                        Str = ushort.Parse(sm.Groups[18].Value),
                                        Agi = ushort.Parse(sm.Groups[19].Value),
                                        Ene = ushort.Parse(sm.Groups[20].Value),
                                        Vit = ushort.Parse(sm.Groups[21].Value),
                                        Cmd = ushort.Parse(sm.Groups[22].Value),
                                        Classes = c
                                    });
                                }
                                break;
                            case ItemType.Shield:
                            case ItemType.Helm:
                            case ItemType.Armor:
                            case ItemType.Pant:
                            case ItemType.Gloves:
                            case ItemType.Boots:
                                foreach (Match sm in Item6Regex.Matches(m.Groups[2].Value))
                                {
                                    var c = new List<HeroClass>();
                                    if (byte.Parse(sm.Groups[21].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[21].Value) - 1));
                                    if (byte.Parse(sm.Groups[22].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[22].Value) - 1 + 0x10));
                                    if (byte.Parse(sm.Groups[23].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[23].Value) - 1 + 0x20));
                                    if (byte.Parse(sm.Groups[24].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[24].Value) - 1 + 0x30));
                                    if (byte.Parse(sm.Groups[25].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[25].Value) - 1 + 0x40));
                                    if (byte.Parse(sm.Groups[26].Value) > 0)
                                        c.Add((HeroClass)(byte.Parse(sm.Groups[26].Value) - 1 + 0x50));

                                    var item = new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Name = sm.Groups[9].Value,
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
                                    };
                                    result.Add(item);
                                }
                                    break;
                            case ItemType.Wing_Orb_Seed:
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
                                        Name = sm.Groups[9].Value,
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
                            case ItemType.Missellaneo:
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
                                        Name = sm.Groups[9].Value,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        Durability = byte.Parse(sm.Groups[11].Value),
                                        Attributes = a,
                                        Classes = c
                                    });
                                }
                                break;
                            case ItemType.Potion:
                                foreach (Match sm in Item14Regex.Matches(m.Groups[2].Value))
                                {
                                    result.Add(new ItemInfo
                                    {
                                        Number = new ItemNumber(type, ushort.Parse(sm.Groups[1].Value)),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Name = sm.Groups[9].Value,
                                        Zen = int.Parse(sm.Groups[10].Value),
                                        Level = ushort.Parse(sm.Groups[11].Value),
                                        Classes = new List<HeroClass>(),
                                    });
                                }
                                break;
                            case ItemType.Scroll:
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

                                    Spell spell = Spell.None;
                                    var index = ushort.Parse(sm.Groups[1].Value);

                                    if (index <= 15)
                                    {
                                        spell = (Spell)(index + 1);
                                    }else if(index <= 18)
                                    {
                                        spell = (Spell)(index + 22);
                                    }
                                    else if (index <= 24)
                                    {
                                        spell = (Spell)(index + 195);
                                    }

                                    var itin = new ItemInfo
                                    {
                                        Number = new ItemNumber(type, index),
                                        Size = new Size(byte.Parse(sm.Groups[4].Value), byte.Parse(sm.Groups[5].Value)),
                                        Option = byte.Parse(sm.Groups[7].Value) != 0,
                                        Drop = byte.Parse(sm.Groups[8].Value) != 0,
                                        Name = sm.Groups[9].Value,
                                        Level = ushort.Parse(sm.Groups[10].Value),
                                        ReqLevel = ushort.Parse(sm.Groups[11].Value),
                                        Ene = ushort.Parse(sm.Groups[12].Value),
                                        Zen = int.Parse(sm.Groups[13].Value),
                                        Classes = c,
                                        Skill = spell
                                    };

                                    result.Add(itin);
                                }
                                break;
                        }
                    }
                }

                var xml = new ItemDbDto();
                xml.items = result.Select(x => new ItemDto
                {
                    Durability = x.Durability,
                    Dmg = $"{x.Damage.X}-{x.Damage.Y}",
                    Drop = x.Drop.ToString(),
                    Level = x.Level,
                    Defense = x.Def,
                    DefenseRate = x.DefRate,
                    MagicDur = x.MagicDur,
                    MagicPower = x.MagicPower,
                    Name = x.Name,
                    NeededAgi = x.Agi,
                    NeededCmd = x.Cmd,
                    NeededEne = x.Ene,
                    NeededLevel = x.ReqLevel,
                    NeededStr = x.Str,
                    NeededVit = x.Vit,
                    Number = x.Number,
                    Option = x.Option.ToString(),
                    ReqClass = string.Join(",", x.Classes),
                    Attributes = string.Join(",", x?.Attributes??new List<AttributeType>()),
                    Size = $"{x.Size.Width},{x.Size.Height}",
                    Skill = x.Skill.ToString(),
                    Speed = x.Speed,
                    Zen = x.Zen,
                    Ruud = x.Ruud,
                }).ToArray();

                XmlSaver(Path.Combine(_root, "Items.xml"), xml);
                return result;
            }           
        }

        public IEnumerable<SpellInfo> LoadSkills()
        {
            var result = new List<SpellInfo>();
            SpellDbDto xml;
            try
            {
                xml = XmlLoader<SpellDbDto>(Path.Combine(_root, "Skills.xml"));
            }catch(FileNotFoundException)
            {
                using (var tr = File.OpenText(Path.Combine(_root, "Skill.txt")))
                {
                    var SkillRegex = new Regex(@"([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(-?[0-9]+)\s+(-?[0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    foreach(Match m in SkillRegex.Matches(tr.ReadToEnd()))
                    {
                        var c = new List<HeroClass>();
                        if (byte.Parse(m.Groups[19].Value) > 0) //DW
                            c.Add((HeroClass)(byte.Parse(m.Groups[19].Value) - 1));
                        if (byte.Parse(m.Groups[20].Value) > 0) //DK
                            c.Add((HeroClass)(byte.Parse(m.Groups[20].Value) - 1 + 0x10));
                        if (byte.Parse(m.Groups[21].Value) > 0) //ELF
                            c.Add((HeroClass)(byte.Parse(m.Groups[21].Value) - 1 + 0x20));
                        if (byte.Parse(m.Groups[22].Value) > 0) //MG
                            c.Add((HeroClass)(byte.Parse(m.Groups[22].Value) - 1 + 0x30));
                        if (byte.Parse(m.Groups[23].Value) > 0) //DL
                            c.Add((HeroClass)(byte.Parse(m.Groups[23].Value) - 1 + 0x40));
                        if (byte.Parse(m.Groups[24].Value) > 0) //SUM
                            c.Add((HeroClass)(byte.Parse(m.Groups[24].Value) - 1 + 0x50));
                        if (byte.Parse(m.Groups[25].Value) > 0) //MONK
                            c.Add((HeroClass)(byte.Parse(m.Groups[25].Value) - 1 + 0x60));

                        var Dmg = ushort.Parse(m.Groups[4].Value);

                        var status = new List<int>
                        {
                            int.Parse(m.Groups[16].Value),
                            int.Parse(m.Groups[17].Value),
                            int.Parse(m.Groups[18].Value),
                        };

                        try
                        {
                            var tmp = new SpellInfo
                            {
                                Number = System.Enum.Parse<Spell>(m.Groups[1].Value, true),
                                Name = m.Groups[2].Value,
                                ReqLevel = ushort.Parse(m.Groups[3].Value),
                                Damage = new Point(Dmg, Dmg * 2),


                                Mana = ushort.Parse(m.Groups[5].Value),
                                BP = ushort.Parse(m.Groups[6].Value),
                                Distance = byte.Parse(m.Groups[7].Value),
                                Delay = uint.Parse(m.Groups[8].Value),
                                Energy = ushort.Parse(m.Groups[9].Value),
                                Command = ushort.Parse(m.Groups[10].Value),
                                Attribute = sbyte.Parse(m.Groups[11].Value),
                                Type = short.Parse(m.Groups[12].Value),
                                UseType = byte.Parse(m.Groups[13].Value),
                                Brand = int.Parse(m.Groups[14].Value),
                                KillCount = int.Parse(m.Groups[15].Value),
                                Status = status,
                                Classes = c,
                                Rank = int.Parse(m.Groups[26].Value),
                                Group = int.Parse(m.Groups[27].Value),
                                MasterP = int.Parse(m.Groups[28].Value),
                                AG = int.Parse(m.Groups[29].Value),
                                SD = int.Parse(m.Groups[30].Value),
                                Duration = int.Parse(m.Groups[31].Value),
                                Str = ushort.Parse(m.Groups[32].Value),
                                Agility = ushort.Parse(m.Groups[33].Value),
                                Icon = ushort.Parse(m.Groups[34].Value),
                                UseType2 = byte.Parse(m.Groups[35].Value),
                                Item = ushort.Parse(m.Groups[36].Value),
                                IsDamage = byte.Parse(m.Groups[37].Value),
                            };
                            result.Add(tmp);
                        } catch (Exception ex)
                        {
                            Logger.Error("", ex);
                        }
                    }
                    xml = new SpellDbDto();
                    xml.skills = result.Select(x => new SkillDto
                    {
                        AG = x.AG,
                        Agility = x.Agility,
                        Attribute = x.Attribute,
                        BP = x.BP,
                        Brand = x.Brand,
                        Command = x.Command,
                        Delay = x.Delay,
                        Distance = x.Distance,
                        Duration = x.Duration,
                        Group = x.Group,
                        Icon = x.Icon,
                        IsDamage = x.IsDamage,
                        Item = x.Item,
                        KillCount = x.KillCount,
                        MasterP = x.MasterP,
                        Rank = x.Rank,
                        ReqLevel = x.ReqLevel,
                        SD = x.SD,
                        Status = string.Join(",", x.Status),
                        Str = x.Str,
                        Type = x.Type,
                        UseType = x.UseType,
                        UseType2 = x.UseType2,
                        Classes = string.Join(",", x.Classes),
                        Dmg = $"{x.Damage.X}-{x.Damage.Y}",
                        Energy = x.Energy,
                        Mana = x.Mana,
                        Name = x.Name,
                        Number = (ushort)x.Number,
                    }).ToArray();

                    XmlSaver(Path.Combine(_root, "Skills.xml"), xml);
                    return result;
                }
            }

            foreach (var i in xml.skills)
            {
                //Logger.Debug("Spell {0} {1}", (Spell)i.Number, i.Number);
                var Dmg = i.Dmg.Split("-").Select(x => int.Parse(x)).ToArray();


                var classList = i.Classes
                    .Split(",")
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => (HeroClass)System.Enum.Parse(typeof(HeroClass), x))
                    .ToList();

                var tmp = new SpellInfo
                {
                    Number = (Spell)i.Number,
                    Name = i.Name,
                    AG = i.AG,
                    Agility = i.Agility,
                    Attribute = i.Attribute,
                    BP = i.BP,
                    Brand = i.Brand,
                    Command = i.Command,
                    Delay = i.Delay,
                    Distance = i.Distance,
                    Duration = i.Duration,
                    Group = i.Group,
                    Icon = i.Icon,
                    IsDamage = i.IsDamage,
                    Item = i.Item,
                    KillCount = i.KillCount,
                    MasterP = i.MasterP,
                    Rank = i.Rank,
                    ReqLevel = i.ReqLevel,
                    SD = i.SD,
                    Status = i.Status.Split(",").Select(x => int.Parse(x)).ToList(),
                    Str = i.Str,
                    Type = i.Type,
                    UseType = i.UseType,
                    UseType2 = i.UseType2,
                    Mana = i.Mana,
                    Energy = i.Energy,
                    Damage = new Point(Dmg[0], Dmg[1]),
                    Classes = classList
                };

                result.Add(tmp);
            }

            return result;
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
                if(@char.Equipament != null)
                foreach(var e in @char.Equipament)
                {
                    eq.Add((ushort)e.Slot, new Item(new ItemNumber((byte)e.Type, (ushort)e.Index), new { Plus = (byte)e.Level }));
                }

                yield return new CharacterInfo
                {
                    Level = (ushort)@char.Level,
                    Class = (HeroClass)System.Enum.Parse(typeof(HeroClass), @char.BaseClass),
                    Map = (Maps)System.Enum.Parse(typeof(Maps), @char.Map),
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

                var loader = new LoadWZTXT<ShopInfoDto>();
                var file = loader.Load(Path.Combine(_root, shop.ItemList));
                var itemList = file.Item.Select(
                    x => new Item(
                        new ItemNumber(x.Type, x.Index),
                        Options:
                        new {
                            x.Plus,
                            x.Durability,
                            x.Skill,
                            x.Luck,
                            Option28 = x.Option,
                            OptionExe = x.Excellent })).ToList();

                try
                {
                    itemList.ForEach(x => result.Storage.Add(x));
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }

                yield return result;
            }
        }

        public IEnumerable<NPCInfo> LoadNPCs()
        {
            var xml = XmlLoader<NPCAttributesDto>(Path.Combine(_root, "NPCs.xml"));
            foreach(var npc in xml.NPCs)
            {
                var type = (NPCAttributeType)System.Enum.Parse(typeof(NPCAttributeType), npc.Type);
                NPCInfo info = new NPCInfo
                {
                    NPC = npc.NPC,
                    Data = 0xffff,
                    Class = type,
                    Icon = npc.Icon,
                };
                switch (type)
                {
                    case NPCAttributeType.ShopRuud:
                    case NPCAttributeType.Shop:
                        var shops = ResourceCache.Instance.GetShops();
                        var shopNum = ushort.Parse(npc.Data);

                        if (shops.ContainsKey(shopNum))
                            info.Data = shopNum;
                        break;
                    case NPCAttributeType.Quest:
                    case NPCAttributeType.Warehouse:
                    case NPCAttributeType.MessengerAngel:
                    case NPCAttributeType.KingAngel:
                    case NPCAttributeType.EventChips:
                    case NPCAttributeType.GuildMaster:
                        break;
                    case NPCAttributeType.Window:
                        info.Data = byte.Parse(npc.Data);
                        break;
                    case NPCAttributeType.Buff:
                        info.Data = ushort.Parse(npc.Data);
                        break;
                }
                yield return info;
            }
        }

        public JOHDto LoadJOH()
        {
            var loader = new LoadWZTXT<JOHDto>();
            var file = loader.Load(Path.Combine(_root, "JewelOfHarmonyOption.txt"));
            return file;
        }

        public IEnumerable<Gate> LoadGates()
        {
            WarpDto xml;
            try
            {
                xml = XmlLoader<WarpDto>(Path.Combine(_root, "Warps.xml"));
            }catch(FileNotFoundException)
            {
                xml = new WarpDto();
                var gates = new List<GateDto>();
                using (var tr = File.OpenText(Path.Combine(_root, "MoveGate.txt")))
                {
                    var GateRegex = new Regex(@"([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    foreach(Match m in GateRegex.Matches(tr.ReadToEnd()))
                    {
                        gates.Add(new GateDto
                        {
                            Number = int.Parse(m.Groups[1].Value),
                            GateType = m.Groups[2].Value,
                            Map = ((Maps)byte.Parse(m.Groups[3].Value)).ToString(),
                            X1 = byte.Parse(m.Groups[4].Value),
                            Y1 = byte.Parse(m.Groups[5].Value),
                            X2 = byte.Parse(m.Groups[6].Value),
                            Y2 = byte.Parse(m.Groups[7].Value),
                            Target = int.Parse(m.Groups[8].Value),
                            Dir = byte.Parse(m.Groups[9].Value),
                            ReqLevel = ushort.Parse(m.Groups[10].Value),
                        });
                    }
                }

                using (var tr = File.OpenText(Path.Combine(_root, "MoveReq.txt")))
                {
                    var MoveRegex = new Regex(@"([0-9]+)\s+" + "\"" + @"(.+)" + "\"" + @"\s+" + "\"" + @"(.+)" + "\"" + @"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)");
                    foreach (Match m in MoveRegex.Matches(tr.ReadToEnd()))
                    {
                        var igate = int.Parse(m.Groups[6].Value);
                        var gate = gates.Where(x => x.Number == igate).FirstOrDefault();
                        if (gate == null)
                            continue;
                        gate.ReqZen = ushort.Parse(m.Groups[4].Value);
                        gate.Move = int.Parse(m.Groups[1].Value);
                        gate.Name = m.Groups[2].Value;
                    }
                }

                xml.Gates = gates.ToArray();

                XmlSaver(Path.Combine(_root, "Warps.xml"), xml);
            }
            foreach (var g in xml.Gates)
            {
                yield return new Gate
                {
                    GateType = System.Enum.Parse<GateType>(g.GateType),
                    Map = System.Enum.Parse<Maps>(g.Map),
                    Number = g.Number,
                    ReqLevel = g.ReqLevel,
                    Target = g.Target,
                    Move = g.Move,
                    Name = g.Name,
                    Dir = g.Dir,
                    Door = new Rectangle(g.X1, g.Y1, g.X2 - g.X1, g.Y2 - g.Y1),
                    ReqZen = g.ReqZen,
                };
            }
        }

        public IEnumerable<QuestInfo> LoadQuests()
        {
            var xml = XmlLoader<QuestsDto>(Path.Combine(_root, "Quests.xml"));
            foreach (var q in xml.Quest)
            {
                var tmp = new QuestInfo
                {
                    Name = q.Name,
                    Type = q.Type,
                    Index = q.Index,
                    NPC = q.NPC,
                    Sub = new List<SubQuest>(),
                    Conditions = new List<RunConditions>(),
                };

                foreach (var sq in q.Details)
                {
                    var stmp = new SubQuest
                    {
                        Index = sq.Index,
                        Allowed = sq.Classes.Split(",").Select(x => System.Enum.Parse<HeroClass>(x)).ToArray(),
                        Messages = new Dictionary<QuestState, ushort>(),
                        CompensationType = System.Enum.Parse<QuestCompensation>(sq.Reward.Type),
                        Amount = sq.Reward.SubType,
                        Requeriment = new List<Item>()
                    };
                    if (sq.NeededItem != null)
                    {
                        foreach (var it in sq.NeededItem)
                        {
                            stmp.Requeriment.Add(new Item(ItemNumber.FromTypeIndex((byte)it.Type, (ushort)it.Index), new { Plus = (byte)it.Level, Durability = (byte)it.Count }));
                            var mon = it.Monster.Split("-").Where(x => !string.IsNullOrEmpty(x)).Select(x => ushort.Parse(x));
                            stmp.MonsterMin = mon.FirstOrDefault();
                            stmp.MonsterMax = mon.LastOrDefault();
                            stmp.Count = it.Count;
                            stmp.Drop = (ushort)it.Drop;
                        }
                    }

                    if (sq.NeededMonster != null)
                    {
                        stmp.Monster = sq.NeededMonster.Type;
                        stmp.Count = sq.NeededMonster.Count;
                    }

                    stmp.Messages.Add(QuestState.Unreg, sq.Message.BeforeReg);
                    stmp.Messages.Add(QuestState.Reg, sq.Message.AfterReg);
                    stmp.Messages.Add(QuestState.Complete, sq.Message.CompleteQuest);
                    stmp.Messages.Add(QuestState.Clear, sq.Message.ClearQuest);

                    tmp.Sub.Add(stmp);
                }

                foreach (var c in q.Conditions)
                {
                    var ctmp = new RunConditions
                    {
                        Index = c.Index,
                        NeededQuestIndex = c.NeededQuest,
                        Cost = c.Cost,
                        MaxLevel = c.MaxLevel,
                        Message = c.Message,
                        MinLevel = c.MinLevel,
                        NeedStr = c.NeedStr,
                    };

                    tmp.Conditions.Add(ctmp);
                }

                Logger.Information("Quest: {0} OK! Linked to NPC:{1}", tmp.Name, tmp.NPC);
                yield return tmp;
            }
        }

        public ChaosMixInfo LoadChaosBox()
        {
            var cbmix = new ChaosMixInfo
            {
                AdditionalJewels = new List<JewelInfo>(),
                Mixes = new List<MixInfo>()
            };
            var xml = XmlLoader<ChaosMixDto>(Path.Combine(_root, "ChaosBox.xml"));
            foreach(var j in xml.Jewels)
            {
                cbmix.AdditionalJewels.Add(new JewelInfo
                {
                    ItemNumber = ItemNumber.FromTypeIndex((byte)j.Type, (ushort)j.Index),
                    Success = j.Success
                });
            }
            foreach(var m in xml.Mixes)
            {
                var mix = new MixInfo
                {
                    Name = m.Name,
                    BaseCost = m.Value,
                    GeneralSuccess = m.Success,
                    NPC = m.NPC,
                    Ingredients = new List<IngredientInfo>(),
                    ResultSuccess = new List<IngredientInfo>(),
                };
                mix.Ingredients.AddRange(
                    m.Ingredients.Select(x => new IngredientInfo
                    {
                        IID = x.IID,
                        Count = x.Count,
                        ItemNumber = ItemNumber.FromTypeIndex((byte)x.Type, (ushort)x.Index),
                        Level = x.Level.Split("-").Select(y => int.Parse(y)).ToArray(),
                        Luck = x.Luck,
                        Option = x.Option,
                        Excellent = x.Excellent,
                        Harmony = x.Harmony,
                        SetOption = x.SetOption,
                        Socket = x.Socket,
                        Skill = x.Skill,
                        Success = x.Success,
                    }
                    ));
                mix.ResultSuccess.AddRange(
                    m.RewardSuccess.Select(x => new IngredientInfo
                    {
                        IID = x.IID,
                        Count = x.Count,
                        ItemNumber = ItemNumber.FromTypeIndex((byte)x.Type, (ushort)x.Index),
                        Level = x.Level.Split("-").Select(y => int.Parse(y)).ToArray(),
                        Luck = x.Luck,
                        Option = x.Option,
                        Excellent = x.Excellent,
                        Harmony = x.Harmony,
                        SetOption = x.SetOption,
                        Socket = x.Socket,
                        Skill = x.Skill,
                        Success = x.Success,
                    }
                    ));
                mix.ResultFail = new IngredientInfo
                    {
                        IID = m.RewardFail.IID,
                        Count = m.RewardFail.Count,
                        ItemNumber = ItemNumber.FromTypeIndex((byte)m.RewardFail.Type, (ushort)m.RewardFail.Index),
                        Level = m.RewardFail.Level.Split("-").Select(y => int.Parse(y)).ToArray(),
                        Luck = m.RewardFail.Luck,
                        Option = m.RewardFail.Option,
                        Excellent = m.RewardFail.Excellent,
                        Harmony = m.RewardFail.Harmony,
                        SetOption = m.RewardFail.SetOption,
                        Socket = m.RewardFail.Socket,
                        Skill = m.RewardFail.Skill,
                        Success = m.RewardFail.Success,
                    };
                cbmix.Mixes.Add(mix);
            }

            return cbmix;
        }

        public IEnumerable<Bag> LoadItembags()
        {
            var xml = XmlLoader<ItemBagsDto>(Path.Combine(_root, "ItemBags/ItemBags.xml"));
            var helper = new LoadWZTXT<DSItemBagDto>();
            var helper2 = new LoadWZTXT<WZItemBagDto>();
            DSItemBagDto baseb = null;
            WZItemBagDto basec = null;
            foreach (var x in xml.ItemBags)
            {
                if(Path.GetExtension(x.Bag) == ".txt")
                {
                    var oldFile = Path.Combine(_root, "ItemBags/" + x.Bag);
                    var newFile = Path.ChangeExtension(oldFile, ".xml");
                    if(File.Exists(newFile))
                    {
                        var basex = XmlLoader<BagDto>(newFile);
                        yield return new Bag(basex);
                        continue;
                    }
                    try
                    {
                        baseb = helper.Load(oldFile);
                    }
                    catch (Exception) { }
                    try
                    {
                        if(baseb.Section1 == null)
                            basec = helper2.Load(oldFile);
                    }
                    catch (Exception) { }
                    BagDto basea = null;
                    if (baseb.Section1 != null)
                    {
                        Logger.Information($"Converting {oldFile} from www.darksideofmu.com file type to XML File Type");
                        try
                        {
                            basea = new BagDto
                            {
                                DropItemCount = 1,
                                DropItemRate = (ushort)Math.Min(baseb.Section1[0].ItemDropRate, (short)100),
                                DropZenRate = (ushort)(100 - baseb.Section1[0].ItemDropRate),
                                LevelMin = 1,
                                MaxZen = baseb.Section1[0].DropZen,
                                MinZen = baseb.Section1[0].DropZen,
                                Number = ItemNumber.FromTypeIndex(baseb.Section1[0].BoxType, baseb.Section1[0].BoxIndex),
                                Monster = x.Monster,
                                Plus = baseb.Section1[0].BoxLevel,
                                Item = baseb.Section2.Select(y => new ItemInBagDto
                                {
                                    Luck = y.Luck == 1,
                                    MaxExcellent = y.maxExOpt,
                                    MaxLevel = y.maxLvl,
                                    MaxOption = y.maxZ28,
                                    MinExcellent = 0,
                                    MinLevel = 0,
                                    MinOption = 0,
                                    Number = ItemNumber.FromTypeIndex(y.Type, y.Index),
                                    Skill = y.Skill == 1,
                                }).ToArray(),
                            };
                        } catch (Exception ex)
                        {
                            Logger.Error("DSFile to XML Error:", ex);
                            continue;
                        }
                    }else
                    {
                        Logger.Information($"Converting {oldFile} from GS 1.00.18 file type to XML File Type");
                        //basec.BagDtos
                        basea = new BagDto
                        {
                            DropItemCount = 1,
                            DropItemRate = 50,
                            DropZenRate = 50,
                            LevelMin = x.LevelMin,
                            MaxZen = 100000,
                            MinZen = 10000,
                            Number = x.Item,
                            Monster = x.Monster,
                            Plus = x.Plus,
                            Item = basec.BagDtos.Select(y => new ItemInBagDto
                            {
                                Luck = y.Luck == 1,
                                MaxExcellent = 0,
                                MaxLevel = y.Lvl,
                                MaxOption = y.Option,
                                MinExcellent = 0,
                                MinLevel = 0,
                                MinOption = 0,
                                Number = ItemNumber.FromTypeIndex(y.Type, y.Index),
                                Skill = y.Skill == 1,
                            }).ToArray(),
                        };
                    }
                    XmlSaver(newFile, basea);
                    yield return new Bag(basea);
                }
                else
                {
                    var basea = XmlLoader<BagDto>(Path.Combine(_root, "ItemBags/" + x.Bag));
                    yield return new Bag(basea);
                }
            }
        }

        public Storage LoadPCPointShop()
        {
            var xml = XmlLoader<PCPointShopDto>(Path.Combine(_root, "PCPointShop.xml"));
            var sto = new Storage(Storage.ShopSize);

            foreach(var x in xml.Items)
            {
                var it = new Item((ItemNumber)x.Number, Options: new { x.Plus, x.Luck, x.Skill, x.Option28, x.Durability, x.OptionExe, x.BuyPrice });
                sto.Add(it);
            }

            return sto;
        }
    }
}
