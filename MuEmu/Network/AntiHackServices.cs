using MU.Network.AntiHack;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network
{
    public class AntiHackServices : MessageHandler
    {
        [MessageHandler(typeof(CAHCheck))]
        public void AHCheck(GSSession session, CAHCheck message)
        { }
    }
}