using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu
{
    public class Character
    {
        public Player Player { get; }
        public Account Account { get; }
        public Guild Guild { get; set; }

        public HeroClass Class { get; set; }
        public string Name { get; set; }

        public Maps Map { get; set; }
        public Point Position { get; set; }
        public long Experience { get; set; }
        public long NextExperience { get; set; }

        // Points
        public ushort LevelUpPoints { get; set; }
        public ushort Str { get; set; }
        public ushort Agi { get; set; }
        public ushort Vit { get; set; }
        public ushort Ene { get; set; }
        public ushort Cmd { get; set; }

        public short AddPoints => 0;
        public short MaxAddPoints => 0;
        public short MinusPoints => 0;
        public short MaxMinusPoints => 0;

        public byte ClientClass => GetClientClass(Class);

        public static byte GetClientClass(HeroClass dbClass)
        {
            int @class = (int)dbClass;
            return (byte)(((@class & 0x70) << 1) | ((@class & 0x03) == 1 ? 0x10 : (((@class & 0x03) == 2) ? 0x18 : 0x00)));
        }
    }
}
