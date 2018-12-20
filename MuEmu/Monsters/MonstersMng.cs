using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MuEmu.Monsters
{
    public class MonstersMng
    {
        public const ushort MonsterStartIndex = 1000;
        private Dictionary<ushort, MonsterBase> _monsterInfo;

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

        public void LoadMonster(string file)
        {
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
                        Magic = int.Parse(m.Groups[10].Value),
                        Attack = int.Parse(m.Groups[11].Value),
                        Success = int.Parse(m.Groups[12].Value),
                        MoveRange = int.Parse(m.Groups[13].Value),
                        A_Type = int.Parse(m.Groups[14].Value),
                        A_Range = int.Parse(m.Groups[15].Value),
                        V_Range = int.Parse(m.Groups[16].Value),
                        MoveSpeed = int.Parse(m.Groups[17].Value),
                        A_Speed = int.Parse(m.Groups[18].Value),
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

            Console.WriteLine($"{_monsterInfo.Count} Type of Monsters");
        }

        public void LoadSetBase(string file)
        {
            using (var tf = File.OpenText(file))
            {
                var contents = tf.ReadToEnd();
                var NPCRegex = new Regex(@"\n+([0-9]+)\s*\n+(?s)(.*?)\nend");
                var NPCSubRegex = new Regex(@"([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+(\-*[0-9]+)\s*");
                var count = 0;
                foreach (Match m in NPCRegex.Matches(contents))
                {
                    switch (int.Parse(m.Groups[1].Value))
                    {
                        case 0:
                            foreach (Match sm in NPCSubRegex.Matches(m.Groups[2].Value))
                            {
                                Monsters.Add(new Monster(ushort.Parse(sm.Groups[1].Value), ObjectType.NPC, (Maps)ushort.Parse(sm.Groups[2].Value), new System.Drawing.Point(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[5].Value)), byte.Parse(sm.Groups[6].Value)) { Index = (ushort)(MonsterStartIndex + count) });
                                count++;
                            }
                            break;
                        case 2:
                            foreach (Match sm in NPCSubRegex.Matches(m.Groups[2].Value))
                            {
                                var dir = int.Parse(sm.Groups[6].Value);
                                if (dir < 0)
                                    dir = 0;
                                Monsters.Add(new Monster(ushort.Parse(sm.Groups[1].Value), ObjectType.Monster, (Maps)ushort.Parse(sm.Groups[2].Value), new System.Drawing.Point(int.Parse(sm.Groups[4].Value), int.Parse(sm.Groups[5].Value)), (byte)dir) { Index = (ushort)(MonsterStartIndex + count) });
                                count++;
                            }
                            break;
                    }
                }
                Console.WriteLine(Monsters.Count + " Monsters Loaded");
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
    }
}
