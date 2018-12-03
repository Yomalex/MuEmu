using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class VPDestroyDto
    {
        [WZMember(0)]
        public ushort Number { get; set; }

        public VPDestroyDto()
        { }

        public VPDestroyDto(ushort num)
        {
            Number = num.ShufleEnding();
        }
    }
}
