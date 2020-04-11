using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network.AntiHack
{
    public class AntiHackServices : MessageHandler
    {
        [MessageHandler(typeof(CAHCheck))]
        public void AHCheck(GSSession session, CAHCheck message)
        { }
    }
}