using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Security
{
    public class GameCheckSum
    {
        private const int MAX_CHECKSUM_KEY = 1024;
        private int _Index { get; set; }
        public static uint[] Table { get; set; }
        public Player Player { set; get; }
        public DateTime Time { get; set; }

        public static void LoadChecksum(string exePath)
        {

        }

        public GameCheckSum(Player player)
        {
            Player = player;
        }

        public ushort GetKey()
        {
            var rand = new Random();
            _Index = rand.Next(MAX_CHECKSUM_KEY);
            Time = DateTime.Now;

            var wRandom = rand.Next(64);
            var wAcc = ((_Index & 0x3F0) * 64) | (wRandom * 16) | (_Index & 0x0F);
            return (ushort)(wAcc ^ 0xB479);
        }

        public bool IsValid(ushort key)
        {
            return Table[_Index] == key;
        }
    }
}
