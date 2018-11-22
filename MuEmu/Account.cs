using MU.DataBase;
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

        public Dictionary<int, string> Characters { get; set; }

        public Account(Player player, AccountDto accountDto)
        {
            Player = player;
            Characters = new Dictionary<int, string>();
            Nickname = accountDto.Account;
            ID = accountDto.ID;

            if (accountDto.Character1 != null)
            {

            }
            if (accountDto.Character2 != null)
            {

            }
            if (accountDto.Character3 != null)
            {

            }
            if (accountDto.Character4 != null)
            {

            }
            if (accountDto.Character5 != null)
            {

            }
        }
    }
}
