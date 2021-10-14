using MU.Resources;
using MuEmu.Monsters;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.MoonRabbit
{
    public class MoonRabbit : Event
    {
        private Maps[] _eventMaps = new Maps[] { Maps.Noria, Maps.Davias, Maps.Lorencia, Maps.Elbeland };
        private Dictionary<Maps, List<Monsters.Monster>> _monsters = new Dictionary<Maps, List<Monsters.Monster>>();
        private Maps _currentEventMap;

        public MoonRabbit()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MoonRabbit));
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Open);
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Playing:
                    if (_monsters.ContainsKey(_currentEventMap))
                    {
                        if(!_monsters[_currentEventMap].Any(x => x.Active))
                        {
                            Trigger(EventState.Closed);
                        }
                    }
                    break;
            }
            base.Update();
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    // rabbits
                    _monsters = Monsters.MonstersMng.Instance.Monsters
                        .Where(x => x.Info.Monster == 413)
                        .GroupBy(x => x.MapID)
                        .ToDictionary(x => x.Key, x => x.ToList());

                    foreach(var mob in _monsters.Values.SelectMany(x => x))
                    {
                        mob.Die += OnMonsterDead;
                        mob.Active = false;
                    }
                    Trigger(EventState.Playing);
                    break;
                case EventState.Playing:
                    _currentEventMap = _eventMaps.ElementAt(Program.RandomProvider(_eventMaps.Length));
                    if(_monsters.ContainsKey(_currentEventMap))
                        _monsters[_currentEventMap].ForEach(x => x.Active = true);

                    Program.NoEventMapAnoucement($"[Moon Rabbit] Lunar Rabits invadieron {_currentEventMap}");
                    break;
                case EventState.Closed:
                    Trigger(EventState.Playing, TimeSpan.FromDays(1));

                    Program.NoEventMapAnoucement($"[Moon Rabbit] Invasion en {_currentEventMap} repelida");
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
