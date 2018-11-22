using System;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network;
using MuEmu.Security;

namespace MuEmu
{
    public class Player
    {
        public Account Account { get; set; }

        public Character Character { get; set; }

        public LoginStatus Status { get; set; }

        public GameCheckSum CheckSum { get; set; }

        public GSSession Session { get; set; }

        public Player(GSSession session)
        {
            Session = session;
            Status = LoginStatus.NotLogged;
            Account = new Account(this, new MU.DataBase.AccountDto { ID = 0, Account = "yomar1234", Character1 = 0, Password = "", Vault="" });
            CheckSum = new GameCheckSum(this);
        }
    }
}
