using MuEmu.Monsters;
using MuEmu.Network.Event;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu.Events.DevilSquare
{
    public class DevilSquare : Event
    {
        private ushort _gate;
        private List<List<Monster>> _ground = new List<List<Monster>>();
        private DevilSquares _manager;
        public int Index { get; private set; }

        public DevilSquare(DevilSquares dsManager, int DSindex, TimeSpan close, TimeSpan open, TimeSpan playing, ushort gate) : base(close, open, playing)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(DevilSquare) +" "+ DSindex);
            Index = DSindex;
            _gate = gate;
            _manager = dsManager;
            var ground = dsManager._dsGround[DSindex - 1];
            var rand = new Random();

            for (var i = 0; i < 7; i++)
            {
                var type = dsManager._mobGround[Index - 1, i];
                var subList = new List<Monster>();
                for (var n = 0; n < 40; n++)
                {
                    var x = rand.Next(ground.Left, ground.Right);
                    var y = rand.Next(ground.Top, ground.Bottom);
                    var mob = new Monster(type, ObjectType.Monster, Index <= 4 ? Maps.DevilSquare : Maps.DevilSquare2, new Point(x, y), (byte)rand.Next(7))
                    { Index = MonstersMng.Instance.GetNewIndex() };
                    mob.Die += OnMonsterDead;
                    subList.Add(mob);
                    mob.Active = false;

                    MonstersMng.Instance.Monsters.Add(mob);
                }
                _ground.Add(subList);
            }
        }
        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            var score = mob.DamageSum[mob.Killer] / mob.MaxLife * mob.Level;
            score *= Index;
            var pInfo = _players.Find(x => x.Player == mob.Killer);
            pInfo.Score += (int)score;

            //_logger.Debug("Monster Dead: {0} {1} {2}", mob.Info.Name, mob.Info.Level, score);
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var @char = sender as Character;
            var plr = @char.Player;

            var eventInfo = _players.Find(x => x.Player == plr);
            eventInfo.Eventer = false;

            @char.WarpTo(27).Wait();

            if (!_players.Any(x => x.Eventer))
                Trigger(EventState.Closed);
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            var @char = sender as Character;
            var plr = @char.Player;

            var eventInfo = _players.Find(x => x.Player == plr);
            eventInfo.Eventer = false;

            if (!_players.Any(x => x.Eventer))
                Trigger(EventState.Closed);
        }

        public override void OnTransition(EventState NextState)
        {
            _logger.Information("State:{0} -> {1}", CurrentState, NextState);
            switch (NextState)
            {
                case EventState.Closed:
                    foreach(var list in _ground)
                    {
                        foreach(var monster in list)
                        {
                            monster.Active = false;
                        }
                    }
                    var ranking = _players
                        .OrderByDescending(x => x.Score)
                        .Select((x,i) => new DevilSquareScoreInfo
                        {
                            rank = (byte)(i+1),
                            player = x.Player,
                            Name = x.Player.Character.Name,
                            TotalScore = x.Score,
                            BonusExp = 0,
                            BonusZen = 0,
                        }).ToList();

                    foreach(var r in ranking)
                    {
                        var a = r.rank * 100.0f / _players.Count;
                        if(r.BonusExp == 1)
                        {
                            r.BonusExp = _manager._rewards[Index - 1][0].EXP;
                            r.BonusZen = _manager._rewards[Index - 1][0].Zen;
                        }else if(a < 0.3f)
                        {
                            r.BonusExp = _manager._rewards[Index - 1][1].EXP;
                            r.BonusZen = _manager._rewards[Index - 1][1].Zen;
                        }
                        else if (a < 0.5f)
                        {
                            r.BonusExp = _manager._rewards[Index - 1][2].EXP;
                            r.BonusZen = _manager._rewards[Index - 1][2].Zen;
                        }
                        else if (a < 0.5f)
                        {
                            r.BonusExp = _manager._rewards[Index - 1][3].EXP;
                            r.BonusZen = _manager._rewards[Index - 1][3].Zen;
                        }
                    }

                    foreach(var p in ranking)
                    {
                        var score = new SDevilSquareResult
                        {
                            MyRank = p.rank,
                            Score = ranking.ToArray()
                        };
                        (p.player as Player).Session.SendAsync(score).Wait();
                    }

                    _players.Clear();
                    break;
                case EventState.Open:
                    Trigger(EventState.Playing, _openTime);
                    break;
                case EventState.Playing:
                    if (_players.Any(x => x.Eventer))
                    {
                        SpawList(0);
                        Trigger(EventState.Closed, _playingTime);
                    }else
                    {
                        Trigger(EventState.Closed);
                    }
                    break;
            }
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Playing:
                    break;
            }
            base.Update();
        }

        internal void SpawList(int number)
        {
            foreach (var monster in _ground[number])
            {
                monster.State = ObjectState.WaitRegen;
                monster.Active = true;
            }
        }

        public override bool TryAdd(Player plr)
        {
            if (_players.Count < DevilSquares.MaxPlayers)
            {
                _players.Add(new PlayerEventInfo { Eventer = true, Player = plr, Score = 0 });
                plr.Character.WarpTo(_gate).Wait();
                plr.Character.CharacterDie += OnPlayerDead;
                plr.Character.MapChanged += OnPlayerLeave;

                return true;
            }

            return false;
        }
    }
}
