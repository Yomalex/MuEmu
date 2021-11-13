using MU.Resources;
using MuEmu.Resources;
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
        private static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MonsterIA));

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
        }

        public static void Initialize(string root)
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new MonsterIA(root);
        }

        public static int InitGroup(int group, EventHandler die = null)
        {
            _logger.Information(ServerMessages.GetMessage(Messages.IA_CreateGroup)/*"Loading group {0}"*/, group);
            Monster Leader = null;
            foreach (var mob in _instance._IAGroups[group])
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
                //mob.monster.Active = false;
                mob.monster.Die += die;
                if(mob.Rank == 0)
                {
                    Leader = mob.monster;
                }
                else if(Leader != null)
                {
                    mob.monster.Leader = Leader;
                }
                MonstersMng.Instance.Monsters.Add(mob.monster);
            }

            return _instance._IAGroups[group].Count;
        }

        public static void DelGroup(int group)
        {
            _logger.Information(ServerMessages.GetMessage(Messages.IA_DeleteGroup)/*"Removing group {0}"*/, group);
            foreach (var mob in _instance._IAGroups[group])
            {
                MonstersMng.Instance.DeleteMonster(mob.monster);
            }
        }
    }
}
