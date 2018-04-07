using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Auth
{

    [WZContract] // 0xC1
    public class SJoinResult : IAuthMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte NumberH { get; set; }

        [WZMember(2)]
        public byte NumberL { get; set; }

        [WZMember(3, 5)]
        public byte[] ClientVersion { get; set; }
    }
}
