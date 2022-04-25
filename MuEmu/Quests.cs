using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Monsters;
using MuEmu.Network.Data;
using MU.Network.Game;
using MU.Network.QuestSystem;
using MuEmu.Resources;
using MuEmu.Resources.XML;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MU.Resources;
using MuEmu.Util;

namespace MuEmu
{
    public class QuestInfoIndex
    {
        public AskType Type { get; set; }
        public uint Index { get; set; }
        public uint Episode { get => (Index >> 0x10); set => Index = Switch + value << 0x10; }
        public uint Switch { get => (Index & 0xFFFF); set => Index = (Episode << 0x10) + value; }

        public static implicit operator QuestInfoIndex(uint id)
        {
            return new QuestInfoIndex { Index = id };
        }

        public static implicit operator uint(QuestInfoIndex info)
        {
            return info.Index;
        }

        public static QuestInfoIndex FromEpisodeSwitch(uint Ep, uint Sw)
        {
            return new QuestInfoIndex { Episode = Ep, Switch = Sw };
        }

        public override string ToString()
        {
            return $"EP{Episode}-{Switch}";
        }
    }
    public class QuestInfoMonster : QuestInfoIndex
    {
        public ushort MonsterClass { get; set; }
        public uint Current { get; set; }
        public uint Value { get; set; }
    }
    public class Quests
    {
        private static Random _rand = new Random();
        private List<Quest> _quests;
        private Dictionary<int, QuestInfoIndex> _episodes = new Dictionary<int, QuestInfoIndex>();
        private QuestEXPDto _questEXP;
        private QuestInfoIndex _currentQuest;
        private ushort _currentNpc;

        internal static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Quests));
        internal Dictionary<uint, byte> _questMonsterKillCount = new Dictionary<uint, byte>();

        public byte[] QuestStates { get; set; }
        public Player Player { get; set; }
        public Quests(Character @char, CharacterDto characterDto)
        {
            if(_questEXP == null)
            {
                try
                {
                    string file = Program.XMLConfiguration.Files.QuestWorld+$"Quest_{Program.Season}.xml";
                    _questEXP = ResourceLoader.XmlLoader<QuestEXPDto>(file);
                }catch(Exception)
                {
                    _questEXP = new QuestEXPDto()
                    {
                        QuestList = Array.Empty<QuestNPCDto>()
                    };
                }
            }
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

            foreach(var q in characterDto.QuestEX)
            {
                var qIndex = QuestInfoIndex.FromEpisodeSwitch((uint)q.Quest, (uint)q.State);
                _episodes.Add(q.Quest, qIndex);
                _logger
                    .ForAccount(Player.Session)
                    .Information("Quest Found:{0}", qIndex);
            }
        }

        internal uint GetEpisodeState(uint episode)
        {
            if (_episodes.TryGetValue((int)episode, out QuestInfoIndex value))
            {
                return value.Switch;
            }

            return 0;
        }
        internal void SetEpisodeState(uint episode, uint _switch)
        {
            if(_episodes.TryGetValue((int)episode, out QuestInfoIndex value))
            {
                value.Switch = _switch;
                return;
            }

            _episodes.Add((int)episode, QuestInfoIndex.FromEpisodeSwitch(episode, _switch));
        }

        public async void SendList()
        {
            var standarQuest = _quests.Where(x => x.Standar).ToArray();
            await Player.Session.SendAsync(new SQuestInfo { Count = (byte)standarQuest.Length, State = QuestStates });
            /*var customQuest = _quests.Where(x => !x.Standar).ToArray();
            await Player.Session.SendAsync(new SNewQuestInfo { QuestList = customQuest.Select((x,i) => new NewQuestInfoDto
            {
                Number = (ushort)i,
                Quest = (ushort)x.Index
            }).ToArray() });*/
            await Player.Session.SendAsync(new SNQWorldLoad());
            await Player.Session.SendAsync(new SNQWorldList { Quest = new SNQWorldListDto { QuestIndex = 11, TagetNumber = 0, QuestState = 1 } });
        }

        public IEnumerable<QuestInfoIndex> EXPListNPC(ushort npc)
        {
            var session = Player.Session;
            var list = _questEXP.QuestList.FirstOrDefault(x => x.Index == npc)?.QuestInfo??null;
            _currentNpc = npc;

            var quest = Array.Empty<QuestInfoIndex>();
            if(list != null)
            {
                quest = (from l in list
                        where 
                        l.MinLevel <= Player.Character.Level && 
                        l.MaxLevel >= Player.Character.Level &&
                        (l.ReqEpisode == 0 || _episodes.ContainsKey((int)l.ReqEpisode))
                         select QuestInfoIndex.FromEpisodeSwitch(l.Episode, 0)).ToArray();

                foreach(var q in quest)
                {
                    if(_episodes.ContainsKey((int)q.Episode))
                    {
                        q.Switch = _episodes[(int)q.Episode].Switch;
                    }
                }
            }
            return quest.Where(x => x.Switch < 65535);
        }

        private QuestNPCStateDto GetQuestEXPStateInfo(uint Episode, uint state)
        {
            var npcQuestList = (from q in _questEXP.QuestList
                                where q.Index == _currentNpc
                                select q.QuestInfo).FirstOrDefault();

            var quest = (from q in npcQuestList
                         where q.Episode == Episode
                         select q).FirstOrDefault();

            var nextState = (from q in quest.QuestState
                             where q.State == state
                             select q).FirstOrDefault();

            return nextState;
        }

        public async Task QuestEXPInfo(QuestInfoIndex info)
        {
            var session = Player.Session;
            var @char = Player.Character;

            try
            {
                if (!_episodes.ContainsKey((int)info.Episode))
                    _episodes.Add((int)info.Episode, info);

                var state = _episodes[(int)info.Episode].Switch;
                byte rewardCount = 0;

                var nextState = GetQuestEXPStateInfo(info.Episode, state);

                var ask = new AskInfoDto[5];
                var reward = new RewardInfoDto[5];
                for (var i = 0; i < 5; i++)
                {
                    ask[i] = new AskInfoDto();
                    reward[i] = new RewardInfoDto();
                }

                if (nextState.RewardEXP > 0)
                {
                    reward[rewardCount].Type = RewardType.Exp;
                    reward[rewardCount].Value = nextState.RewardEXP;
                    rewardCount++;
                }

                if (nextState.RewardGENS > 0)
                {
                    reward[rewardCount].Type = RewardType.Point;
                    reward[rewardCount].Value = nextState.RewardGENS;
                    rewardCount++;
                }

                if (nextState.RewardZEN > 0)
                {
                    reward[rewardCount].Type = RewardType.Zen;
                    reward[rewardCount].Value = nextState.RewardZEN;
                    rewardCount++;
                }

                byte askCount = 0;
                switch (nextState.Type)
                {
                    case AskType.Tutorial:
                        ask[askCount].Type = nextState.Type;
                        break;
                    case AskType.Item:
                        foreach (var it in nextState.Item)
                        {
                            var item = new Item(ItemNumber.FromTypeIndex(it.Type, it.Index), Options: new { Plus = it.Level });

                            var list = @char.Inventory.FindAllItems(ItemNumber.FromTypeIndex(it.Type, it.Index))
                            .Where(x => x.Plus == it.Level && it.Skill == x.Skill && it.Option == x.Option28 && it.Excellent == x.OptionExe);

                            ask[askCount].Type = nextState.Type;
                            ask[askCount].ItemInfo = item.GetBytes();
                            ask[askCount].CurrentValue = (uint)list.Count();
                            ask[askCount].Value = it.Count;
                            askCount++;
                        }
                        break;
                    case AskType.Monster:
                        var infoM = @char.Quests.GetEpisode<QuestInfoMonster>((int)info.Episode, state);
                        infoM.Type = nextState.Type;
                        foreach (var it in nextState.Monster)
                        {
                            ask[askCount].Type = nextState.Type;
                            ask[askCount].Index = it.Index;
                            ask[askCount].CurrentValue = infoM.Current;
                            ask[askCount].Value = it.Count;
                            infoM.MonsterClass = it.Index;
                            askCount++;
                        }
                        break;
                }
                if (rewardCount > 0 || askCount > 0)
                {
                    await session.SendAsync(new SSendQuestEXPProgressAsk
                    {
                        dwQuestInfoIndexID = QuestInfoIndex.FromEpisodeSwitch(info.Episode, state),
                        AskCnt = askCount,
                        RandRewardCnt = 0,
                        RewardCnt = rewardCount,
                        Asks = ask,
                        Rewards = reward,
                    });
                }
                else
                {
                    await session.SendAsync(new SSendQuestEXPProgress
                    {
                        dwQuestInfoIndexID = QuestInfoIndex.FromEpisodeSwitch(info.Episode, state),
                        /*AskCnt = 0,
                        RandRewardCnt = 0,
                        RewardCnt = 0,*/
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "QuestInfo");
                await session.SendAsync(new SQuestEXP { Result = 1 });
            }
        }

        public void QuestEXPSetProgress(QuestInfoIndex info, int result)
        {
            var state = GetEpisodeState(info.Episode);
            var nextState = GetQuestEXPStateInfo(info.Episode, state);
            var select = (ushort)nextState.Get("Select" + result);
            SetEpisodeState(info.Episode, select);
            _logger.ForAccount(Player.Session).Information("Update state EP{0} {1}->{2}", info.Episode, state, select);
        }

        public async Task QuestEXPCompleted(QuestInfoIndex info)
        {
            var session = Player.Session;
            var @char = Player.Character;

            var ss = GetEpisodeState(info.Episode);
            var state = GetQuestEXPStateInfo(info.Episode, ss);
            SetEpisodeState(info.Episode, state.Select1);

            if (state.Item != null && state.Item.Length > 0)
            {
                foreach (var it in state.Item)
                {
                    var list = @char.Inventory.FindAllItems(ItemNumber.FromTypeIndex(it.Type, it.Index))
                        .Where(x => x.Plus == it.Level && it.Skill == x.Skill && it.Option == x.Option28 && it.Excellent == x.OptionExe);

                    if (list.Count() < it.Count)
                    {
                        await session.SendAsync(new SQuestEXP { Result = 1 });
                        return;
                    }
                }
            }

            @char.Money += state.RewardZEN;
            @char.Experience += state.RewardEXP;
            await session.SendAsync(new SKillPlayerEXT(ushort.MaxValue, (int)state.RewardEXP, 0));
            @char.Gens.Contribution += (int)state.RewardGENS;

            var rewardItemOut = "";
            if (state.RewardItem != null)
            {
                foreach (var it in state.RewardItem)
                {
                    var item = new Item(ItemNumber.FromTypeIndex(it.Type, it.Index), Options: new { Plus = it.Level });
                    @char.Inventory.Add(item);
                    rewardItemOut += item.ToString() + "\n";
                }
            }

            _logger
                .Information($"[{info}] Quest completed! Reward List:\n\t\tZEN:{state.RewardZEN}\n\t\tEXP:{state.RewardEXP}\n\t\tGENS:{state.RewardGENS}\n\t\tItems:{rewardItemOut}\n");
            await session.SendAsync(new SSendQuestEXPComplete
            {
                dwQuestInfoIndexID = QuestInfoIndex.FromEpisodeSwitch(info.Episode, state.Select1),
                Result = 1,
            });
        }

        public uint[] QuestEXPProgressList()
        {
            return _episodes.Select(x => x.Value.Index).ToArray();
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
                        if (sq.Monster != monster.Info.Monster)
                            continue;

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

                            dropItem = new Item(it.Number, new { it.Plus });
                            break;
                        }

                        if(dropItem != null)
                            Player.Character.Map.AddItem(monster.Position.X, monster.Position.Y, dropItem);
                    }
                }
            }

            foreach(var episode in _episodes.Values)
            {
                if (episode.Type != AskType.Monster)
                    continue;

                var mons = episode as QuestInfoMonster;
                if(mons.MonsterClass == monster.Info.Monster)
                {
                    mons.Current++;
                }
            }
        }

        internal T GetEpisode<T>(int episode, uint _switch)
            where T: class
        {
            if(_episodes.ContainsKey(episode))
            {
                if(_episodes[episode] is T)
                    return (T)(object)_episodes[episode];
            }

            var newObject = (QuestInfoIndex)Activator.CreateInstance(typeof(T));
            newObject.Index = QuestInfoIndex.FromEpisodeSwitch((uint)episode, _switch);

            if(_episodes.ContainsKey(episode))
            {
                _episodes[episode] = newObject;
            }
            else
            {
                _episodes.Add(episode, newObject);
            }

            return (T)(object)newObject;
        }

        internal QuestInfoIndex GetLastEpisode()
        {
            return _episodes.Last().Value;
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

            var running = listed.Where(x => (x.State == QuestState.Reg || x.State == QuestState.Unreg) && x.Details.CanRun(Player.Character));
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

            foreach(var q in _episodes)
            {
                var entity = db.QuestsEX.SingleOrDefault(x => x.CharacterId == Player.Character.Id && x.Quest == q.Key);
                if (entity == null)
                    entity = new QuestEXDto();

                entity.Quest = q.Key;
                entity.State = (int)q.Value.Switch;
                entity.CharacterId = Player.Character.Id;
                if (entity.QuestId == 0)
                    db.QuestsEX.Add(entity);
                else
                    db.QuestsEX.Update(entity);
            }
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
            var sub = Details.Sub.Find(x => x.Allowed.Any(y => y == Character.Class));

            if (sub == null)
                return 0;

            return (byte)(Details.Conditions.FindAll(x => x.Index == sub.Index || x.Index == -1)
                .LastOrDefault(x => !x.CanRun(Character))?.Message ?? 0);
        }

        internal async Task Save(GameContext db)
        {
            if (!_needSave)
                return;
            _needSave = false;

            var details = string.Join(";",Manager._questMonsterKillCount
                .Where(x => (x.Key & 0xFF0000) >> 16 == Index)
                .Select(x => x.Key+"="+x.Value));

            var dto =
                    new MU.DataBase.QuestDto
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
