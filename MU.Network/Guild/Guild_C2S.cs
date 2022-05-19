using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Guild
{
    [WZContract]
    public class CGuildInfoSave : IGuildMessage
    {
        [WZMember(0)] public byte Type { get; set; }

        [WZMember(1, 8)] public byte[] btName { get; set; }

        [WZMember(2, 32)] public byte[] Mark { get; set; }

        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CGuildReqViewport : IGuildMessage
    {
        [WZMember(0)] public byte Padding { get; set; }
        [WZMember(1)] public int Guild { get; set; }
    }

    [WZContract]
    public class CGuildListAll : IGuildMessage
    { }

    [WZContract]
    public class CGuildRequest : IGuildMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CGuildRequestAnswer : IGuildMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public ushort wzNumber { get; set; }
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CGuildSetStatus : IGuildMessage
    {
        [WZMember(0)] public byte Type { get; set; }
        [WZMember(1)] public GuildStatus Status { get; set; }
        [WZMember(2,10)] public byte[] btName { get; set; }

        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CGuildRemoveUser : IGuildMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)] public string Name { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 20)] public string JoominNumber { get; set; }
    }

    [WZContract]
    public class CRelationShipJoinBreakOff : IGuildMessage
    {
        [WZMember(0)] public GuildRelationShipType RelationShipType { get; set; }    // 3
        [WZMember(1)] public GuildUnionRequestType RequestType { get; set; } // 4
        [WZMember(2)] public ushort wzTargetUserIndex { get; set; }    // 5-6
        [WZMember(3)] public byte Padding { get; set; }
        [WZMember(4, typeof(BinaryStringSerializer), 8)] public string Guild { get; set; }

        public ushort TargetUserIndex { get => wzTargetUserIndex.ShufleEnding(); set => wzTargetUserIndex = value.ShufleEnding(); }
    };

    [WZContract]
    public class CUnionList : IGuildMessage
    { };

    [WZContract]
    public class CGuildMatchingList : IGuildMessage
    {
        [WZMember(0)] public int Page { get; set; }
    }

    [WZContract]
    public class CGuildMatchingListSearch : IGuildMessage
    {
        [WZMember(0)] public int Page { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 11)] public string Text { get; set; }
    }

    [WZContract]
    public class CGuildMatchingRegister : IGuildMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 41)] public string Text { get; set; }
        [WZMember(1)] public GMInterestType InterestType { get; set; }
        [WZMember(2)] public GMLevelRange LevelRange { get; set; }
        [WZMember(3)] public ushortle Class { get; set; }
    }
    
    [WZContract]
    public class CGuildMatchingRegisterCancel : IGuildMessage
    {

    }

    [WZContract]
    public class CGuildMatchingJoin : IGuildMessage
    {
        [WZMember(0)] public int GuildID { get; set; }
    }

    [WZContract]
    public class CGuildMatchingJoinCancel : IGuildMessage
    {
    }

    [WZContract]
    public class CGuildMatchingJoinAccept : IGuildMessage
    {
        [WZMember(0)] public int Type { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 11)] public string Name { get; set; }
    }

    [WZContract]
    public class CGuildMatchingJoinList : IGuildMessage
    {
    }

    [WZContract]
    public class CGuildMatchingJoinInfo : IGuildMessage
    {
    }
}
