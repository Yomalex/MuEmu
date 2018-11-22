using MU.DataBase;
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
        public Inventory Inventory { get; }

        // Basic Info
        public HeroClass Class { get; set; }
        public string Name { get; set; }

        // Map
        public Maps Map { get; set; }
        public Point Position { get; set; }

        // Experience
        public long Experience { get; set; }
        public long NextExperience { get; set; }

        // Points
        public ushort LevelUpPoints { get; set; }
        public ushort Str { get; set; }
        public ushort Agility { get; set; }
        public ushort Vitality { get; set; }
        public ushort Energy { get; set; }
        public ushort Command { get; set; }

        public short AddPoints => 0;
        public short MaxAddPoints => 0;
        public short MinusPoints => 0;
        public short MaxMinusPoints => 0;

        public byte ClientClass => GetClientClass(Class);

        public static byte GetClientClass(HeroClass dbClass)
        {
            int @class = (int)dbClass;
            return (byte)(((@class & 0x70) << 1) | ((@class & 0x03) == 1 ? 0x10 : (((@class & 0x03) == 2) ? 0x08 : 0x00)));
        }

        public Character(Player plr, CharacterDto characterDto)
        {
            Player = plr;
            Account = Player.Account;
            Class = (HeroClass)characterDto.Class;
            Inventory = new Inventory(this, characterDto);
        }
    }
}
