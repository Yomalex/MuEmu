using MU.Resources;
using MuEmu.Resources;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MuEmu.Monsters
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("MonsterIA")]
    public class XmlMonsterIAInfo
    {
        [XmlElement("Info")] public MonsterIAGroup[] Monsters { get; set; }
    }
    public class MonsterIAGroup
    {
        [XmlAttribute] public int GroupNumber { get; set; }
        [XmlAttribute] public int Guid { get; set; }
        [XmlAttribute] public ushort Class { get; set; }
        [XmlAttribute] public int Rank { get; set; }
        [XmlAttribute] public int StartAI { get; set; }
        [XmlAttribute] public int AI01 { get; set; }
        [XmlAttribute] public int AI02 { get; set; }
        [XmlAttribute] public int AI03 { get; set; }
        [XmlAttribute] public int CreateType { get; set; }
        [XmlAttribute] public Maps MapNumber { get; set; }
        [XmlAttribute] public int StartX { get; set; }
        [XmlAttribute] public int StartY { get; set; }
        [XmlAttribute] public sbyte StartDir { get; set; }
        [XmlAttribute] public int RegenType { get; set; }

        internal Monster monster;
    }
    public class MonsterIA
    {
        private static MonsterIA _instance;
        private Dictionary<int, List<MonsterIAGroup>> _IAGroups;

        private MonsterIA(string root)
        {
            LoadGroups(root + "MonsterAIGroup.txt");
        }

        private void LoadGroups(string file)
        {
            var loader = new LoadWZTXT<XmlMonsterIAInfo>();
            var xml = loader.Load(file);
            _IAGroups = xml.Monsters.GroupBy(x => x.GroupNumber)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var group in _IAGroups)
            {
                foreach (var mob in group.Value)
                {
                    Point point = new Point();
                    var mapInfo = ResourceCache.Instance.GetMaps()[mob.MapNumber];
                    switch (mob.CreateType)
                    {
                        case 0:
                            point = new Point(mob.StartX, mob.StartY);
                            break;
                        case 1:
                            for (var y = mob.StartY - 5; y <= (mob.StartY + 5) && point.X == 0; y++)
                                for (var x = mob.StartX - 5; x <= (mob.StartX + 5) && point.X == 0; x++)
                                {
                                    if (mapInfo.GetAttributes(x, y).Length == 0)
                                    {
                                        point = new Point(mob.StartX, mob.StartY);
                                    }
                                }
                            break;
                        default:
                            continue;
                    }
                    if (point.X == 0)
                        continue;

                    mob.monster = new Monster(mob.Class, ObjectType.Monster, mob.MapNumber, point, mob.StartDir == -1 ? (byte)Program.RandomProvider(7) : (byte)mob.StartDir) { Index = MonstersMng.Instance.GetNewIndex() };
                    mob.monster.Active = false;
                    MonstersMng.Instance.Monsters.Add(mob.monster);
                }
            }

            /*using (var tf = File.OpenText(file))
            {
                var content = tf.ReadToEnd();
                var IAGroup0Regex = new Regex(@"\n+([0-9]+)\s*\n+(?s)(.*?)end");
                var IAGroupRegex = new Regex(@"([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(\-*[0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(\-*[0-9]+)\s+(\-*[0-9]+)\s*");

                var m0 = IAGroup0Regex.Match(content);

                _IAGroups = IAGroupRegex
                    .Matches(m0.Groups[2].Value)
                    .Select(x => new MonsterIAGroup().AssignRegex(x.Groups.ToArray()))
                    .GroupBy(x => x.GroupNumber)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach(var group in _IAGroups)
                {
                    foreach(var mob in group.Value)
                    {
                        System.Drawing.Point point;
                        var mapInfo = ResourceCache.Instance.GetMaps()[mob.MapNumber];
                        switch (mob.CreateType)
                        {
                            case 0:
                                point = new System.Drawing.Point(mob.StartX, mob.StartY);
                                break;
                            case 1:
                                for (var y = mob.StartY - 5; y <= (mob.StartY + 5) && point.X == 0; y++)
                                    for (var x = mob.StartX - 5; x <= (mob.StartX + 5) && point.X == 0; x++)
                                    {
                                        if (mapInfo.GetAttributes(x, y).Length == 0)
                                        {
                                            point = new System.Drawing.Point(mob.StartX, mob.StartY);
                                        }
                                    }
                                break;
                            default:
                                continue;
                        }
                        if (point.X == 0)
                            continue;

                        mob.monster = new Monster(mob.Class, ObjectType.Monster, mob.MapNumber, point, mob.StartDir == -1 ? (byte)Program.RandomProvider(7) : (byte)mob.StartDir) { Index = MonstersMng.Instance.GetNewIndex() };
                        mob.monster.Active = false;
                        MonstersMng.Instance.Monsters.Add(mob.monster);
                    }
                }
            }*/
        }

        public static void Initialize(string root)
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new MonsterIA(root);
        }

        public static int Group(int group, bool active, EventHandler die = null)
        {
            foreach (var mob in _instance._IAGroups[group])
            {
                mob.monster.Active = active;

                if(active)
                    mob.monster.Die += die;
                else if(die != null)
                    mob.monster.Die -= die;
            }

            return _instance._IAGroups[group].Count;
        }
    }
}
