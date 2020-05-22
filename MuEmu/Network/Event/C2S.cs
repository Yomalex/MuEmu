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


    // Crywolf
    [WZContract]
    public class CCrywolfBenefit : IEventMessage
    { }

    // DevilSquare
    [WZContract]
    public class CDevilSquareMove : IEventMessage
    {
        [WZMember(0)] public byte SquareNumber { get; set; }  // 3
        [WZMember(1)] public byte InvitationItemPos { get; set; }	// 4
    }

    // ChaosCastle
    [WZContract]
    public class CChaosCastleMove : IEventMessage
    {
        [WZMember(0)] public byte SquareNumber { get; set; }  // 3
        [WZMember(1)] public byte InvitationItemPos { get; set; }	// 4
    }

    [WZContract]
    public class CKanturuStateInfo : IEventMessage
    { }
}
