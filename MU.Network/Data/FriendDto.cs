using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class FriendDto
    {
        [WZMember(0, 10)]
        public byte[] btName { get; set; }

        [WZMember(1)]
        public byte Server { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }
}
