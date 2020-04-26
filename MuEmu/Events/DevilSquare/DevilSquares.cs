using MuEmu.Monsters;
using MuEmu.Network.Event;
using MuEmu.Network.Game;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu.Events.DevilSquare
{
    public class DevilSquares : Event
    {
        private struct DevilSquareLevels
        {
            public int Normal;
            public int Special;
        }

        internal struct DevilSquareRewards
        {
            public int EXP { get; set; }
            public int Zen { get; set; }
        }

        private readonly ItemNumber r_devilInvitation = new ItemNumber(14, 19);
        private readonly DevilSquareLevels[] r_levels = new DevilSquareLevels[]
        {
            new DevilSquareLevels{ Normal = 15, Special = 10 },
            new DevilSquareLevels{ Normal = 131, Special = 111 },
            new DevilSquareLevels{ Normal = 181, Special = 161 },
            new DevilSquareLevels{ Normal = 231, Special = 211 },
            new DevilSquareLevels{ Normal = 281, Special = 261 },
            new DevilSquareLevels{ Normal = 331, Special = 311 },
            new DevilSquareLevels{ Normal = 401, Special = 401 }
        };
        internal readonly Rectangle[] _dsGround = new Rectangle[]
        {
            new Rectangle(119, 80, 31, 35),
            new Rectangle(121, 152, 31, 35),
            new Rectangle(49, 138, 31, 35),
            new Rectangle(53, 74, 31, 35),
            new Rectangle(120, 80, 31, 35),
            new Rectangle(122, 152, 31, 35),
            new Rectangle(49, 138, 31, 35),
        };
        internal readonly ushort[,] _mobGround = new ushort[7,7]
        {
            { 17, 15, 5, 13, 8, 36, 18 },
            { 10, 39, 34, 41, 40, 35, 49 },
            { 41, 37, 35, 180, 64, 65, 67 },
            { 64, 65, 60, 294, 57, 70, 66 },
            { 60, 294, 71, 144, 61, 73, 291 },
            { 290, 57, 70, 293, 74, 292, 275 },
            { 356, 510, 409, 411, 358, 360, 340 },
        };
        internal readonly List<DevilSquareRewards[]> _rewards = new List<DevilSquareRewards[]>
        {
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP = 6000, Zen =  30000 },
                new DevilSquareRewards{ EXP = 4000, Zen =  25000 },
                new DevilSquareRewards{ EXP = 2000, Zen =  20000 },
                new DevilSquareRewards{ EXP = 1000, Zen =  15000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP = 8000, Zen =  40000 },
                new DevilSquareRewards{ EXP = 6000, Zen =  35000 },
                new DevilSquareRewards{ EXP = 4000, Zen =  30000 },
                new DevilSquareRewards{ EXP = 2000, Zen =  25000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP =10000, Zen =  50000 },
                new DevilSquareRewards{ EXP = 8000, Zen =  45000 },
                new DevilSquareRewards{ EXP = 6000, Zen =  40000 },
                new DevilSquareRewards{ EXP = 4000, Zen =  35000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP =20000, Zen =  60000 },
                new DevilSquareRewards{ EXP =10000, Zen =  55000 },
                new DevilSquareRewards{ EXP = 8000, Zen =  50000 },
                new DevilSquareRewards{ EXP = 6000, Zen =  45000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP =22000, Zen =  70000 },
                new DevilSquareRewards{ EXP =20000, Zen =  65000 },
                new DevilSquareRewards{ EXP =10000, Zen =  60000 },
                new DevilSquareRewards{ EXP = 8000, Zen =  55000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP =24000, Zen =  80000 },
                new DevilSquareRewards{ EXP =22000, Zen =  75000 },
                new DevilSquareRewards{ EXP =20000, Zen =  70000 },
                new DevilSquareRewards{ EXP =10000, Zen =  65000 }
            },
            new DevilSquareRewards[]{
                new DevilSquareRewards{ EXP =26000, Zen =  90000 },
                new DevilSquareRewards{ EXP =24000, Zen =  85000 },
                new DevilSquareRewards{ EXP =22000, Zen =  80000 },
                new DevilSquareRewards{ EXP =20000, Zen =  75000 }
            },
        };

        private DevilSquare[] _devilSquares;
        private DateTimeOffset _nextSpawn;
        private int _nextSpawnNumber;
        private DateTimeOffset _nextMessage = DateTimeOffset.Now;
        internal IEnumerable<MapInfo> _maps;

        public const int MaxPlayers = 10;
        private const int ClosedTime = 17;
        private const int OpenedTime = 2;
        private const int PlayingTime = 5;

        public DevilSquares() : 
            this(TimeSpan.FromMinutes(ClosedTime), TimeSpan.FromMinutes(OpenedTime), TimeSpan.FromMinutes(PlayingTime))
        {
        }
        public DevilSquares(TimeSpan close, TimeSpan open, TimeSpan playing) : base(close, open, playing)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(DevilSquares));

            _devilSquares = new DevilSquare[7]
            {
                new DevilSquare(this, 1, close+playing, open, playing, 58),
                new DevilSquare(this, 2, close+playing, open, playing, 59),
                new DevilSquare(this, 3, close+playing, open, playing, 60),
                new DevilSquare(this, 4, close+playing, open, playing, 61),
                new DevilSquare(this, 5, close+playing, open, playing, 111),
                new DevilSquare(this, 6, close+playing, open, playing, 112),
                new DevilSquare(this, 7, close+playing, open, playing, 270),
            };

            _maps = Resources.ResourceCache.Instance.GetMaps()
                .Where(x => x.Key == Maps.DevilSquare || x.Key == Maps.DevilSquare2)
                .Select(x => x.Value);

            foreach(var map in _maps)
            {
                map.MonsterAdd += OnMonsterAdd;
            }
        }

        public override void Initialize()
        {
            Trigger(EventState.Open);
        }

        public override void NPCTalk(Player plr)
        {
            if(CurrentState != EventState.Open)
            {
                plr.Session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.DevilStarted));
                return;
            }

            var res = plr.Character.Inventory.FindAllItems(r_devilInvitation);

            //var DSNumber = GetPlayerDS(plr);

            if (!res.Any(/*x => x.Plus == DSNumber*/))
            {
                plr.Session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.YouNeedInvitationDS));
                return;
            }

            plr.Session.SendAsync(new STalk { Result = NPCWindow.DevilSquared });
        }

        public override void OnTransition(EventState nextState)
        {
            var ns = nextState;
            switch(ns)
            {
                case EventState.Closed:
                    Trigger(EventState.Open, _closedTime);
                    break;
                case EventState.Open:
                    Trigger(EventState.Playing, _openedTime);
                    foreach (var ds in _devilSquares)
                        ds.Trigger(EventState.Open);
                    break;
                case EventState.Playing:
                    _nextSpawnNumber = 0;
                    _nextSpawn = DateTimeOffset.Now.Add(_playingTime / 7);
                    Trigger(EventState.Closed, _playingTime);
                    break;
            }
        }

        public override void Update()
        {
            switch (CurrentState)
            {
                case EventState.Closed:
                    if (TimeLeft.TotalMinutes <= 16 && ((int)TimeLeft.TotalMinutes) % 5 == 0 && _nextMessage < DateTimeOffset.Now)
                    {
                        _nextMessage = DateTimeOffset.Now.AddMinutes(4);
                        Program.NoEventMapAnoucement($"DevilSquare will be open in {(int)TimeLeft.TotalMinutes} minutes");
                    }
                    if((int)TimeLeft.TotalSeconds == 30)
                    {
                        Program.server.Clients
                            .Where(x => x.Player.Status == LoginStatus.Playing)
                            .SendAsync(new SDevilSquareSet(DevilSquareState.Close));
                    }
                    break;
                case EventState.Open:
                    if((int)TimeLeft.TotalSeconds == 30)
                    {
                        Program.server.Clients
                            .Where(x => x.Player.Status == LoginStatus.Playing)
                            .SendAsync(new SDevilSquareSet(DevilSquareState.Open));
                    }
                    break;
                case EventState.Playing:
                    if(DateTimeOffset.Now >= _nextSpawn)
                    {
                        _nextSpawn = DateTimeOffset.Now.Add(_playingTime / 7);
                        _nextSpawnNumber++;
                        foreach (var ds in _devilSquares)
                        {
                            ds.SpawList(_nextSpawnNumber);
                        }
                    }

                    if ((int)TimeLeft.TotalSeconds == 30)
                    {
                        Resources.ResourceCache.Instance.GetMaps()
                            .Where(x => x.Key == Maps.DevilSquare || x.Key == Maps.DevilSquare2)
                            .SelectMany(y => y.Value.Players)
                            .Select(z => z.Player.Session)
                            .SendAsync(new SDevilSquareSet(DevilSquareState.Playing));
                    }
                    break;
            }
            foreach (var ds in _devilSquares)
            {
                ds.Update();
            }
            base.Update();
        }

        public int GetPlayerDS(Player plr)
        {
            int DSNumber = -1;
            if (plr.Character.MasterClass)
                DSNumber = 7;
            else
                for (var i = 0; i < 6; i++)
                {
                    if (plr.Character.BaseClass == HeroClass.MagicGladiator || plr.Character.BaseClass == HeroClass.DarkLord)
                    {
                        if (r_levels[i].Special <= plr.Character.Level && 
                            r_levels[i + 1].Special > plr.Character.Level)
                        {
                            DSNumber = i + 1;
                            break;
                        }
                    }
                    else
                    {
                        if (r_levels[i].Normal <= plr.Character.Level &&
                            r_levels[i + 1].Normal > plr.Character.Level)
                        {
                            DSNumber = i + 1;
                            break;
                        }
                    }
                }

            return DSNumber;
        }

        public override bool TryAdd(Player plr)
        {
            var DSNumber = GetPlayerDS(plr);
            var ds = _devilSquares[DSNumber - 1];
            return ds.TryAdd(plr);
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void OnMonsterAdd(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }
    }
}
