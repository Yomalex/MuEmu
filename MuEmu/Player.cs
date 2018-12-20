using System;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network;
using MuEmu.Security;
using MU.DataBase;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Player
    {
        public Account Account { get; set; }

        public Character Character { get; set; }

        public LoginStatus Status { get; set; }

        public GameCheckSum CheckSum { get; set; }

        public GSSession Session { get; set; }

        public object Window { get; set; }

        public Player(GSSession session)
        {
            Session = session;
            Status = LoginStatus.NotLogged;
        }

        public void SetAccount(AccountDto acc)
        {
            Account = new Account(this, acc);
            CheckSum = new GameCheckSum(this);
            Status = LoginStatus.Logged;
        }

        public async Task SendV2Message(object message)
        {
            if (Status != LoginStatus.Playing)
                throw new InvalidOperationException("Player is not playing");

            await Character.SendV2Message(message);
        }
    }
}
