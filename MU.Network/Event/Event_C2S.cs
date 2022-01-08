using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Event
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

    [WZContract]
    public class CCrywolfContract : IEventMessage
    {
        [WZMember(0)]
        public ushort wzIndex { get; set; }
        public ushort Index => wzIndex.ShufleEnding();
    }

    [WZContract]
    public class CCrywolfState : IEventMessage
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

    [WZContract]
    public class CKanturuEnterBossMap : IEventMessage
    { }

    [WZContract]
    public class CImperialGuardianEnter : IEventMessage
    { }

    [WZContract]
    public class CMuRummyOpen : IEventMessage
    { }

    [WZContract(Serialized = true)]
    public class CEventItemGet : IEventMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }
    [WZContract]
    public class CEventItemThrow : IEventMessage
    {
        [WZMember(0)] public byte px { get; set; }
        [WZMember(1)] public byte py { get; set; }
        [WZMember(2)] public byte Ipos { get; set; }
    }

    [WZContract]
    public class CAcheronEventEnter : IEventMessage
    { }
}
