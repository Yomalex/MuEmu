using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MuEmu.Events
{
    public class GlobalEvents
    {
        private Dictionary<string, GlobalEvent> _events = new Dictionary<string, GlobalEvent>();

        public GlobalEvents AddEvent(string name, GlobalEvent ev)
        {
            _events.Add(name, ev);
            return this;
        }

        public Item GetItem(ushort mobLevel, Maps map)
        {
            return _events
                .Select(x => x.Value.GetItem(mobLevel, map))
                .Where(x => x != null)
                .FirstOrDefault();
        }
    }
}
