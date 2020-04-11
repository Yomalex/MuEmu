using MuEmu.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Data
{
    public class QuestInfo
    {
        public int Type { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public ushort NPC { get; set; }
        public List<SubQuest> Sub { get; set; }
        public List<RunConditions> Conditions { get; set; }
    }

    public class SubQuest
    {
        public HeroClass[] Allowed { get; set; }
        public List<Item> Requeriment { get; set; }
        public ushort Monster { get; set; }
        public ushort MonsterMin { get; set; }
        public ushort MonsterMax { get; set; }
        public int Count { get; set; }
        public Dictionary<QuestState, ushort> Messages { get; set; }
        public QuestCompensation CompensationType { get; set; }
        public byte Amount { get; set; }
        public ushort Drop { get; set; }
    }

    public class RunConditions
    {
        public int NeededQuestIndex { get; set; }
        public ushort MinLevel { get; set; }
        public ushort MaxLevel { get; set; }
        public int NeedStr { get; set; }
        public int Cost { get; set; }
        public int Message { get; set; }

        public bool CanRun(Character @char)
        {
            if ((MinLevel > @char.Level && MinLevel != 0) || (@char.Level > MaxLevel && MaxLevel != 0))
                return false;

            if (@char.Money < Cost)
                return false;

            if (!@char.Quests.IsClear(NeededQuestIndex))
                return false;

            return true;
        }
    }
}
