using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.AntiHack
{
    public interface IAntiHackMessage
    { }

    public class AntiHackMessageFactory : MessageFactory<AHOpCode, IAntiHackMessage>
    {
        public AntiHackMessageFactory()
        {
            // C2S
            Register<CAHCheck>(AHOpCode.AHCheck);

            // S2C
            Register<SAHPreSharedKey>(AHOpCode.AHEncKey);
        }
    }
}
