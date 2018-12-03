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
        public int Magic { get; set; }
        public int Attack { get; set; }
        public int Success { get; set; }
        public int MoveRange { get; set; }
        public int A_Type { get; set; }
        public int A_Range { get; set; }
        public int V_Range { get; set; }
        public int MoveSpeed { get; set; }
        public int A_Speed { get; set; }
        public int RegenTime { get; set; }
        public int Attribute { get; set; }
        public int ItemRate { get; set; }
        public int M_Rate { get; set; }
        public int MaxItem { get; set; }
        public int Skill { get; set; }
    }
}
