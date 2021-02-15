using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.GensSystem
{
    [WZContract]
    public class SGensSendInfoS9 : IGensMessage
    {
        [WZMember(0)] public byte Influence { get; set; }
        [WZMember(1)] public int Ranking { get; set; }
        [WZMember(2)] public int Class { get; set; }
        [WZMember(3)] public int ContributePoint { get; set; }
        [WZMember(4)] public int NextContributePoint { get; set; }
    }
}
