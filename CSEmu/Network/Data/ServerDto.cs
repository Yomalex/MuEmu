using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace CSEmu.Network.Data
{
    [WZContract]
    public class ServerDto
    {
        [WZMember(0)]
        public ushort Index { get; set; }

        [WZMember(1)]
        public byte Load { get; set; }

        [WZMember(2)]
        public byte Type { get; set; }
    }
}
