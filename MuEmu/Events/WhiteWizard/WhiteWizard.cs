using MU.Resources;
using MuEmu.Monsters;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu.Events.WhiteWizard
{
    public class WhiteWizard : Event
    {
        private Maps _eventMap;
        private Dictionary<Maps, Rectangle> _eventSpot = new Dictionary<Maps, Rectangle>()
        {
            {Maps.Lorencia, new Rectangle(66,44,125,153)},
            {Maps.Davias, new Rectangle(111,14,121,111)},
            {Maps.Noria, new Rectangle(136,53,87,128)},
        };
        private Dictionary<Maps, Point[]> _eventDest = new Dictionary<Maps, Point[]>()
        {
            { Maps.Lorencia, new Point[]{ new Point(133, 79), new Point(87, 126), new Point(133, 178), new Point(180, 126) } },
            { Maps.Davias, new Point[]{ new Point(164, 42), new Point(221, 85), new Point(164, 42), new Point(221, 85) } },
            { Maps.Noria, new Point[]{ new Point(160, 45), new Point(160, 45), new Point(152, 117), new Point(209, 133) } }
        };
        private List<Monster> _monsters = new List<Monster>();
        private Monster _whiteWizard;

        public WhiteWizard()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WhiteWizard));
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Open);
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
            if (mob.Info.Monster == 135)
            {
                _=Program.GlobalAnoucement($"{mob.Killer.Character.Name} as defeated the Lorencia White Wizard corps!");
            }
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    {
                        _whiteWizard = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 135);
                        var Orcs = MonstersMng.Instance.Monsters.Where(x => x.Info.Monster == 136 || x.Info.Monster == 137);
                        if (_whiteWizard != null)
                            _monsters.Add(_whiteWizard);

                        _monsters.AddRange(Orcs);

                        foreach (var Mob in _monsters)
                        {
                            Mob.Active = false;
                            Mob.Die += OnMonsterDead;
                        }
                        if (_whiteWizard == null)
                        {
                            Trigger(EventState.None);
                            return;
                        }

                        _whiteWizard.Die += OnMonsterDead;
                        Trigger(EventState.Playing);
                    }
                    break;
                case EventState.Playing:
                    {
                        _eventMap = _eventSpot.Keys.ElementAt(Program.RandomProvider(_eventSpot.Keys.Count));
                        var rect = _eventSpot[_eventMap];
                        var randomSpawX = (byte)Program.RandomProvider(rect.Right, rect.Left);
                        var randomSpawY = (byte)Program.RandomProvider(rect.Bottom, rect.Top);
                        var map =Resources.ResourceCache.Instance.GetMaps()[_eventMap];
                        do
                        {
                            randomSpawX = (byte)Program.RandomProvider(rect.Right, rect.Left);
                            randomSpawY = (byte)Program.RandomProvider(rect.Bottom, rect.Top);
                        } while (map.GetAttributes(new Point(randomSpawX, randomSpawY)).Contains(MapAttributes.Safe));

                        foreach (var mob in _monsters)
                        {
                            mob.Active = true;

                            if (mob.Info.Monster == 135)
                            {
                                mob.Warp(_eventMap, randomSpawX, randomSpawY);
                            }
                            else
                            {
                                mob.Warp(
                                    _eventMap, 
                                    (byte)Program.RandomProvider(randomSpawX + 5, randomSpawX - 5),
                                    (byte)Program.RandomProvider(randomSpawY + 5, randomSpawY - 5)
                                    );
                            }
                        }
                        _logger.Information("Started in {0},{1}", randomSpawX, randomSpawY);
                        _ = Program.GlobalAnoucement($"The White Wizard and his corps has invaded {_eventMap}");
                        Trigger(EventState.Closed, TimeSpan.FromDays(1));
                    }
                    break;
                case EventState.Closed:
                    Trigger(EventState.Playing);
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
