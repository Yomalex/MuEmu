using System;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network;
using MuEmu.Security;
using MU.DataBase;
using System.Threading.Tasks;
using MuEmu.Entity;

namespace MuEmu
{
    public class Player
    {
        public ushort ID => (ushort)Session.ID;
        public Account Account { get; set; }

        public Character Character { get; set; }

        public LoginStatus Status { get; set; }

        public GameCheckSum CheckSum { get; set; }

        public GSSession Session { get; set; }

        public object Window { get; set; }
        public object Killer { get; internal set; }

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

        public async Task SendV2Message(object message, Player exclude = null)
        {
            if (Status != LoginStatus.Playing)
                return;

            await Character.SendV2Message(message, exclude);
        }

        public async Task Save(GameContext db)
        {
            if(Account != null)
                await Account.Save(db);

            if(Character != null)
                await Character.Save(db);
        }
    }
}
