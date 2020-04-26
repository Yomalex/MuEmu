using MuEmu.Network.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events.Kanturu
{
    public class Kanturu
    {
        private static Kanturu s_instance;

        public static void Initialize()
        {
            if (s_instance == null)
                s_instance = new Kanturu();
        }

        public static async void NPCTalk(Player plr)
        {
            var msg = new SKanturuStateInfo
            {
                State = KanturuState.None,
                btDetailState = 0,
                btEnter = 0,
                btUserCount = 0,
                iRemainTime = 100,
            };
            await plr.Session.SendAsync(msg);
        }
    }
}
