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
    internal struct QuestKCInfo
    {
        public uint Number
        {
            get => (uint)((Quest << 24) | (Monster << 8) | Count);
            set
            {
                Quest = (value & 0xff000000) >> 24;
                Monster = (ushort)((value & 0x00ffff00) >> 8);
                Count = (byte)(value & 0xff);
            }
        }
        
        public uint Quest { get; set; }
        public ushort Monster { get; set; }
        public byte Count { get; set; }
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
        internal List<QuestKCInfo> _questMonsterKillCount = new List<QuestKCInfo>();

        public byte[] QuestStates { get; set; }
        public Player Player { get; set; }
        public Quests(Character @char, CharacterDto characterDto)
        {
            if(_questEXP == null)
            {
                string file = Program.XMLConfiguration.Files.DataRoot+Program.XMLConfiguration.Files.QuestWorld+$"Quest_{Program.Season}.xml";
                try
                {
                    _questEXP = ResourceLoader.XmlLoader<QuestEXPDto>(file);
                }catch(Exception)
                {
                    _questEXP = new QuestEXPDto()
                    {
                        QuestList = Array.Empty<QuestNPCDto>()
                    };
                    ResourceLoader.XmlSaver(file, _questEXP);
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
                _questMonsterKillCount = q.Details
                    .Split(";")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new QuestKCInfo() { Number = uint.Parse(x) })
                    .ToList();

                _logger
                    .ForAccount(Player.Session)
                    .Information("Quest Found:{0} State:{1}", nq.Details.Name, nq.State);
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
            //var customQuest = _quests.Where(x => !x.Standar).ToArray();
            await Player.Session.SendAsync(new SNQWorldLoad());
            await Player.Session.SendAsync(new SNewQuestInfo { 
                QuestList = _episodes.Values.Select(x => x.Index).ToArray() });
            await Player.Session.SendAsync(new SNQWorldList { Quest = new SNQWorldListDto { QuestIndex = 1, TagetNumber = 1, QuestState = 0 } });
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
            return quest;
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
                        var id = _questMonsterKillCount.FindIndex(x => x.Quest == q.Index && x.Monster == sq.Monster);
                        var info = (id == -1) ? new QuestKCInfo { Monster = sq.Monster, Quest = (uint)q.Index } : _questMonsterKillCount[id];

                        if(info.Count < sq.Count)
                        info.Count++;

                        if (id == -1)
                        {
                            id = _questMonsterKillCount.Count;
                            _questMonsterKillCount.Add(info);
                        }

                        _questMonsterKillCount[id] = info;

                        Player
                            .Session
                            .SendAsync(new SNotice(NoticeType.Blue, $"{monster.Info.Name}: {info.Count}/{sq.Count}"))
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

            var running = listed.Where(x => x.State != QuestState.Complete).FirstOrDefault();
            if (running != null)
            {
                return running;
            }

            var newQ = (from q in NPCQuests
                       where !listed.Any(x => x.Index == q.Index) && q.CanRun(Player.Character)
                       select q).FirstOrDefault();

            if (newQ != null)
            {
                var newQuest = new Quest
                {
                    Index = newQ.Index,
                    Manager = this,
                };

                newQuest.State = QuestState.Unreg;

                _quests.Add(newQuest);
                return newQuest;
            }

            return listed.FirstOrDefault();
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

        internal IEnumerable<QuestKCInfo> GetKillCount(int Index)
        {
            return from q in _questMonsterKillCount
            where q.Quest == Index
            select q;
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

        internal byte Clear(SubQuest sq)
        {
            var session = Character.Player.Session;

            byte RewardArg = 0;

            switch (sq.CompensationType)
            {
                case QuestCompensation.Changeup:
                    Character.LevelUpPoints += sq.Amount;
                    Character.Changeup = true;
                    RewardArg = Character.ClientClass;

                    _ = session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
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
                    _ = session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
                    break;
                case QuestCompensation.AllStatsUp:
                    Character.Strength += sq.Amount;
                    Character.Agility += sq.Amount;
                    Character.Vitality += sq.Amount;
                    Character.Energy += sq.Amount;
                    if(Character.BaseClass == HeroClass.DarkLord) Character.Command += sq.Amount;
                    break;
                case QuestCompensation.Majestic:
                    Character.LevelUpPoints += sq.Amount;

                    if (!Character.MasterClass)
                    {
                        return 1;
                    }
                    Character.MajesticClass = true;
                    RewardArg = (byte)(Character.ClientClass|14);
                    _ = session.SendAsync(new SSendQuestPrize((ushort)session.ID, QuestCompensation.Statup, sq.Amount));
                    break;
            }

            _ = session.SendAsync(new SSendQuestPrize((ushort)session.ID, sq.CompensationType, RewardArg));

            return 0;
        }

        internal byte canClear(bool clear = true)
        {
            var inv = Character.Inventory;
            var list = new List<Item>();
            var total = 0;
            var mobFinish = true;
            foreach(var sq in Details.Sub.Where(x => x.Allowed.Contains(Character.Class)))
            {
                if (sq.Monster != 0)
                {
                    var kcInfo = Manager.GetKillCount(Index).FirstOrDefault(x => x.Monster == sq.Monster);
                    mobFinish &= sq.Count <= kcInfo.Count;
                }
                else
                {
                    foreach (var req in sq.Requeriment)
                    {
                        var Items = (from it in inv.FindAllItems(req.Number)
                                    where it.Plus == req.Plus
                                    select it)
                                    .Take(sq.Count);

                        list.AddRange(Items);
                        total += sq.Count;
                    }
                }
            }
            var result = mobFinish && total == list.Count;
            if(result && clear)
            {
                list.ForEach(x => _=inv.Delete((byte)x.SlotId));
                Details.Sub.Where(x => x.Allowed.Contains(Character.Class)).ToList().ForEach( x => Clear(x));
            }

            return (byte)(result?0:1);
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
                .Where(x => x.Quest == Index)
                .Select(x => x.Number.ToString()));

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

        internal IDictionary<ushort, byte> GetKillCount()
        {
            return Manager._questMonsterKillCount
                .Where(x => x.Quest == Index)
                .ToDictionary(x => x.Monster, y => y.Count);
        }
    }

    internal class Quest4thInfo
    {
        private byte state;
        private List<Monster> monsters = new List<Monster>();
        private DateTime end;
        private Monster centNPC;

        public Player Master { get; set; }
        public List<Player> Members => Master.Character.Party?.Members.ToList() ?? new List<Player> { Master };
        public byte State { get => state;
            set
            {
                state = value;
                onSetState();
            }
        }

        public Quest4thInfo()
        {
            centNPC = MonstersMng.Instance.CreateMonster(766, 
                ObjectType.NPC, 
                Maps.NewQuest,
                new System.Drawing.Point(147, 29),
                1);

            centNPC.Active = false;
            centNPC.Params = this;
        }

        internal IEnumerable<Monster> GetMonsters()
        {
            var copy = monsters.ToList();
            copy.Add(centNPC);
            return copy;
        }

        private void onSetState()
        {
            monsters.ForEach(x => x.Active = false);
            centNPC.Active = false;

            var masterQuest = Master.Character?.Quests.Find(766)??null;
            Log.Logger.Debug("Quest state changed to {0}, main quest index {1}", state, masterQuest?.Index??0);
            switch (state)
            {
                case 0: // Created State
                    break;
                case 10: // Talk End
                case 7: // Talk
                case 4: // Talk
                case 1: // Firts Talk
                    centNPC.Active = true;
                    break;
                case 8: // Set Reg
                case 5: // Set Reg
                case 2: // Set Reg
                    if (questPartyNextStep(766, new QuestState[] { QuestState.Clear, QuestState.Unreg }))
                        State++;
                    else
                        State--;
                    break;
                case 9: // Final Cent Battle
                    monsters.ForEach(x => x.Active = true);
                    break;
                case 6: // Summon Monsters
                case 3: // Cent Test Battle
                    var SubQuests = masterQuest.Details.Sub.Where(x => x.Allowed.Contains(Master.Character.Class)).ToList();
                    foreach (var sub in SubQuests)
                    {
                        for (var i = 0; i < sub.Count; i++)
                        {
                            monsters.Add(MonstersMng.Instance.CreateMonster(
                                sub.Monster,
                                ObjectType.Monster,
                                Maps.NewQuest,
                                new System.Drawing.Point(147, 29),
                                1));
                        }
                    }
                    monsters.ForEach(x => x.Die += X_Die);
                    if(state == 6)
                    {
                        _ = Members.SendAsync(new SQuestSurvivalTime
                        {
                            Increase = 0,
                            Time = 1 * 60 * 1000,
                            Type = QSType.QuestSurvivalTime,
                        });

                        end = DateTime.Now.AddMinutes(1);
                    }
                    break;
            }
            monsters.ForEach(x => x.Params = this);
        }

        private bool questPartyNextStep(ushort npc, QuestState[] states)
        {
            var masterQuest = Members.First().Character.Quests.Find(npc);
            bool result = true;
            foreach (var member in Members)
            {
                var q = member.Character.Quests.Find(npc);
                if (q.Index > masterQuest.Index || (q.Index == masterQuest.Index && q.State == QuestState.Complete))
                    continue;

                if (q.Index < masterQuest.Index)
                    return false;

                if (states.Contains(q.State))
                    result &= q.NextStep() == 0;
            }

            return result;
        }

        private void X_Die(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            var instance = mob.Params as Quest4thInfo;
            var quests = instance.Master.Character.Quests;
            var masterQuest = quests.Find(766);

            if (masterQuest.Index != 8)
            {
                instance.State++;
            }
        }

        public void Update()
        {
            if (state == 6 && end <= DateTime.Now)
            {
                var q = Master.Character.Quests.Find(766);
                if (q.canClear(false) == 0)
                {
                    State++;
                }
                else
                {
                    State = 4;
                }
            }
        }

        public void Dispose()
        {
            centNPC.Params = null;
            MonstersMng.Instance.DeleteMonster(centNPC);
            centNPC = null;
            monsters.ForEach(x => MonstersMng.Instance.DeleteMonster(x));
            monsters.Clear();
        }
    }
    internal static class Quest4th
    {
        private static Dictionary<Player, Quest4thInfo> _info = new Dictionary<Player, Quest4thInfo>();

        public static Quest4thInfo GetInfo(Player plr)
        {
            var master = plr.Character.Party?.Master ?? plr;

            if (!_info.ContainsKey(master))
            {
                _info.Add(master, new Quest4thInfo { Master = master });
                master.OnStatusChange += Master_OnStatusChange;
            }
            return _info[master];
        }

        private static void Master_OnStatusChange(object sender, EventArgs e)
        {
            RemoveInstance(sender as Player);
        }

        internal static void RemoveInstance(Player plr)
        {
            if (!_info.ContainsKey(plr))
                return;

            var instance = _info[plr];
            instance.State = 0;
            instance.Dispose();
            _info.Remove(plr);
        }

        internal static void Update()
        {
            _info.Values.ToList().ForEach(x => x.Update());
        }
    }
}
