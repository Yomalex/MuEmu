using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class Account
    {
        public int ID { get; set; }

        public string Nickname { get; set; }

        public Player Player { get; set; }

        public Account(Player player)
        {
            Player = player;
        }
    }
}
