using MuEmu.Util;
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
    }
    internal class EventInfo
    {
        public Event Obj { get; set; }

        public bool IsEnabled { get; set; }
    }
    public class EventManagement
    {
        Dictionary<Events, EventInfo> _events = new Dictionary<Events, EventInfo>();

        public EventManagement AddEvent(Events ev, Event obj)
        {
            _events.Add(ev, new EventInfo { IsEnabled = true, Obj = obj });
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
            return (T)(object)_events.Values.First(x => x.Obj.GetType() == typeof(T)).Obj;
            //_events[ev].Obj;
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
