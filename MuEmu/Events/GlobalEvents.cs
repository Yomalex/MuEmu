using MU.Resources;
using MuEmu.Resources;
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
            Logger.Information(ServerMessages.GetMessage(Messages.GE_AddEvent), name, ev.Active, ev.Rate);
            _events.Add(name, ev);
            ev.Name = name;
            return this;
        }

        public Item GetItem(ushort mobLevel, Maps map)
        {
            foreach(var ev in _events)
            {
                var ret = ev.Value.GetItem(mobLevel, map);
                if (ret == null)
                    continue;

                Logger.Information(ServerMessages.GetMessage(Messages.GE_GetItem), ev.Key, map, ret);

                return ret;
            }

            foreach(var ev in Program.EventManager.GetEvents())
            {
                var ret = ev.GetItem(mobLevel, map);
                if (ret == null)
                    continue;

                Logger.Information(ServerMessages.GetMessage(Messages.GE_GetItem), ev.GetType(), map, ret);

                return ret;
            }

            return null;
        }

        public void Update()
        {
            _events.Values.ToList().ForEach(x => x.Update());
            Program.Experience.ExperienceRate = (float)ExpAdd;
        }

        public double ExpAdd => _events.Values.Where(x => x.Active).Sum(x => x.ExpAdd);
        public bool AnyEvent => _events.Values.Any(x => x.Active);
    }
}
