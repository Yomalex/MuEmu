using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Monsters;
using MuEmu.Network.Data;
using MuEmu.Network.Game;
using MuEmu.Network.QuestSystem;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Quests
    {
        private static Random _rand = new Random();
        private List<Quest> _quests;

        internal static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Quests));
        internal Dictionary<uint, byte> _questMonsterKillCount = new Dictionary<uint, byte>();

        public byte[] QuestStates { get; set; }
        public Player Player { get; set; }
        public Quests(Character @char, CharacterDto characterDto)
        {
            QuestStates =  new byte[20];
            _quests = new List<Quest>();
            Array.Fill<byte>(QuestStates, 0xff);

            Player = @char.Player;
            foreach(var q in characterDto.Quests)
            {
                var nq = new Quest
                {
                    Manager = this,
                    Index = q.Quest,
                    State = (QuestState)q.State,
                    _dbId = q.QuestId,
                };

                nq._needSave = false;
                _quests.Add(nq);
                var details = q.Details
                    .Split(";")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Split("="))
                    .ToDictionary(x => uint.Parse(x[0]), y => byte.Parse(y[1]));

                _logger
                    .ForAccount(Player.Session)
                    .Information("Quest Found:{0} State:{1}", nq.Details.Name, nq.State);

                foreach(var d in details)
                    _questMonsterKillCount.Add(d.Key, d.Value);
            }
        }

        public async void SendList()
        {
            var standarQuest = _quests.Where(x => x.Standar).ToArray();
            await Player.Session.SendAsync(new SQuestInfo { Count = (byte)standarQuest.Length, State = QuestStates });
            var customQuest = _quests.Where(x => !x.Standar).ToArray();
            await Player.Session.SendAsync(new SNewQuestInfo { QuestList = Array.Empty<NewQuestInfoDto>() });
        }

        public void OnMonsterDie(Monster monster)
        {
            var runningQuests = _quests.Where(x => x.State == QuestState.Reg);

            foreach(var q in runningQuests)
            {
                foreach(var sq in q.Details.Sub.Where(x => x.Allowed.Contains(Player.Character.Class)))
                {
                    if(sq.Monster != 0 && sq.Drop == 0)
                    {
                        var key = (sq.Monster | (uint)(q.Index << 16));

                        if (_questMonsterKillCount.ContainsKey(key))
                        {
                            if(_questMonsterKillCount[key] >= sq.Count)
                            {
                                continue;
                            }
                            _questMonsterKillCount[key]++;
                        }
                        else
                            _questMonsterKillCount.Add(key, 1);

                        Player
                            .Session
                            .SendAsync(new SNotice(NoticeType.Blue, $"{monster.Info.Name}: {_questMonsterKillCount[key]}/{sq.Count}"))
                            .Wait();

                        continue;
                    }

                    if (sq.MonsterMin > monster.Level ||
                        sq.MonsterMax < monster.Level)
                        continue;

                    if(sq.Drop > _rand.Next(100))
                    {
                        Item dropItem = null;
                        foreach(var it in sq.Requeriment)
                        {
                            var cantDrop = Player.Character.Inventory.FindAll(it.Number).Count() == it.Durability;
                            if (cantDrop)
                                continue;

                            dropItem = new Item(it.Number, 0, new { it.Plus });
                            break;
                        }

                        if(dropItem != null)
                            Player.Character.Map.AddItem(monster.Position.X, monster.Position.Y, dropItem);
                    }
                }
            }
        }

        public Quest GetByIndex(int Index)
        {
            return _quests.Where(x => x.Index == Index).FirstOrDefault();
        }

        public Quest Find(ushort npc)
        {
            var NPCQuests = from q in ResourceCache.Instance.GetQuests().Values
                            where q.NPC == npc
                            select q;

            var listed = from q in _quests
                        where NPCQuests.Any(x => x.Index == q.Index)
                        select q;

            var running = listed.Where(x => x.State == QuestState.Reg || x.State == QuestState.Unreg);
            if (running.Any())
                return running.First();

            var newQ = from q in NPCQuests
                       where !listed.Any(x => x.Index == q.Index) && q.CanRun(Player.Character)
                       select q;

            if (newQ.Any())
            {
                var nq = newQ.First();
                var newQuest = new Quest
                {
                    Index = nq.Index,
                    Manager = this,
                };

                newQuest.State = QuestState.Unreg;

                _quests.Add(newQuest);
                return newQuest;
            }

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

        public async Task Save(GameContext db)
        {
            foreach (var q in _quests)
                await q.Save(db);
        }
    }

    public class Quest
    {
        private int _index;
        private int _questByte;
        private byte _killCount;

        internal int _dbId;
        internal bool _needSave;

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
                _needSave = true;
            }
        }

        public byte KillCount
        {
            get => _killCount;
            set
            {
                if (value == _killCount)
                    return;

                _needSave = true;
                _killCount = value;
            }
        }

        public bool Standar { get; set; }
        public int Index { get => _index;
            set
            {
                _index = value;
                _questByte = _index / 4;
                Shift = (_index % 4) * 2;
                Details = Resources.ResourceCache.Instance.GetQuests()[_index];
            }
        }
        public int Shift { get; private set; }
        public QuestInfo Details { get; private set; }
        public Quests Manager { get; set; }
        public Character Character => Manager.Player.Character;

        public uint Cost => (uint)Details.Conditions.Sum(x => x.Cost);

        internal byte NextStep()
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
                                        Character.Changeup = true;
                                        RewardArg = Character.ClientClass;

                                        session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
                                        break;
                                    case QuestCompensation.Statup:
                                    case QuestCompensation.Plusstat:
                                        Character.LevelUpPoints += sq.Amount;
                                        RewardArg = sq.Amount;
                                        break;
                                    case QuestCompensation.Comboskill:
                                        RewardArg = 0;
                                        break;
                                    case QuestCompensation.Master:
                                        Character.LevelUpPoints += sq.Amount;

                                        if (!Character.Changeup && !(Character.Class == HeroClass.MagicGladiator || Character.Class == HeroClass.DarkLord))
                                        {
                                            return 1;
                                        }
                                        Character.MasterClass = true;
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

        internal async Task Save(GameContext db)
        {
            if (!_needSave)
                return;
            _needSave = false;

            var details = string.Join(";",Manager._questMonsterKillCount
                .Where(x => (x.Key & 0xFF00) >> 16 == Index)
                .Select(x => x.Key+"="+x.Value));

            var dto =
                    new QuestDto
                    {
                        QuestId = _dbId,
                        Quest = Index,
                        State = (byte)State,
                        Details = details,
                        CharacterId = Manager.Player.Character.Id,
                    };

            var msg = _dbId == 0 ? "Added" : "Updated";
            Quests._logger
                    .ForAccount(Manager.Player.Session)
                    .Information("Quest:{0} {1} State:{2}", Details.Name, msg, State);

            if (_dbId == 0)
                db.Quests.Add(dto);
            else
                db.Quests.Update(dto);

            await db.SaveChangesAsync();

            _dbId = dto.QuestId;
        }
    }
}
