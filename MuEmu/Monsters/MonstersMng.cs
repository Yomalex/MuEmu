using MuEmu.Resources;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using MuEmu.Util;
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

        public ushort GetNewIndex()
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
            if(File.Exists(file+".xml"))
            {
                Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Loading), file + ".xml");
                var xml = ResourceLoader.XmlLoader<XmlMonsterInfo>(file + ".xml");
                _monsterInfo = xml.Monsters.ToDictionary(x => x.Monster);
            }
            else if (File.Exists(file + ".txt"))
            {
                Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Loading), file + ".txt");
                var loader = new LoadWZTXT<XmlMonsterInfo>();
                var xml = loader.Load(file + ".txt");
                foreach (var monst in xml.Monsters)
                {
                    if (monst.Spell >= (Spell)100 && monst.Spell < (Spell)200)
                    {
                        monst.Spell -= 100;
                        monst.AttackRange += 2;
                    }
                    _monsterInfo.Add(monst.Monster, monst);
                }

                //xml.Monsters = _monsterInfo.Select(x => x.Value).ToArray();
                ResourceLoader.XmlSaver(file + ".xml", xml);
            }

            Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Types), _monsterInfo.Count);
        }

        public void LoadSetBase(string file)
        {
            XmlMonsterSetBase xml = null;
            if (File.Exists(file + ".xml"))
            {
                Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Loading2), file + ".xml");
                xml = ResourceLoader.XmlLoader<XmlMonsterSetBase>(file + ".xml");
            }
            else if (File.Exists(file + ".txt"))
            {
                Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Loading2), file + ".txt");
                var loader = new LoadWZTXT<XmlMonsterSetBase>();
                xml = loader.Load(file + ".txt");
                ResourceLoader.XmlSaver(file + ".xml", xml);
            }

            foreach(var npc in xml.NPCs)
                Monsters.Add(new Monster(npc.Type, ObjectType.NPC, npc.Map, new Point(npc.PosX, npc.PosY), (byte)npc.Dir) { Index = GetNewIndex() });

            foreach (var npc in xml.Normal)
                Monsters.Add(new Monster(npc.Type, ObjectType.Monster, npc.Map, new Point(npc.PosX, npc.PosY), (byte)npc.Dir) { Index = GetNewIndex() });

            foreach (var npc in xml.BloodCastles)
                Monsters.Add(new Monster(npc.Type, ObjectType.Monster, npc.Map, new Point(npc.PosX, npc.PosY), (byte)npc.Dir) { Index = GetNewIndex() });

            foreach (var npc in xml.Golden)
            {
                for (var i = 0; i < npc.Quant; i++)
                {
                    var dir = (byte)_rand.Next(7);
                    var mPos = GetSpawn(npc.Map, npc.PosX, npc.PosX2, npc.PosY, npc.PosY2);
                    var mob = new Monster(npc.Type, ObjectType.Monster, npc.Map, mPos, dir) { Index = GetNewIndex() };
                    Monsters.Add(mob);
                    Program.GoldenInvasionManager.AddMonster(mob);
                }
            }

            foreach (var npc in xml.Spots)
            {
                for (var i = 0; i < npc.Quant; i++)
                {
                    try
                    {
                        var dir = (byte)_rand.Next(7);
                        var mPos = GetSpawn(npc.Map, npc.PosX, npc.PosX2, npc.PosY, npc.PosY2);
                        var mob = new Monster(npc.Type, ObjectType.Monster, npc.Map, mPos, dir, npc.Element) { Index = GetNewIndex() };
                        Monsters.Add(mob);
                    }catch(InvalidOperationException)
                    { }
                }
            }
            Logger.Information(Program.ServerMessages.GetMessage(Messages.MonsterMng_Loaded), Monsters.Count);
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

            if(maxX == 0 && maxY == 0)
                return new Point(minX, minY);

            var _minX = Math.Min(minX, maxX);
            var _maxX = Math.Max(minX, maxX);
            var _minY = Math.Min(minY, maxY);
            var _maxY = Math.Max(minY, maxY);

            MapAttributes[] att = new MapAttributes[] { MapAttributes.Safe, MapAttributes.NoWalk, MapAttributes.Hide};
            do
            {
                x = _rand.Next(_minX, _maxX);
                y = _rand.Next(_minY, _maxY);
            } while (
                mMap.ContainsAny(x,y, att)
                && ++z < 10
                );

            if (z == 10)
                throw new InvalidOperationException();

            return new Point(x, y);
        }
    }
}
