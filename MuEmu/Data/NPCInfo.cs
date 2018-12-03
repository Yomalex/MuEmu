using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Data
{
    public class NPCInfo
    {
        public ushort NPC { get; set; }
        public bool Warehouse { get; set; }
        public ushort Quest { get; set; }
        public ShopInfo Shop { get; set; }
    }
}
