using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class MuBot
    {
        private bool _state;
        private DateTime _start;

        public Player Player { get; private set; }
        public MuBot(Player player)
        {
            Player = player;
            Player.Character.CharacterDie += Character_CharacterDie;
            Player.OnStatusChange += Player_OnStatusChange;
        }

        private void Player_OnStatusChange(object sender, EventArgs e)
        {
            Enable(false);
        }

        private void Character_CharacterDie(object sender, EventArgs e)
        {
            Enable(false);
        }

        public void Enable(bool state = true)
        {
            _start = DateTime.Now;
            _state = state;

            Player.Session.SendAsync(new SMuHelperState {
                Status = (byte)(state ? 0 : 1),
                Money = 0,
                usTime = 0
            }).Wait();
        }

        public void Update()
        {
            var time = DateTime.Now - _start;

            var minutes = (int)time.TotalMinutes;

            if(_state && minutes > 0 && (minutes%5) == 0)
            {
                uint money = (uint)(10 * Player.Character.Level);

                if(money > Player.Character.Money)
                {
                    Enable(false);
                    return;
                }

                Player.Character.Money -= money;

                Player.Session.SendAsync(new SMuHelperState
                {
                    Status = 0,
                    Money = money,
                    usTime = (ushort)minutes
                }).Wait();
            }else if(_state)
            {
                Player.Session.SendAsync(new SMuHelperState
                {
                    Status = 0,
                    Money = 0,
                    usTime = (ushort)minutes
                }).Wait();
            }
        }
    }
}
