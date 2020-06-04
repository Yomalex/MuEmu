using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class GuildListDto
    {
        [WZMember(0, 10)] public byte[] btName { get; set; }  // 0
        [WZMember(1)] public byte Number { get; set; }    // A
        [WZMember(2)] public byte ConnectAServer { get; set; }    // B
        [WZMember(3)] public GuildStatus btGuildStatus{ get; set; }   // C

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }
}
