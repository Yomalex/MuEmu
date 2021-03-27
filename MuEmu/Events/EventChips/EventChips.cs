using MU.Network.Event;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events.EventChips
{
    public class EventChips
    {
        public static EventChips Instance { get; set; }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception(nameof(EventChips) + "Already Initialized");

            Instance = new EventChips();
        }

        public static void NPCTalk(Player plr)
        {
            if(plr.Character.MapID == Maps.Lorencia)
            {
                plr.Session.SendAsync(new SEventChipInfo(0, 0, new short[] { 0, 0, 0 }));
            }
            else if (plr.Character.MapID == Maps.Davias)
            {
                plr.Session.SendAsync(new SEventChipInfo(2, 0, new short[] { 0, 0, 0 }));
            }
        }
    }
}
