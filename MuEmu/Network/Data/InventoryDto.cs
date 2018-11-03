using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class InventoryDto
    {
        [WZMember(0)]
        public byte Index { get; set; }

        [WZMember(1, 12)]
        public byte[] Item { get; set; }
    }
}
