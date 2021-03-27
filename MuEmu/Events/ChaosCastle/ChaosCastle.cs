using MuEmu.Monsters;
using MU.Network.Event;
using MU.Network.Game;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using WebZen.Util;
using MU.Resources;

namespace MuEmu.Events.ChaosCastle
{
    public class ChaosCastle : Event
    {
        private ushort _monsterA = 162;
        private ushort _monsterB = 163;
        private List<Monster> _ccMonsters;
        private Maps _map;
        private MapInfo _mapInfo;
        private Rectangle _floorState;
        private int _recaudation;
        private byte _trapStatus;

        public int Index { get; private set; }
        public ChaosCastle(int index, TimeSpan close, TimeSpan open, TimeSpan playing) : base(close, open, playing)
        {
            Index = index + 1;
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ChaosCastle) + " " + Index);
            _monsterA += (ushort)index;
            _monsterB += (ushort)index;
            _map = (Maps)((int)Maps.ChaosCastle1 + index);
            if (Index == 7)
            {
                _monsterA = 426;
                _monsterB = 427;
                _map = Maps.ChaosCastle7;
            }
            _mapInfo = Resources.ResourceCache.Instance.GetMaps()[_map];
            _ccMonsters = _mapInfo.Monsters;
            _floorState = ChaosCastles.Ground;
            _mapInfo.MonsterAdd += OnMonsterAdd;
        }

        public void OnMonsterAdd(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Die += OnMonsterDead;
            mob.Active = false;
            mob.CanDrop = false;
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
            
            var plrInfo = _players.Find(x => x.Player == mob.Killer);
            if (plrInfo == null)
                return;

            plrInfo.Score += 100;

            var playersInBlowArea = _mapInfo.Players.Where(x => x.Position.Substract(mob.Position).Length() <= 2);
            foreach (var plr in playersInBlowArea)
            {
                var distance = mob.Position.Substract(plr.Position);
                var ls = Math.Max(distance.LengthSquared(), 0);
                var norma = distance.Normalize();
                var direction = new Point((int)(norma.X * 3 / ls), (int)(norma.Y * 3 / ls));
                plr.Position = new Point(plr.Position.X + direction.X, plr.Position.Y + direction.Y);
                var msg = new SPositionSet((ushort)plr.Player.Session.ID, plr.Position);
                var dmg = 15 / ls;
                plr.Health -= (float)dmg;
                plr.Player.Session.SendAsync(msg).Wait();
                switch (Program.Season)
                {
                    case 9:
                        plr.Player.Session.SendAsync(new SAttackResultS9((ushort)plr.Player.Session.ID, (ushort)dmg, DamageType.Regular, 0)).Wait();
                        break;
                    default:
                        plr.Player.Session.SendAsync(new SAttackResult((ushort)plr.Player.Session.ID, (ushort)dmg, DamageType.Regular, 0)).Wait();
                        break;                
                }
                plr.Player.SendV2Message(msg).Wait();
            }

            UpdateFloor();

            if (EventCount() == 1)
                GiveRewards();
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var plr = (sender as Character)?.Player ?? null;
            plr.Character
                .WarpTo(22)
                .Wait();

            //var plrInfo = _players.Find(x => x.Player == plr);

            //if (plrInfo != null)
            //{
            //    plrInfo.Eventer = false;
            //    if(plr.Killer.GetType() == typeof(Player))
            //    {
            //        var killer = plr.Killer as Player;
            //        plrInfo = _players.Find(x => x.Player == killer);
            //        if(plrInfo != null)
            //        {
            //            plrInfo.Score += 1;
            //        }
            //    }
            //}

            //UpdateFloor();

            //if (EventCount() == 1)
            //    GiveRewards();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            var plr = (sender as Character)?.Player??null;

            var plrInfo = _players.Find(x => x.Player == plr);

            if (plrInfo == null)
            {
                _logger.Error("Player isn't in the event");
                plr.Character.CharacterDie -= OnPlayerDead;
                plr.Character.MapChanged -= OnPlayerLeave;
            }
            else
            {
                plr.Character.CharacterDie -= OnPlayerDead;
                plr.Character.MapChanged -= OnPlayerLeave;
                plrInfo.Eventer = false;
            }

            UpdateFloor();

            if (EventCount() == 1)
                GiveRewards();
        }

        public override void OnTransition(EventState NextState)
        {
            _logger.Information("State:{0}->{1}", CurrentState, NextState);
            switch (NextState)
            {
                case EventState.Open:
                    _recaudation = 0;
                    _players.Clear();
                    Trigger(EventState.Playing, _openTime);
                    _floorState = ChaosCastles.Ground;
                    break;
                case EventState.Playing:
                    Program
                        .MapAnoucement(_map, $"Chaos Castle {Index} Starts!.")
                        .Wait();

                    _mapInfo.Push();
                    SendSatus(5);

                    var fill = Math.Min(50 - _players.Count(x => x.Eventer), _ccMonsters.Count);
                    for(var i = 0; i < fill; i++)
                        _ccMonsters[i].Active = true;

                    Trigger(EventState.Closed, _playingTime);

                    _mapInfo.RemoveAttribute(MapAttributes.Safe, ChaosCastles.Ground)
                        .Wait();
                    break;
                case EventState.Closed:
                    _mapInfo.Push();
                    SendSatus(7);
                    for (var i = 0; i < _ccMonsters.Count; i++)
                        _ccMonsters[i].Active = false;
                    break;
            }
        }

        public override void Update()
        {
            switch (CurrentState)
            {
                case EventState.Closed:
                    if((int)TimeLeft.TotalSeconds == 30)
                    {
                        Program.NoEventMapSendAsync(new SDevilSquareSet(DevilSquareState.CCBeforeEnter)).Wait();
                    }
                    break;
                case EventState.Open:
                    if (((int)TimeLeft.TotalSeconds) % 60 == 0 && (int)TimeLeft.TotalSeconds > 0)
                    {
                        Program
                            .MapAnoucement(_map, $"Chaos Castle {Index} Will start in {(int)TimeLeft.TotalMinutes} minute(s).")
                            .Wait();
                    }
                    if ((int)TimeLeft.TotalSeconds == 30)
                    {
                        _mapInfo.SendAsync(new SDevilSquareSet(DevilSquareState.CCBeforePlay)).Wait();
                    }
                    _trapStatus = 6;
                    break;
                case EventState.Playing:
                    if (!CanRun())
                    {
                        GiveRewards();
                        Trigger(EventState.Closed);
                        break;
                    }

                    if ((int)TimeLeft.TotalSeconds == 60)
                    {
                        GiveRewards();
                        break;
                    }

                    if ((int)TimeLeft.TotalSeconds == 30)
                    {
                        _mapInfo.SendAsync(new SDevilSquareSet(DevilSquareState.CCBeforeEnd)).Wait();
                    }

                    if (TimeLeft > TimeSpan.FromMinutes(1))
                        SendSatus(6);
                    break;
            }

            base.Update();
        }

        public bool CanRun()
        {
            return _players.Any(x => x.Eventer);
        }

        public override bool TryAdd(Player plr)
        {
            if (_players.Count >= 70)
                return false;

            if (plr.Character.Money < ChaosCastles.EntryFee[Index - 1])
                return false;

            plr.Character.Money -= (uint)ChaosCastles.EntryFee[Index - 1];
            _recaudation += ChaosCastles.EntryFee[Index - 1];

            _players.Add(new PlayerEventInfo
            {
                Eventer = true,
                Player = plr,
                Score = 0,
            });

            plr.Character
                .WarpTo(Index==7?272:81+Index)
                .Wait();
            plr.Character.CharacterDie += OnPlayerDead;
            plr.Character.MapChanged += OnPlayerLeave;

            return true;
        }

        private int EventCount()
        {
            return _ccMonsters.Count(x => x.Active) + _players.Count(x => x.Eventer);
        }

        private void SendSatus(byte status)
        {
            _players.Where(x => x.Eventer)
                .Select(x => x.Player.Session)
                .SendAsync(new SBloodCastleState(status, (ushort)(TimeLeft.TotalSeconds-60), 100, (ushort)EventCount(), 255, 255))
                .Wait();
        }

        private void UpdateFloor()
        {
            bool changeFloor = false;
            switch (EventCount())
            {
                case 40:
                    _floorState = ChaosCastles.Ground;
                    break;
                case 30:
                    _floorState.Inflate(new Size(4, 4));
                    changeFloor = true;
                    _trapStatus = 8;
                    break;
                case 20:
                    _floorState.Inflate(new Size(4, 4));
                    changeFloor = true;
                    _trapStatus = 9;
                    break;
                case 10:
                    _floorState.Inflate(new Size(4, 4));
                    changeFloor = true;
                    _trapStatus = 10;
                    break;
            }

            if (changeFloor)
            {
                var ground = ChaosCastles.Ground;
                var up = new Rectangle(ground.X, ground.Y, ground.Width, _floorState.Y - ground.Y);
                var down = new Rectangle(ground.X, ground.Y+up.Height+_floorState.Height, ground.Width, _floorState.Y - ground.Y);
                var left = new Rectangle(ground.X, ground.Y, _floorState.X - ground.X, ground.Height);
                var right = new Rectangle(ground.X+left.Width+_floorState.Width, ground.Y, _floorState.X - ground.X, ground.Height);

                _mapInfo.AddAttribute(MapAttributes.Hide, up).Wait();
                _mapInfo.AddAttribute(MapAttributes.Hide, down).Wait();
                _mapInfo.AddAttribute(MapAttributes.Hide, left).Wait();
                _mapInfo.AddAttribute(MapAttributes.Hide, right).Wait();
                SendSatus(_trapStatus);

                _players
                    .Where(x => x.Eventer)
                    .Where(x => !_floorState.Contains(x.Player.Character.Position))
                    .ToList()
                    .ForEach(x => x.Player.Character.Health = 0.0f);
            }
        }

        private void GiveRewards()
        {
            var playersIn = _players
                            .Where(x => x.Eventer);

            PlayerEventInfo winner = null;

            if (playersIn.Count() != 0)
            {
                winner = playersIn.OrderByDescending(x => x.Score/100*2+x.Score%100).First();
                var wXP = ChaosCastles.MonsterKillExp[Index - 1] * (winner.Score / 100);
                wXP += ChaosCastles.PlayerKillExp[Index - 1] * (winner.Score % 100);
                winner.Player.Session.SendAsync(new SBloodCastleReward()
                {
                    Winner = 1,
                    Type = 0xFE,
                    ScoreTable = new BCScore[] {
                                    new BCScore {
                                     Name = winner.Player.Character.Name,
                                     Score = winner.Score/100,
                                     Experience = wXP,
                                     Zen = winner.Score%100,
                                    }
                                }
                })
                    .Wait();

                winner.Player.Character.Experience += (ulong)wXP;
                winner.Player.Session.SendAsync(new SKillPlayer(0xffff, (ushort)wXP, 0)).Wait();
                winner.Player.Character.Money += (uint)(_recaudation * 0.8f);
            }

            var losers = _players.Where(x => x != winner);
            foreach (var l in losers)
            {
                if (l.Eventer)
                {
                    l.Player.Character
                        .WarpTo(22)
                        .Wait();
                }

                var XP = ChaosCastles.MonsterKillExp[Index - 1] * (winner.Score / 100);
                XP += ChaosCastles.PlayerKillExp[Index - 1] * (winner.Score % 100);

                l.Player.Session.SendAsync(new SBloodCastleReward()
                {
                    Winner = 0,
                    Type = 0xFE,
                    ScoreTable = new BCScore[] {
                                    new BCScore {
                                     Name = winner.Player.Character.Name,
                                     Score = winner.Score/100,
                                     Experience = XP,
                                     Zen = winner.Score%100,
                                    }
                                }
                })
                .Wait();

                l.Player.Character.Experience += (ulong)XP;
            }

            foreach (var mob in _ccMonsters)
                mob.Active = false;

            SendSatus(7);
            
            _recaudation = 0;
            Trigger(EventState.Closed, TimeSpan.FromMinutes(1));
        }
    }
}
