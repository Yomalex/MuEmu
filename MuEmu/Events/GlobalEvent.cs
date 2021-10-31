using MU.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events
{
    class DropRange
    {
        public ushort MinLevel;
        public ushort MaxLevel;
        public Maps Map;
        public Item Item;
    }
    public class GlobalEvent
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GlobalEvent));
        private GlobalEvents manager;
        private List<DropRange> ranges = new List<DropRange>();
        public bool Active { get; set; }
        public byte Rate { get; set; }
        public GERepeatType RepeatType { get; set; }
        public DateTime Start { get; set; }
        public TimeSpan Duration { get; set; }
        public double ExpAdd { get; set; }
        public string Name { get; set; }

        public GlobalEvent(GlobalEvents mngr)
        {
            manager = mngr;
        }

        public GlobalEvent AddRange(Item it, ushort mobMinLevel, ushort mobMaxLevel, Maps map = (Maps)255)
        {
            ranges.Add(new DropRange { Item = it, MaxLevel = mobMaxLevel, MinLevel = mobMinLevel, Map = map });
            return this;
        }

        public Item GetItem(ushort mobLevel, Maps map)
        {
            if(!Active || Program.RandomProvider(100) > Rate)
            {
                return null;
            }

            var prev = ranges.Where(x => x.Map == (Maps)255 || x.Map == map);

            return prev.FirstOrDefault(x => x.MinLevel <= mobLevel && x.MaxLevel >= mobLevel)?.Item.Clone() as Item??null;
        }

        public void Update()
        {
            Active = Start < DateTime.Now && Start.Add(Duration) > DateTime.Now;
            if(RepeatType != GERepeatType.None && Start.Add(Duration) < DateTime.Now)
            {
                switch(RepeatType)
                {
                    case GERepeatType.Annually:
                        Start = Start.AddYears(1);
                        break;
                    case GERepeatType.Monthly:
                        Start = Start.AddMonths(1);
                        break;
                    case GERepeatType.Weekly:
                        Start = Start.AddDays(7);
                        break;
                    case GERepeatType.Daily:
                        Start = Start.AddDays(1);
                        break;
                }
                Logger
                    .ForContext(Constants.SourceContextPropertyName, Name)
                    .Information("Updated active period, Start {0} - End {1}", Start, Start.Add(Duration));
            }
        }
    }
}
