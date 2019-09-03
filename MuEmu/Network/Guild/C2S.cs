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
        [WZMember(0)] public int Guild { get; set; }
    }

    [WZContract]
    public class CGuildListAll : IGuildMessage
    { }
}
