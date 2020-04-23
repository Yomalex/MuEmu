using MuEmu.Monsters;
using MuEmu.Network.Event;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu.Events.BloodCastle
{
    public class BloodCastle
    {
        private readonly ILogger Logger;
        private readonly TimeSpan r_BCPlayTime = TimeSpan.FromSeconds(5*60);
        private readonly TimeSpan r_BCBeforePlayTime = TimeSpan.FromSeconds(60);
        private readonly TimeSpan r_BCBeforeCloseTime = TimeSpan.FromSeconds(30);

        private static List<int> s_BCRewardZenIn = new List<int>
        {
            20000,
            50000,
            100000,
            150000,
            200000,
            250000,
            250000,
            250000,
        };
        private static Item s_reward = new Item(new ItemNumber(13, 19));

        private MapInfo _map;
        private BloodCastles _manager;
        private BCState _state;
        private BCState _nextState;
        private List<Player> _playerForReward = new List<Player>();
        private Player _winner;

        internal TimeSpan _nextStateIn;

        public Maps MapID { get; set; }

        internal BCState State { get => _state;
            set
            {
                if(_state != value)
                {
                    if (_manager.State != value)
                        Logger.Information("State: {0} -> {1}", _state, value);
                    _state = value;

                    OnTransition(value);
                }
            }
        }

        public const int MaxPlayers = 10;
        public int Bridge { get; }
        public Player DoorWinner { get; set; }
        public Player StatueWinner { get; set; }
        public Player Winner { get => _winner; 
            set
            {
                _winner = value;
                _nextState = BCState.Ended;
                _nextStateIn = TimeSpan.Zero;
            }
        }

        public Player WeaponOwner { get; set; }
        public List<Player> Players { get; set; }
        public List<Monster> Monsters { get; set; }
        public Monster Gate { get; set; }
        public Monster Statue { get; set; }
        public Monster Archangel { get; set; }
        public ushort MonsterCount { get; set; }
        public ushort MonsterKill { get; set; }
        public ushort BossCount { get; set; }
        public ushort BossKill { get; set; }
        
        public BloodCastle(BloodCastles mng, int bridge)
        {
            Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BloodCastle) + " " + (bridge+1));

            _manager = mng;
            Bridge = bridge;
            Players = new List<Player>();
            Monsters = new List<Monster>();
            MapID = (bridge < 8 ? (Maps)((int)Maps.BloodCastle1 + bridge) : Maps.BloodCastle8);
            _map = ResourceCache.Instance.GetMaps()[MapID];
            _map.PlayerJoins += PlayerAdded;
            _map.PlayerLeaves += Remove;
            _map.MonsterAdd += AddMonster;
        }

        public bool TryAdd(Player plr)
        {
            if (_state != BCState.Open)
                return false;

            if (Players.Count >= MaxPlayers)
                return false;

            Players.Add(plr);
            if(Bridge < 6)
            {
                plr.Character.WarpTo(66 + Bridge);
            }else if(Bridge == 6)
            {
                plr.Character.WarpTo(80);
            }
            else if (Bridge == 7)
            {
                plr.Character.WarpTo(271);
            }else
            {
                Players.Remove(plr);
                return false;
            }

            return true;
        }

        private void Remove(object sender, EventArgs e)
        {
            var plr = sender as Player;
            Players.Remove(plr);
            if (WeaponOwner == plr)
            {
                WeaponOwner = null;
                _map.AddItem(plr.Character.Position.X, plr.Character.Position.Y, s_reward);
            }

            if (Players.Count == 0 && _state == BCState.Started)
                State = BCState.Close;
        }

        private void PlayerAdded(object sender, EventArgs e)
        {
            var plr = sender as Player;

            // Prevent login into BC
            if (!Players.Contains(plr))
                plr.Character.WarpTo(22);
        }

        private void AddMonster(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            mons.Active = false;

            switch (mons.Info.Monster)
            {
                // Spirit Sorcerer
                case 89:
                case 95:
                case 112:
                case 118:
                case 124:
                case 130:
                case 143:
                case 433:
                    mons.Die += SorcererDead;
                    break;
                // Castle Gate
                case 131:
                    mons.Die += GateDead;
                    Logger.Information("Gate Added");
                    Gate = mons;
                    mons.Active = true;
                    break;
                // Statue of Saint
                case 132:
                case 133:
                case 134:
                    mons.Die += StatueDead;
                    Logger.Information("Statue Added");
                    Statue = mons;
                    break;
                case 232:
                    Logger.Information("Archangel Added");
                    mons.Active = true;
                    Archangel = mons;
                    break;
                default:
                    mons.Die += MonsterDead;
                    Monsters.Add(mons);
                    break;
            }
        }

        private void MonsterDead(object sender, EventArgs e)
        {
            MonsterKill++;
            if (MonsterKill == MonsterCount)
                CastleBridge(true);
        }

        private void SorcererDead(object sender, EventArgs e)
        {
            BossKill++;
            //var mons = sender as Monster;
        }

        private void GateDead(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            DoorWinner = mons.Killer;
            mons.Active = false;
            CastleDoor(true);
        }

        private void StatueDead(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            StatueWinner = mons.Killer;
            mons.Active = false;
            _map.AddItem(mons.Position.X, mons.Position.Y, s_reward);
        }

        private void Clear()
        {
            foreach(var mons in Monsters)
            {
                mons.Active = false;
            }

            if(Gate!=null)
                Gate.Active = true;

            if (Statue != null)
                Statue.Active = true;

            if (Archangel != null)
                Archangel.Active = true;

            Players.Clear();
            _playerForReward.Clear();
            _winner = null;
            DoorWinner = null;
            StatueWinner = null;
        }

        public async Task Update()
        {
            switch(_state)
            {
                case BCState.Open:
                    break;
                case BCState.BeforeStart:
                    if (_nextStateIn == TimeSpan.FromSeconds(30))
                        await SendAsync(new SDevilSquareSet(DevilSquareState.BeforeStart));
                    break;
                case BCState.Started:
                    if(MonsterKill < MonsterCount)
                    {
                        await SendAsync(new SBloodCastleState(1, (ushort)_nextStateIn.TotalSeconds, MonsterCount, MonsterKill, 0xffff, 1));
                    }
                    else if(DoorWinner == null)
                    {
                        await SendAsync(new SBloodCastleState(1, (ushort)_nextStateIn.TotalSeconds, MonsterCount, MonsterKill, 0xffff, 1));
                    }
                    else if(BossKill < BossCount)
                    {
                        await SendAsync(new SBloodCastleState(4, (ushort)_nextStateIn.TotalSeconds, BossCount, BossKill, 0xffff, 1));
                    }else if(StatueWinner == null)
                    {
                        await SendAsync(new SBloodCastleState(4, (ushort)_nextStateIn.TotalSeconds, BossCount, BossKill, 0xffff, 1));
                    }
                    else
                    {
                        if(WeaponOwner == null)
                        {
                            WeaponOwner = Players.FirstOrDefault(x => x.Character.Inventory.FindAllItems(s_reward.Number).Any());
                        }

                        await SendAsync(new SBloodCastleState(4, (ushort)_nextStateIn.TotalSeconds, BossCount, BossKill, (byte)(WeaponOwner?.Session.ID??0xffff), 1));
                    }

                    if (_nextStateIn == TimeSpan.FromSeconds(30))
                        await SendAsync(new SDevilSquareSet(DevilSquareState.BeforeEnd));
                    break;
                case BCState.Ended:
                    if (_nextStateIn == TimeSpan.FromSeconds(30))
                        await SendAsync(new SDevilSquareSet(DevilSquareState.Quit));
                    break;
            }

            if (_nextStateIn <= TimeSpan.Zero)
                State = _nextState;
            else
                _nextStateIn -= TimeSpan.FromSeconds(1);
        }

        private async void OnTransition(BCState next)
        {
            switch(next)
            {
                case BCState.Open:
                    _nextState = BCState.BeforeStart;
                    _nextStateIn = TimeSpan.FromMinutes(10);
                    break;
                case BCState.BeforeStart:
                    await SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.GoAheadBC));
                    await SendAsync(new SNotice(NoticeType.Blue, "Blood castle will start in 60 seconds"));
                    _nextStateIn = r_BCBeforePlayTime;
                    _nextState = BCState.Started;
                    if (Players.Count == 0)
                    {
                        _nextState = BCState.Ended;
                        _nextStateIn = TimeSpan.Zero;
                        return;
                    }
                    break;
                case BCState.Started:
                    await SendAsync(new SNotice(NoticeType.Blue, $"Blood castle {Bridge+1} start!"));
                    _playerForReward = Players.ToList();
                    MonsterCount = (ushort)(Players.Count * 40);
                    BossCount = (ushort)(10 * Players.Count);
                    _nextStateIn = r_BCPlayTime;
                    _nextState = BCState.Ended;
                    foreach (var mons in Monsters)
                    {
                        mons.State = ObjectState.Regen;
                        mons.Active = true;
                    }
                    await SendAsync(new SBloodCastleState(0, (ushort)_nextStateIn.TotalSeconds, 0, 0, 0xffff, 1));
                    // Bridge Start
                    CastleEntrace(true);
                    break;
                case BCState.Ended:
                    GiveReward();
                    _nextStateIn = r_BCBeforeCloseTime;
                    _nextState = BCState.Close;
                    Clear();
                    break;
                case BCState.Close:
                    CastleEntrace(false);
                    CastleDoor(false);
                    CastleBridge(false);

                    foreach (var plr in Players.ToArray())
                        await plr.Character.WarpTo(22);

                    Players.Clear();
                    break;
            }
        }

        private async Task SendAsync(object message)
        {
            foreach(var plr in Players)
            {
                await plr.Session.SendAsync(message);
            }
        }

        private async void CastleEntrace(bool free)
        {
            if (free) await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(13, 15, 2, 8));
            else await _map.AddAttribute(MapAttributes.NoWalk, new Rectangle(13, 15, 2, 8));
        }

        private async void CastleBridge(bool free)
        {
            if (free) await _map.RemoveAttribute(MapAttributes.Hide, new Rectangle(13, 70, 2, 5));
            else await _map.AddAttribute(MapAttributes.Hide, new Rectangle(13, 70, 2, 5));
        }

        private async void CastleDoor(bool free)
        {
            if (free) await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(13, 70, 2, 5));
            else await _map.AddAttribute(MapAttributes.NoWalk, new Rectangle(13, 70, 2, 5));
        }

        private async void GiveReward()
        {
            List<BCScore> bc = new List<BCScore>();
            if (Winner != null)
            {
                var party = Winner.Character.Party;
                foreach (var plr in _playerForReward)
                {
                    int points;
                    if (plr == Winner)
                    {
                        points = 1000 + Bridge * 5;
                    }
                    else if (plr.Character.Party == party)
                    {
                        points = 800;
                    }
                    else if (Players.Any(x => x == plr))
                    {
                        points = 600;
                    }
                    else
                    {
                        points = 300;
                    }

                    float experience = 0;
                    if (plr == Winner || Winner.Character.Party == plr.Character.Party)
                    {
                        var mult = plr == Winner ? 1.0f : 0.5f;
                        experience += (5000 + Bridge * 5000) * mult;
                        experience += (float)(_nextStateIn.TotalSeconds * (160 + 20 * Bridge)) * mult;
                    }

                    if (plr == DoorWinner || DoorWinner.Character.Party == plr.Character.Party)
                    {
                        var mult = plr == DoorWinner ? 1.0f : 0.5f;
                        experience += (20000 + 30000 * Bridge) * mult;
                    }

                    if(plr == StatueWinner || StatueWinner.Character.Party == plr.Character.Party)
                    {
                        var mult = plr == StatueWinner ? 1.0f : 0.5f;
                        experience += (20000 + 30000 * Bridge) * mult;
                    }

                    var zMult = Players.Any(x => x == plr)?1.0f:0.5f;

                    if (plr.Status != LoginStatus.NotLogged)
                        await plr.Session.SendAsync(new SBloodCastleReward(false, 255, new BCScore[] { new BCScore {
                            Name = plr.Character.Name,
                            Experience = (int)experience,
                            Score = points,
                            Zen = (int)(s_BCRewardZenIn[Bridge] * zMult),
                        } }));

                    /*bc.Add(new BCScore
                    {
                        Name = plr.Character.Name,
                        Experience = (int)experience,
                        Score = points,
                        Zen = (int)(s_BCRewardZenIn[Bridge] * zMult),
                    });*/
                }
            }
            else
            {
                foreach (var plr in _playerForReward)
                {
                    float experience = 0;

                    if(DoorWinner != null)
                        if (plr == DoorWinner || DoorWinner.Character.Party == plr.Character.Party)
                        {
                            var mult = plr == DoorWinner ? 1.0f : 0.5f;
                            experience += (20000 + 30000 * Bridge) * mult;
                        }

                    if (StatueWinner != null)
                        if (plr == StatueWinner || StatueWinner.Character.Party == plr.Character.Party)
                        {
                            var mult = plr == StatueWinner ? 1.0f : 0.5f;
                            experience += (20000 + 30000 * Bridge) * mult;
                        }

                    if (plr.Status != LoginStatus.NotLogged)
                        await plr.Session.SendAsync(new SBloodCastleReward(false, 255, new BCScore[] { new BCScore {
                        Name = plr.Character.Name,
                            Experience = (int)experience,
                            Score = -300,
                            Zen = 0,
                        } }));

                    /*bc.Add(new BCScore
                    {
                        Name = plr.Character.Name,
                        Experience = (int)experience,
                        Score = -300,
                        Zen = 0,
                    });*/
                }
            }
            /*var msg = new SBloodCastleReward((Winner != null), (byte)bc.Count, bc.ToArray());
            _playerForReward.ForEach(x => x.Session.SendAsync(bc).Wait());*/

            _playerForReward.Clear();
        }
    }
}
