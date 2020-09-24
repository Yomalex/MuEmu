using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MuEmu.Events
{
    public class GlobalEvents
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GlobalEvents));
        private Dictionary<string, GlobalEvent> _events = new Dictionary<string, GlobalEvent>();

        public GlobalEvents AddEvent(string name, GlobalEvent ev)
        {
            Logger.Information("Added event: {0}, Active: {1} - Item Drop Per: {2}", name, ev.Active, ev.Rate);
            _events.Add(name, ev);
            return this;
        }

        public Item GetItem(ushort mobLevel, Maps map)
        {
            foreach(var ev in _events)
            {
                var ret = ev.Value.GetItem(mobLevel, map);
                if (ret == null)
                    continue;

                Logger.Information("Event iten drop for event: {0} - Map:{1} - Item:{2}", ev.Key, map, ret);

                return ret;
            }

            return null;
        }
    }
}
