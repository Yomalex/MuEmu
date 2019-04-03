using MU.DataBase;
using MuEmu.Data;
using MuEmu.Network.Data;
using MuEmu.Network.Game;
using MuEmu.Network.QuestSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Quests
    {
        public byte[] QuestStates { get; set; }
        public Player Player { get; set; }
        private List<Quest> _quests;
        public Quests(Character @char, CharacterDto characterDto)
        {
            _quests = new List<Quest>();
            Player = @char.Player;
            QuestStates =  new byte[20];
            Array.Fill<byte>(QuestStates, 0xff);
        }

        public async void SendList()
        {
            var standarQuest = _quests.Where(x => x.Standar).ToArray();
            await Player.Session.SendAsync(new SQuestInfo { Count = (byte)standarQuest.Length, State = Array.Empty<byte>() });
            var customQuest = _quests.Where(x => !x.Standar).ToArray();
            await Player.Session.SendAsync(new SNewQuestInfo { QuestList = Array.Empty<NewQuestInfoDto>() });
        }

        public void OnMonsterDie(ushort type)
        {
            
        }

        public Quest GetByIndex(int Index)
        {
            return _quests.Where(x => x.Index == Index).FirstOrDefault();
        }

        public Quest Find(ushort npc)
        {
            var quests = (from q in _quests
                        from sq in q.Details.Conditions
                        where q.Details.NPC == npc
                        select q.Details)
                        .OrderByDescending( x => x.Index );

            var newQuests = (from q in Resources.ResourceCache.Instance.GetQuests()
                             let qs = q.Value
                             from sq in qs.Conditions
                             where qs.NPC == npc && sq.CanRun(Player.Character)
                             select q.Value)
                            .OrderByDescending(x => x.Index)
                            .Except(quests);

            if(newQuests.Count() > 0)
            {
                var nq = newQuests.First();
                var newQuest = new Quest
                {
                    Index = nq.Index,
                    Manager = this,
                };

                newQuest.State = QuestState.Unreg;

                _quests.Add(newQuest);
                return newQuest;
            }

            var active = quests.FirstOrDefault();
            if (active != null)
                return _quests.First(x => x.Index == active.Index);

            return null;
        }

        public bool IsClear(int Index)
        {
            if (Index == -1)
                return true;

            var q = GetByIndex(Index);
            if (q == null)
                return false;
            return (q.State == QuestState.Complete || q.State == QuestState.Clear);
        }

        public byte SetState(int Index)
        {
           return GetByIndex(Index)?.NextStep() ?? 0xff;
        }
    }

    public class Quest
    {
        private int _index;
        private int _questByte;
        private int _shift;

        public byte StateByte => Manager.QuestStates[_questByte];
        public QuestState State
        {
            get => (QuestState)((StateByte >> Shift) & 0x03);
            set
            {
                var curState = StateByte;
                var curMask = (byte)(~(0x03 << Shift));

                curState &= curMask;
                curState |= (byte)(((byte)value) << Shift);
                Manager.QuestStates[_questByte] = curState;
            }
        }

        public bool Standar { get; set; }
        public int Index { get => _index;
            set
            {
                _index = value;
                _questByte = _index / 4;
                _shift = (_index % 4) * 2;
                Details = Resources.ResourceCache.Instance.GetQuests()[_index];
            }
        }
        public int Shift => _shift;
        public QuestInfo Details { get; private set; }
        public Quests Manager { get; set; }
        public Character Character => Manager.Player.Character;

        public uint Cost => (uint)Details.Conditions.Sum(x => x.Cost);

        public byte NextStep()
        {
            byte result = 0xff;
            switch(State)
            {
                case QuestState.Unreg:
                case QuestState.Clear:
                    result = canRun();

                    if (result != 0)
                        return result;

                    Character.Money -= Cost;
                    State = QuestState.Reg;
                    return 0;
                case QuestState.Reg:
                    result = canClear();

                    if (result != 0)
                        return result;

                    State = QuestState.Complete;
                    return 0;
                default:
                    return 0xff;
            }
        }

        private byte canClear()
        {
            foreach(var sq in Details.Sub)
            {
                if(sq.Allowed.Contains(Character.Class))
                {
                    if (sq.Monster != 0)
                    {

                    }
                    else
                    {
                        var inv = Character.Inventory;
                        foreach (var req in sq.Requeriment)
                        {
                            var Items = (from it in inv.FindAllItems(req.Number)
                                        where it.Plus == req.Plus
                                        select it)
                                        .Take(sq.Count);

                            if (Items.Count() == sq.Count)
                            {
                                foreach (var x in Items)
                                    inv.Delete(x);

                                var session = Character.Player.Session;

                                byte RewardArg = 0;

                                switch (sq.CompensationType)
                                {
                                    case QuestCompensation.Changeup:
                                        Character.LevelUpPoints += sq.Amount;
                                        Character.Class |= (HeroClass)0x01;
                                        RewardArg = Character.ClientClass;

                                        session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
                                        break;
                                    case QuestCompensation.Statup:
                                        Character.LevelUpPoints += sq.Amount;
                                        RewardArg = sq.Amount;
                                        break;
                                    case QuestCompensation.Plusstat:
                                        break;
                                    case QuestCompensation.Comboskill:
                                        break;
                                    case QuestCompensation.Master:
                                        Character.LevelUpPoints += sq.Amount;

                                        if (!Character.Changeup && !(Character.Class == HeroClass.MagicGladiator || Character.Class == HeroClass.DarkLord))
                                        {
                                            return 1;
                                        }
                                        var newClass = Character.BaseClass | (HeroClass)0x02;
                                        Character.Class = newClass;
                                        RewardArg = Character.ClientClass;
                                        session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
                                        break;
                                }

                                session.SendAsync(new SSendQuestPrize((ushort)session.ID, sq.CompensationType, RewardArg));

                                return 0;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private byte canRun()
        {
            return (byte)(Details.Conditions
                .LastOrDefault(x => !x.CanRun(Character))?.Message ?? 0);
        }
    }
}
