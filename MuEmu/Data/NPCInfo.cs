using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Data
{
    public class NPCInfo
    {
        public ushort NPC { get; set; }
        public NPCAttributeType Class { get; set; }
        public ushort Data { get; set; }
        public MiniMapTag Icon { get; set; }
        public ShopInfo Shop => Data != 0xffff ? Resources.ResourceCache.Instance.GetShops()[Data] : null;
    }
}
