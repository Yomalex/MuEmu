using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class FriendDto
    {
        [WZMember(0, 10)]
        public byte[] Name { get; set; }

        [WZMember(1)]
        public byte Server { get; set; }
    }
}
