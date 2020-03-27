using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MuEmu.Monsters
{
    public class MonstersMng
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MonstersMng));
        public const ushort MonsterStartIndex = 2000;
        private Random _rand = new Random();
        private Dictionary<ushort, MonsterBase> _monsterInfo;

        private List<ushort> _clearIndex = new List<ushort>();
        private ushort _lastUsedIndex = MonsterStartIndex-1;

        public Dictionary<ushort, MonsterBase> MonsterInfo => _monsterInfo;
        public static MonstersMng Instance { get; set; }
        public List<Monster> Monsters { get; set; }

        public MonstersMng()
        {
            if (Instance != null)
                throw new Exception("Already Initialized");

            _monsterInfo = new Dictionary<ushort, MonsterBase>();
            Monsters = new List<Monster>();
        }

        private ushort GetNewIndex()
        {
            var clearIndex = _clearIndex.FirstOrDefault();
            if (clearIndex != 0)
            {
                return clearIndex;
            }

            if (Monsters.Count() == 65535)
                throw new OverflowException();

            return ++_lastUsedIndex;
        }

        public void LoadMonster(string file)
        {
            Logger.Information("Loading monsters from '{0}'", file);
            using (var tf = File.OpenText(file))
            {
                var contents = tf.ReadToEnd();
                var MonsterRegex = new Regex(@"([\/0-9]+)\s+([0-9]+)\s+"+"\""+"(.*)"+"\""+@"\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s*");
                foreach(Match m in MonsterRegex.Matches(contents))
                {
                    //Console.WriteLine(m.Value);
                    if (m.Value.StartsWith("//"))
                        continue;

                    var monst = new MonsterBase
                    {
                        Monster = ushort.Parse(m.Groups[1].Value),
                        Rate = int.Parse(m.Groups[2].Value),
                        Name = m.Groups[3].Value,
                        Level = ushort.Parse(m.Groups[4].Value),
                        HP = int.Parse(m.Groups[5].Value),
                        MP = int.Parse(m.Groups[6].Value),
                        DmgMin = int.Parse(m.Groups[7].Value),
                        DmgMax = int.Parse(m.Groups[8].Value),
                        Defense = int.Parse(m.Groups[9].Value),
                        MagicDefense = int.Parse(m.Groups[10].Value),
                        Attack = int.Parse(m.Groups[11].Value),
                        Success = int.Parse(m.Groups[12].Value),
                        MoveRange = int.Parse(m.Groups[13].Value),
                        Spell = (Spell)ushort.Parse(m.Groups[14].Value),
                        AttackRange = int.Parse(m.Groups[15].Value),
                        ViewRange = int.Parse(m.Groups[16].Value),
                        MoveSpeed = int.Parse(m.Groups[17].Value),
                        AttackSpeed = int.Parse(m.Groups[18].Value),
                        RegenTime = int.Parse(m.Groups[19].Value),
                        Attribute = int.Parse(m.Groups[20].Value),
                        ItemRate = int.Parse(m.Groups[21].Value),
                        M_Rate = int.Parse(m.Groups[22].Value),
                        MaxItem = int.Parse(m.Groups[23].Value),
                        Skill = int.Parse(m.Groups[24].Value),
                    };
                    _monsterInfo.Add(monst.Monster, monst);
                }
            }

            Logger.Information($"{_monsterInfo.Count} Type of Monsters");
            //Console.WriteLine($"{_monsterInfo.Count} Type of Monsters");
        }

        public void LoadSetBase(string file)
        {
            Logger.Information("Loading monsters ubication from '{0}'", file);
            using (var tf = File.OpenText(file))
            {
                var contents = tf.ReadToEnd();
                var NPCRegex = new Regex(@"\n+([0-9]+)\s*\n+(?s)(.*?)\nend");
                var NPCSubRegex = new Regex(@"\n+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(\-*[0-9]+)\s*");
                var SpotRegex = new Regex(@"\n+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(\-*[0-9]+)\s+([0-9]+)\s*");

                foreach (Match m in NPCRegex.Matches(contents))
                {
                    switch (int.Parse(m.Groups[1].Value))
                    {
                        case 0:
                            foreach (Match sm in NPCSubRegex.Matches(m.Groups[2].Value))
                            {
                                var mType = ushort.Parse(sm.Groups[1].Value);
                                var mMap = (Maps)ushort.Parse(sm.Groups[2].Value);
                                var mPos = new Point(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[5].Value));
                                var mDir = byte.Parse(sm.Groups[6].Value);
                                Monsters.Add(new Monster(mType, ObjectType.NPC, mMap, mPos, mDir) { Index = GetNewIndex() });
                            }
                            break;
                        case 1:
                            foreach(Match sm in SpotRegex.Matches(m.Groups[2].Value))
                            {
                                for(var i = 0; i < ushort.Parse(sm.Groups[9].Value); i++)
                                {
                                    var minX = Math.Min(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[6].Value));
                                    var maxX = Math.Max(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[6].Value));
                                    var minY = Math.Min(int.Parse(sm.Groups[5].Value), int.Parse(sm.Groups[7].Value));
                                    var maxY = Math.Max(int.Parse(sm.Groups[5].Value), int.Parse(sm.Groups[7].Value));
                                    var dir = (byte)_rand.Next(7);
                                    var mIndex = ushort.Parse(sm.Groups[1].Value);
                                    var mMap = (Maps)ushort.Parse(sm.Groups[2].Value);
                                    try
                                    {
                                        var mPos = GetSpawn(mMap, minX, maxX, minY, maxY);
                                        Monsters.Add(new Monster(mIndex, ObjectType.Monster, mMap, mPos, dir) { Index = GetNewIndex() });
                                    }
                                    catch (Exception)
                                    {
                                        Logger.Error("Invalid monster spawn area Map:{4} X:{0}-{1} Y:{2}-{3}", minX, maxX, minY, maxY, mMap);
                                    }
                                }       
                            }
                            break;
                        case 2:
                            foreach (Match sm in NPCSubRegex.Matches(m.Groups[2].Value))
                            {
                                var mIndex = ushort.Parse(sm.Groups[1].Value);
                                var mMap = (Maps)ushort.Parse(sm.Groups[2].Value);
                                var dir = (byte)int.Parse(sm.Groups[6].Value);
                                var mPos = new Point(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[5].Value));
                                if (dir > 7)
                                    dir = 0;
                                Monsters.Add(new Monster(mIndex, ObjectType.Monster, mMap, mPos, dir) { Index = GetNewIndex() });
                            }
                            break;
                        case 4:
                            foreach (Match sm in NPCSubRegex.Matches(m.Groups[2].Value))
                            {
                                var mIndex = ushort.Parse(sm.Groups[1].Value);
                                var mMap = (Maps)ushort.Parse(sm.Groups[2].Value);
                                var dir = (byte)int.Parse(sm.Groups[6].Value);
                                var mPos = new Point(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[5].Value));
                                if (dir > 7)
                                    dir = 0;
                                Monsters.Add(new Monster(mIndex, ObjectType.Monster, mMap, mPos, dir) { Index = GetNewIndex() });
                            }
                            break;
                    }
                }
                Logger.Information(Monsters.Count + " Monsters Loaded");
            }
        }

        public Monster GetMonster(ushort Index)
        {
            return Monsters.First(x => x.Index == Index);
        }
        
        public static void Initialize()
        {
            Instance = new MonstersMng();
        }

        public Point GetSpawn(Maps map, int minX, int maxX, int minY, int maxY)
        {
            var x = 0;
            var y = 0;
            var z = 0;
            var mMap = ResourceCache.Instance.GetMaps()[map];
            MapAttributes[] att;
            do
            {
                x = _rand.Next(minX, maxX);
                y = _rand.Next(minY, maxY);
                att = mMap
                .GetAttributes(x, y);
            } while (
                (att.Contains(MapAttributes.Safe) ||
                att.Contains(MapAttributes.NoWalk) ||
                att.Contains(MapAttributes.Hide))
                && ++z < 10
                );

            if (z == 10)
                throw new InvalidOperationException();

            return new Point(x, y);
        }
    }
}
