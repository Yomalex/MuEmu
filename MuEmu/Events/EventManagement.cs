using MuEmu.Events.Minigames;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events
{
    public enum Events
    {
        DevilSquared,
        BloodCastle,
        Kanturu,
        ChaosCastle,
        Crywolf,
        ImperialGuardian,
        DoubleGoer,
        MoonRabbit,
        WhiteWizard,
        EventEgg,
        MuRummy,
        CastleSiege,
        Raklion,
        AcheronGuardian,
        JeweldryBingo,
        MineSweeper,
        BallsAndCows,
    }
    internal class EventInfo
    {
        public Event Obj { get; set; }

        public bool IsEnabled { get; set; }
    }
    public class EventManagement
    {
        Dictionary<Events, EventInfo> _events = new Dictionary<Events, EventInfo>();
        private static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(EventManagement));

        public EventManagement AddEvent(Events ev, Event obj)
        {
            _events.Add(ev, new EventInfo { IsEnabled = true, Obj = obj });
            _logger.Information("Event {ev} Initialized", ev);
            obj.Initialize();
            return this;
        }

        public EventManagement SetEventEnable(Events ev, bool isEnabled)
        {
            _events[ev].IsEnabled = isEnabled;
            return this;
        }

        public T GetEvent<T>(/*Events ev*/)
        {
            return (T)(object)_events.Values.FirstOrDefault(x => x.Obj.GetType() == typeof(T))?.Obj??default(T);
            //_events[ev].Obj;
        }

        public IEnumerable<Event> GetEvents()
        {
            return _events.Values
                .Where(x => x.IsEnabled)
                .Select(x => x.Obj);
        }

        public void Update()
        {
            foreach(var ev in _events.Values.Where(x => x.IsEnabled))
            {
                ev.Obj.Update();
            }
        }
    }
}
