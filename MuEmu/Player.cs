using System;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network;
using MuEmu.Security;
using MU.DataBase;
using System.Threading.Tasks;
using MuEmu.Entity;
using MU.Resources;
using MuEmu.Monsters;
using MuEmu.Network.GameServices;

namespace MuEmu
{
    public class Player
    {
        private LoginStatus _loginStatus;
        /// <summary>
        /// Session ID or Connection ID
        /// </summary>
        public ushort ID => (ushort)Session.ID;
        public Account Account { get; set; }

        public Character Character { get; set; }

        /// <summary>
        /// Connection Status on GS, NotLogged - Logged - Playing
        /// </summary>
        public LoginStatus Status { get => _loginStatus; set
            {
                if (_loginStatus == value)
                    return;
                _loginStatus = value;
                OnStatusChange?.Invoke(this, new EventArgs());
            }
        }

        public GameCheckSum CheckSum { get; set; }

        /// <summary>
        /// Connection pipeline
        /// </summary>
        public GSSession Session { get; set; }

        public object Window { get; set; }
        public object Killer { get; internal set; }
        public Gens Gens { get; internal set; }

        public Player(GSSession session)
        {
            Session = session;
            Status = LoginStatus.NotLogged;
            OnStatusChange += Player_OnStatusChange;
        }

        private void Player_OnStatusChange(object sender, EventArgs e)
        {
            var plr = sender as Player;
            switch(_loginStatus)
            {
                case LoginStatus.NotLogged:
                case LoginStatus.Logged:
                    GameServices.CCloseWindow(Session);
                    var @char = plr.Character;

                    using (var db = new GameContext())
                    {
                        if(plr.Account != null) plr.Account.Save(db).Wait();
                        if (@char != null)
                        {
                            @char.Save(db).Wait();
                            @char.Dispose();
                        }
                        db.SaveChanges();
                    }

                    plr.Character = null;
                    break;
            }
        }

        public void SetAccount(AccountDto acc)
        {
            Account = new Account(this, acc);
            CheckSum = new GameCheckSum(this);
            Status = LoginStatus.Logged;
        }

        /// <summary>
        ///  Send message to all player in this player View Port
        /// </summary>
        /// <param name="message">Any WZContract message</param>
        /// <param name="exclude">Player excluded</param>
        /// <returns></returns>
        public void SendV2Message(object message, Player exclude = null)
        {
            if (Status != LoginStatus.Playing)
                return;

            Character.SendV2Message(message, exclude);
        }

        public async Task Save(GameContext db)
        {
            if (Account != null) await Account.Save(db);
            if(Character != null) await Character.Save(db);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Sender Player
        /// </summary>
        public event EventHandler OnStatusChange;

        public override string ToString()
        {
            return $"{Account?.ID??0}{Account?.Nickname??""}:{Character?.Name??""}";
        }
    }
}
