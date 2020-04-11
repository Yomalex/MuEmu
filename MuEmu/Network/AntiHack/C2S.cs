using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.AntiHack
{
    [WZContract]
    public class CAHCheck : IAntiHackMessage
    {
        [WZMember(0, 5)] public byte[] Data { get; set; }
    }
}
