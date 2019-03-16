using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class VPICreateDto
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public byte X { get; set; }

        [WZMember(2)]
        public byte Y { get; set; }

        [WZMember(3, 12)]
        public byte[] ItemInfo { get; set; }
    }
}
