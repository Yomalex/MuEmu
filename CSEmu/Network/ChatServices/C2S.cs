using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace CSEmu.Network.ChatServices
{
    [WZContract]
    public class CChatAuth : IChatMessage
    {
        [WZMember(0)] public ushort wzRoom { get; set; }

        [WZMember(1, typeof(BinaryStringSerializer), 10)] public string Auth { get; set; }

        public ushort Room => wzRoom.ShufleEnding();
    }
}
