using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Event
{
    [WZContract]
    public class CEventRemainTime : IEventMessage
    {
        [WZMember(0)]
        public EventEnterType EventType { get; set; }

        [WZMember(1)]
        public byte ItemLevel { get; set; }
    }

    // LuckyCoins
    [WZContract]
    public class CLuckyCoinsCount : IEventMessage
    { }

    [WZContract]
    public class CLuckyCoinsRegistre : IEventMessage
    { }

    // BloodCastle
    [WZContract]
    public class CBloodCastleMove : IEventMessage
    {
        [WZMember(0)]
        public byte Bridge { get; set; }

        [WZMember(1)]
        public byte ItemPos { get; set; }
    }

}
