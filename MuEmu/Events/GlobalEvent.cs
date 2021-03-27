using MU.Resources;
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
        private GlobalEvents manager;
        private List<DropRange> ranges = new List<DropRange>();
        public bool Active { get; set; }
        public byte Rate { get; set; }

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

            return prev.FirstOrDefault(x => x.MinLevel <= mobLevel && x.MaxLevel >= mobLevel)?.Item??null;
        }
    }
}
