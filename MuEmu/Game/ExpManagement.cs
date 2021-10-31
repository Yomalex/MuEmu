using MU.Network.Game;
using MuEmu.Network;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Game
{
    public class ExpManagement
    {
        private float addEXPRate;
        private float experienceRate;
        private float goldChannel;
        private float baseExpRate;

        public float BaseExpRate
        {
            get => baseExpRate; 
            set => baseExpRate = value;
        }
        public float AddEXPRate
        {
            get => addEXPRate;
            set
            {
                addEXPRate = value;
                SendExpInfo();
            }
        }
        public float ExperienceRate
        {
            get => experienceRate;
            set
            {
                if (value != experienceRate)
                {
                    experienceRate = value;
                    SendExpInfo();
                }
            }
        }
        public float GoldChannel
        {
            get => goldChannel;
            set
            {
                goldChannel = value;
                SendExpInfo();
            }
        }

        public float FullExperate => (1.0f+BaseExpRate) * (1.0f + AddEXPRate) * (1.0f + ExperienceRate) * (1.0f + GoldChannel);

        public async void SendExpInfo()
        {
            if (Program.server != null)
                await Program.server.Clients.SendAsync(new SExpEventInfo
                {
                    PCBangRate = (ushort)(AddEXPRate * 100.0f),
                    EventExp = (ushort)(ExperienceRate * 100.0f),
                    GoldChannel = (ushort)(GoldChannel * 100.0f),
                });
        }
        public async void SendExpInfo(GSSession session)
        {
            await session.SendAsync(new SExpEventInfo
            {
                PCBangRate = (ushort)(AddEXPRate * 100.0f),
                EventExp = (ushort)(ExperienceRate * 100.0f),
                GoldChannel = (ushort)(GoldChannel * 100.0f),
            });
        }
    }
}
