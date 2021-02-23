using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public enum Element : byte
    {
        None,
        Fire,
        Water,
        Earth,
        Wind,
        Dark,
    }
    public class Pentagrama
    {
        private static Pentagrama _instance;
        private Dictionary<Element, int> _rates = new Dictionary<Element, int>()
        {
            { Element.Fire, 2000 },
            { Element.Water, 4000 },
            { Element.Earth, 6000 },
            { Element.Wind, 8000 },
            { Element.Dark, 10000 },
        };

        public static void Initialize()
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new Pentagrama();
        }

        public static Element GetElement()
        {
            var rand = Program.RandomProvider(10000);

            foreach(var e in _instance._rates)
            {
                if (e.Value > rand)
                    return e.Key;
            }

            return Element.None;
        }
    }
}
