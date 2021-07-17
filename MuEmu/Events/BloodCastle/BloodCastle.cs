using MuEmu.Monsters;
using MU.Network.Event;
using MU.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MU.Resources;
using MuEmu.Network;
using MuEmu.Resources.Game;
using MuEmu.Entity;

namespace MuEmu.Events.BloodCastle
{
    public class BloodCastle : Event
    {
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
        private Item _reward = new Item(new ItemNumber(13, 19));

        private MapInfo _map;
        private BloodCastles _manager;
        private List<Player> _playerForReward = new List<Player>();
        private Player _winner;
        private bool _ended;

        public Maps MapID { get; set; }

        public const int MaxPlayers = 10;
        public int Bridge { get; }
        public Player DoorWinner { get; set; }
        public Player StatueWinner { get; set; }
        public Player Winner { get => _winner; 
            set
            {
                _winner = value;
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
        
        public BloodCastle(BloodCastles mng, int bridge, TimeSpan closed, TimeSpan open, TimeSpan playing)
            :base(closed, open, playing)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BloodCastle) + " " + (bridge+1));

            _manager = mng;
            Bridge = bridge;
            Players = new List<Player>();
            Monsters = new List<Monster>();
            MapID = (bridge < 7 ? (Maps)((int)Maps.BloodCastle1 + bridge) : Maps.BloodCastle8);
            _map = ResourceCache.Instance.GetMaps()[MapID];
            _map.PlayerJoins += PlayerAdded;
            _map.PlayerLeaves += OnPlayerLeave;
            _map.MonsterAdd += AddMonster;
        }

        public override bool TryAdd(Player plr)
        {
            if (CurrentState != EventState.Open)
                return false;

            if (Players.Count >= MaxPlayers)
                return false;

            Players.Add(plr);
            if(Bridge < 6)
            {
                plr.Character.WarpTo(66 + Bridge).Wait();
            }else if(Bridge == 6)
            {
                plr.Character.WarpTo(80).Wait();
            }
            else if (Bridge == 7)
            {
                plr.Character.WarpTo(271).Wait();
            }else
            {
                Players.Remove(plr);
                return false;
            }

            plr.Character.CharacterDie += OnPlayerDead;
            //plr.Character.MapChanged += OnPlayerLeave;

            return true;
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            var plr = (sender as Player);
            Players.Remove(plr);
            if (WeaponOwner == plr)
            {
                try
                {
                    var a = plr.Character.Inventory.FindAll(_reward.Number).First();
                    GameServices.CItemThrow(plr.Session, new CItemThrow
                    {
                        MapX = (byte)plr.Character.Position.X,
                        MapY = (byte)plr.Character.Position.Y,
                        Source = a,
                    }).Wait();
                }catch(Exception)
                { }
                WeaponOwner = null;
                //_map.AddItem(, , _reward);
            }

            if (Players.Count == 0 && CurrentState == EventState.Playing)
                Trigger(EventState.Closed);
        }

        private void PlayerAdded(object sender, EventArgs e)
        {
            var plr = sender as Player;

            // Prevent login into BC
            if (!Players.Contains(plr))
                plr.Character.WarpTo(22).Wait();
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
                    Monsters.Add(mons);
                    break;
                // Castle Gate
                case 131:
                    mons.Die += GateDead;
                    Gate = mons;
                    mons.Active = true;
                    break;
                // Statue of Saint
                case 132:
                case 133:
                case 134:
                    mons.Die += StatueDead;
                    Statue = mons;
                    break;
                case 232:
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
            if (MonsterKill < MonsterCount)
            {
                MonsterKill++;
                if (MonsterKill == MonsterCount)
                {
                    CastleBridge(true);
                    Program.MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_MonstersKilled));
                }
            }
        }

        private void SorcererDead(object sender, EventArgs e)
        {
            if (BossKill < BossCount)
            {
                BossKill++;
                if (BossKill == BossCount)
                {
                    Statue.Active = true;
                    Program.MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_BossKilled)).Wait();
                }
            }
        }

        private void GateDead(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            DoorWinner = mons.Killer;
            mons.Active = false;
            Program
                .MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_DoorKiller, DoorWinner.Character.Name))
                .Wait();
            CastleDoor(true);
        }

        private void StatueDead(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            StatueWinner = mons.Killer;
            mons.Active = false;
            _reward.Plus = (byte)(mons.Info.Monster - 132);
            _map.AddItem(mons.Position.X, mons.Position.Y, _reward.Clone() as Item, mons.Killer.Character);
            Program
                .MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_StatueKiller, StatueWinner.Character.Name))
                .Wait();
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
                Statue.Active = false;

            if (Archangel != null)
                Archangel.Active = true;

            if (WeaponOwner != null)
            {
                var a = WeaponOwner.Character.Inventory.FindAll(_reward.Number).First();
                WeaponOwner.Character.Inventory.Delete(a);
                WeaponOwner = null;
            }

            _playerForReward.Clear();
            _winner = null;
            DoorWinner = null;
            StatueWinner = null;
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Open:
                    if (TimeLeft.Seconds != 0)
                        break;
                    Program.MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_Open2, TimeLeft.Minutes));
                    break;
                case EventState.Playing:
                    if ((int)Time.TotalSeconds == 30)
                        SendAsync(new SDevilSquareSet(DevilSquareState.BeforeStart)).Wait();

                    if ((int)Time.TotalSeconds < 60)
                        return;

                    if ((int)Time.TotalSeconds == 60)
                    {
                        SendAsync(new SBloodCastleState(0, (ushort)(TimeLeft.TotalSeconds - 60), 0, 0, 0xffff, 1)).Wait();
                        foreach (var mons in Monsters)
                        {
                            mons.State = ObjectState.Regen;
                            mons.Active = true;
                        }
                        // Bridge Start
                        CastleEntrace(true);
                    }

                    if (TimeLeft.TotalSeconds > 60 && !_ended)
                    {
                        var timeleft = (ushort)(TimeLeft.TotalSeconds - 60);

                        var msg = new SBloodCastleState(4, timeleft, MonsterCount, MonsterKill, (ushort)(WeaponOwner?.Session.ID ?? 0xffff), _reward.Plus);

                        if (MonsterKill < MonsterCount)
                        {
                            msg.State = 1;
                        }
                        else if(DoorWinner == null)
                        {
                            msg.State = 2;
                        }
                        else if (BossKill < BossCount || StatueWinner == null)
                        {
                            msg.State = 4;
                            msg.MaxKillMonster = BossCount;
                            msg.CurKillMonster = BossKill;
                        }
                        else
                        {
                            msg.State = 4;
                            msg.MaxKillMonster = BossCount;
                            msg.CurKillMonster = BossKill;
                            if (WeaponOwner == null)
                            {
                                WeaponOwner = Players
                                    .FirstOrDefault(x => x.Character.Inventory.FindAllItems(_reward.Number).Any());

                                if (WeaponOwner != null)
                                    Program.MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_WeaponOwner, WeaponOwner.Character.Name));
                            }
                        }

                        SendAsync(msg).Wait();

                        if(timeleft == 30)
                            SendAsync(new SDevilSquareSet(DevilSquareState.BeforeEnd)).Wait();
                    }

                    if((int)TimeLeft.TotalSeconds == 60 || Winner != null)
                    {
                        GiveReward();
                        _ended = true;
                        SendAsync(new SBloodCastleState(2, 59, 0, 0, 0xffff, 1)).Wait();

                        if(Winner != null)
                            Program.MapAnoucement(MapID, ServerMessages.GetMessage(Messages.BC_Winner, Winner.Character.Name)).Wait();
                        Winner = null;
                        Trigger(EventState.Closed, TimeSpan.FromSeconds(59));
                        Clear();
                    }

                    if (TimeLeft == TimeSpan.FromSeconds(30))
                        SendAsync(new SDevilSquareSet(DevilSquareState.Quit)).Wait();
                    break;
            }
            base.Update();
        }

        public override void OnTransition(EventState NextState)
        {
            _logger.Information("State:{0}->{1}", CurrentState, NextState);
            switch (NextState)
            {
                case EventState.Closed:
                    CastleEntrace(false);
                    CastleDoor(false);
                    CastleBridge(false);

                    foreach (var plr in Players.ToArray())
                        plr.Character.WarpTo(22).Wait();

                    Players.Clear();
                    if(_manager.CurrentState != EventState.Closed)
                        Trigger(EventState.Open, _manager.TimeLeft + _closedTime);
                    break;
                case EventState.Open:
                    Trigger(EventState.Playing, _openTime);
                    break;
                case EventState.Playing:
                    SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.GoAheadBC)).Wait();
                    _playerForReward = Players.ToList();
                    MonsterCount = (ushort)(Players.Count * 40);
                    BossCount = (ushort)(10 * Players.Count);
                    Trigger(EventState.Closed, _playingTime);
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
            if (free)
            {
                await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(13, 75, 2, 5));// Door Itself
                await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(11, 80, 14, 9));// Zone Beginh Door
                await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(8, 80, 2, 3));// Altar 

            }
            else
            {
                await _map.AddAttribute(MapAttributes.NoWalk, new Rectangle(13, 75, 2, 5));
                await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(11, 80, 14, 9));// Zone Beginh Door
                await _map.RemoveAttribute(MapAttributes.NoWalk, new Rectangle(8, 80, 2, 3));// Altar 
            }
        }

        private async void GiveReward()
        {
            List<BCScore> bc = new List<BCScore>();
            using (var db = new GameContext())
            {
            if (Winner != null)
                {
                    var party = Winner.Character.Party;
                    foreach (var plr in _playerForReward)
                    {
                        int points;
                        if (plr == Winner)
                        {
                            points = 1000 + Bridge * 5;
                            // Jewel of Chaos
                            _map.AddItem(plr.Character.Position.X, plr.Character.Position.Y, new Item(new ItemNumber(6159)), plr.Character);
                        }
                        else if (plr.Character.Party == party)
                        {
                            points = 800;
                            // Jewel of Chaos
                            _map.AddItem(plr.Character.Position.X, plr.Character.Position.Y, new Item(new ItemNumber(6159)), plr.Character);
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
                            experience += (float)(TimeLeft.TotalSeconds * (160 + 20 * Bridge)) * mult;
                        }

                        if (plr == DoorWinner || DoorWinner.Character.Party == plr.Character.Party)
                        {
                            var mult = plr == DoorWinner ? 1.0f : 0.5f;
                            experience += (20000 + 30000 * Bridge) * mult;
                        }

                        if (plr == StatueWinner || StatueWinner.Character.Party == plr.Character.Party)
                        {
                            var mult = plr == StatueWinner ? 1.0f : 0.5f;
                            experience += (20000 + 30000 * Bridge) * mult;
                        }

                        var zMult = Players.Any(x => x == plr) ? 1.0f : 0.5f;

                        plr.Character.Experience += (uint)experience;
                        plr.Character.Money += (uint)(s_BCRewardZenIn[Bridge] * zMult);

                        if (plr.Status != LoginStatus.NotLogged)
                            await plr.Session.SendAsync(new SBloodCastleReward(true, 255, new BCScore[] { new BCScore {
                            Name = plr.Character.Name,
                            Experience = (int)experience,
                            Score = points,
                            Zen = (int)(s_BCRewardZenIn[Bridge] * zMult),
                        } }));
                        var _dto = (from c in db.BloodCastles
                                where c.CharacterId == plr.Character.Id
                                select c).SingleOrDefault();

                        if (_dto == null)
                        {
                            _dto = new MU.DataBase.BloodCastleDto
                            {
                                CharacterId = plr.Character.Id,
                                Points = points
                            };
                            db.BloodCastles.Add(_dto);
                        }
                        else
                        {
                            _dto.Points += points;
                            db.BloodCastles.Update(_dto);
                        }
                    }
                }
                else
                {
                    foreach (var plr in _playerForReward)
                    {
                        float experience = 0;

                        if (DoorWinner != null)
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
                        var _dto = (from c in db.BloodCastles
                                    where c.CharacterId == plr.Character.Id
                                    select c).SingleOrDefault();

                        if (_dto == null)
                        {
                            _dto = new MU.DataBase.BloodCastleDto
                            {
                                CharacterId = plr.Character.Id,
                                Points = -300
                            };
                            db.BloodCastles.Add(_dto);
                        }
                        else
                        {
                            _dto.Points += -300;
                            db.BloodCastles.Update(_dto);
                        }
                    }
                }
            }

            _playerForReward.Clear();
        }

        public override void NPCTalk(Player plr)
        {
            var session = plr.Session;
            if (CurrentState != EventState.Playing || (CurrentState == EventState.Playing && TimeLeft > TimeSpan.FromMinutes(4)))
            {
                session.SendAsync(new SNotice(NoticeType.Blue, ServerMessages.GetMessage(Messages.BC_Time))).Wait();
                return;
            }

            if ((int)TimeLeft.TotalSeconds == 60 || Winner != null)
            {
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.CompletedBC)).Wait();
                return;
            }

            var item = plr.Character.Inventory.FindAllItems(_reward.Number).FirstOrDefault();

            if (item == null)
            {
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC)).Wait();
                return;
            }

            plr.Character.Inventory.Delete(item).Wait();

            if (DoorWinner == null || StatueWinner == null)
            {
                _logger.Error(ServerMessages.GetMessage(Messages.BC_WeaponError));
                return;
            }
            session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.SucceedBC)).Wait();
            Winner = plr;
            WeaponOwner = null;
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var @char = sender as Character;
            var item = @char.Inventory.FindAll(_reward.Number);
            if(item.Length > 0)
            {
                GameServices.CItemThrow(@char.Player.Session, new CItemThrow
                {
                    MapX = (byte)@char.Position.X,
                    MapY = (byte)@char.Position.Y,
                    Source = item.First()
                }).Wait();
            }
            @char.WarpTo(22).Wait();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
