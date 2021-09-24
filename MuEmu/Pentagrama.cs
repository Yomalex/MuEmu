using MU.Resources;
using MU.Resources.XML;
using MuEmu.Monsters;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Pentagrama
    {
        private static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Pentagrama));
        private static Pentagrama _instance;
        private Dictionary<Element, int> _rates = new Dictionary<Element, int>()
        {
            { Element.Fire, 2000 },
            { Element.Water, 4000 },
            { Element.Earth, 6000 },
            { Element.Wind, 8000 },
            { Element.Dark, 10000 },
        };
        private PentagramaDto _Info;

        private Pentagrama()
        {
            _Info = Resources.ResourceLoader.XmlLoader<PentagramaDto>("./Data/PentagramaItems.xml");
            _logger.Information("Initialized");
            foreach(var mob in _Info.Monsters)
            {
                ushort rated = 0;
                foreach(var it in mob.Items)
                {
                    rated += it.Rate;
                    it.Rate = rated;
                }
            }

            var _openRateMax = 0;
            foreach(var socket in _Info.Sockets)
            {
                _openRateMax += socket.OpenRate;
                socket.OpenRate = _openRateMax;
                var rated = 0;
                foreach (var rate in socket.Rates)
                {
                    rated += rate.Set;
                    rate.Set = rated;
                }
            }
        }

        public static void Initialize()
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new Pentagrama();
        }

        public static Element GetElement()
        {
            var rand = Program.RandomProvider(10000);

            foreach (var e in _instance._rates)
            {
                if (e.Value > rand)
                    return e.Key;
            }

            return Element.None;
        }

        public static int GetSockets()
        {
            var top = _instance._Info.Sockets.Max(x => x.OpenRate);
            var result = _instance._Info
                .Sockets
                .Where(x => x.OpenRate >= Program.RandomProvider(top))
                .FirstOrDefault();

            if (result == null)
                return -1;

            top = result.Rates.Max(x => x.Set);
            var result2 = result.Rates
                .Where(x => x.Set >= Program.RandomProvider(top))
                .FirstOrDefault();

            if(result2 == null)
            {
                return -1;
            }

            if(result2.Slot2 != 100 && result2.Slot2 <= Program.RandomProvider(100))
            {
                return 1;
            }

            if (result2.Slot3 != 100 && result2.Slot3 <= Program.RandomProvider(100))
            {
                return 2;
            }

            if (result2.Slot4 != 100 && result2.Slot4 <= Program.RandomProvider(100))
            {
                return 3;
            }

            if (result2.Slot5 != 100 && result2.Slot5 <= Program.RandomProvider(100))
            {
                return 4;
            }

            return 5;
        }

        public static Item Drop(Monster mob)
        {
            var result = _instance.
                _Info
                .Monsters
                .Where(x => x.Number == mob.Info.Monster)
                .FirstOrDefault();

            if (result == null)
                return null;

            if (Program.RandomProvider(100) > result.Rate)
                return null;

            var top = result.Items.Max(x => x.Rate);

            var itResult = result
                .Items
                .Where(x => x.Rate >= Program.RandomProvider(top))
                .Select(x => new Item(x.Number))
                .FirstOrDefault();

            if (itResult == null)
                return null;

            itResult.PentagramaMainAttribute = GetElement();
            var sockets = GetSockets();
            if (sockets >0)
            {
                itResult.Slots = new SocketOption[sockets];
                for (var i = 0; i < sockets; i++)
                {
                    itResult.Slots[i] = SocketOption.EmptySocket;
                }
            }
            _logger.Information("Item drop {0}", itResult);
            return itResult;
        }
    }
}
