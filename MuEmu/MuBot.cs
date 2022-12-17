using MU.Network.Game;
using MuEmu.Network.GameServices;
using MuEmu.Resources.Map;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WebZen.Util;

namespace MuEmu
{
    public class MuBot
    {
        private bool _state;
        private DateTime _start;
        private uint TotalMinutes => (uint)(DateTime.Now - _start).TotalMinutes;

        public Player Player { get; private set; }
        public CMUBotData Configuration { get; internal set; }

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

            if (!_state && state)
                Player.Character.HuntingRecord.Start();

            if (_state && !state)
                Player.Character.HuntingRecord.Save();

            _state = state;

            _ = Player.Session.SendAsync(new SMuHelperState {
                Status = (byte)(state ? 0 : 1),
                Money = 0,
                usTime = 0
            });
        }

        public void Update()
        {
            if (!_state)
                return;

            var time = DateTime.Now - _start;
            var minutes = (int)time.TotalMinutes;

            if (minutes <= 0 || (minutes % 5) != 0)
                return;

            uint money = (uint)(10 * Player.Character.Level);
            if (money > Player.Character.Money)
            {
                _state = false;
                Player.Character.Money = 0;
            }

            _ = Player.Session.SendAsync(new SMuHelperState
            {
                Status = (byte)(_state ? 0 : 1),
                Money = money,
                usTime = (ushort)minutes
            });
        }
    }
}
