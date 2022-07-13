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
    public class CEventInventoryOpenS16 : IEventMessage
    {
        [WZMember(0)] public byte Event { get; set; }
    }

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

    [WZContract]
    public class CMuRummyStart : IEventMessage
    {
        [WZMember(0)] public byte Type { get; set; }
    }

    [WZContract]
    public class CMuRummyPlayCard : IEventMessage
    {
        [WZMember(0)] public byte From { get; set; }
        [WZMember(1)] public byte To { get; set; }
    }
    [WZContract]
    public class CMuRummyMatch : IEventMessage
    {
        //C1 04 4D 14
    }
    [WZContract]
    public class CMuRummySpecialMatch : IEventMessage
    {
        //C1 04 4D 14
    }
    [WZContract]
    public class CMuRummyExit : IEventMessage
    {
        //C1 04 4D 15
    }
    [WZContract]
    public class CMuRummyReveal : IEventMessage
    {
        //C1 04 4D 11
    }
    [WZContract]
    public class CMuRummyThrow : IEventMessage
    {
        [WZMember(0)] public byte From { get; set; }
        //C1 04 4D 14
    }

    [WZContract]
    public class CMineSweeperOpen : IEventMessage
    { }

    [WZContract]
    public class CMineSweeperStart : IEventMessage
    { }

    [WZContract]
    public class CMineSweeperReveal : IEventMessage
    {
        [WZMember(0)] public byte Cell { get; set; }
    }

    [WZContract]
    public class CMineSweeperMark : IEventMessage
    {
        [WZMember(0)] public byte Cell { get; set; }
    }

    [WZContract]
    public class CMineSweeperGetReward : IEventMessage
    { }

    [WZContract]
    public class CJewelBingoMove : IEventMessage
    {
        [WZMember(0)] public byte Type { get; set; }
        [WZMember(1)] public JBType JewelType { get; set; }
        [WZMember(2)] public byte Slot { get; set; }
    }

    [WZContract]
    public class CJewelBingoBox : IEventMessage
    {
        [WZMember(0)] public byte Box { get; set; }
    }

    [WZContract]
    public class CJewelBingoStart : IEventMessage
    { }

    [WZContract]
    public class CJewelBingoSelect : IEventMessage
    {
        [WZMember(0)] public JBType JewelType { get; set; }
        [WZMember(1)] public byte Slot { get; set; }
    }

    [WZContract]
    public class CJewelBingoGetReward : IEventMessage
    { }
}
