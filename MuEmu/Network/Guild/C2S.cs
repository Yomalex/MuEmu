using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Guild
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
        [WZMember(0, 10)] public byte[] btName { get; set; }
        [WZMember(1, 20)] public byte[] btJoominNumber { get; set; }

        public string Name => btName.MakeString();
        public string JoominNumber => btJoominNumber.MakeString();
    }
}
