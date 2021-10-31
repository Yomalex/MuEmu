using MU.Network.Event;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.CastleSiege
{
    [WZContract]
    public class CSiegeState : IEventMessage
    { }
    [WZContract]
    public class CGuildRegisteInfo : IEventMessage
    { }
    [WZContract]
    public class CGuildMarkOfCastleOwner : IEventMessage
    { }
    [WZContract]
    public class CGuildRegiste : IEventMessage
    { }
    [WZContract]
    public class CSiegeGuildList : IEventMessage
    { }
    [WZContract]
    public class CSiegeRegisteMark : IEventMessage
    {
        [WZMember(0)] public byte ItemPos { get; set; }
    }
}
