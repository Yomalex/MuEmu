using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.AntiHack
{
    [WZContract]
    public class SAHPreSharedKey : IAntiHackMessage
    {
        [WZMember(0, 32)] public byte[] Key { get; set; }
    }
}
