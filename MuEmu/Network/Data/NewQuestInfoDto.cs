using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class NewQuestInfoDto
    {
        [WZMember(0)]
        public ushort Number { get; set; }
        [WZMember(1)]
        public ushort Quest { get; set; }
    }
}
