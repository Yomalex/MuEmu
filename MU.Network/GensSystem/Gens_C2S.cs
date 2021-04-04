using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.GensSystem
{
    [WZContract]
    public class CRequestJoin : IGensMessage
    {
        [WZMember(0)] public GensType Influence { get; set; }
    }
    [WZContract]
    public class CRequestLeave : IGensMessage
    {
        //[WZMember(0)] public byte Result { get; set; }
    }
}
