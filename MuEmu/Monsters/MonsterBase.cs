using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Monsters
{
    public class MonsterBase
    {
        public ushort Monster { get; set; }
        public int Rate { get; set; }
        public string Name { get; set; }
        public ushort Level { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int DmgMin { get; set; }
        public int DmgMax { get; set; }
        public int Defense { get; set; }
        public int MagicDefense { get; set; }
        public int Attack { get; set; }
        public int Success { get; set; }
        public int MoveRange { get; set; }
        public Spell Spell { get; set; }
        public int AttackRange { get; set; }
        public int ViewRange { get; set; }
        public int MoveSpeed { get; set; }
        public int AttackSpeed { get; set; }
        public int RegenTime { get; set; }
        public int Attribute { get; set; }
        public int ItemRate { get; set; }
        public int M_Rate { get; set; }
        public int MaxItem { get; set; }
        public int Skill { get; set; }
    }
}
