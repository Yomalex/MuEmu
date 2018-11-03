using MuEmu.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Item
    {
        public Player Player { get; set; }
        public int Serial { get; set; }
        public ItemInfo BasicInfo { get; set; }
        public byte Plus { get; set; }
        public bool Luck { get; set; }
        public bool Skill { get; set; }
        public byte Durability { get; set; }
        public HarmonyOption Harmony { get; set; }
        public SocketOption[] Slots { get; set; }

        public Item(ushort Number, int Serial, object Options = null)
        {
            var ItemDB = ResourceCache.Instance.GetItems();
            BasicInfo = ItemDB[Number];
            Slots = new SocketOption[] { SocketOption.None, SocketOption.None, SocketOption.None, SocketOption.None, SocketOption.None };

            if (Options != null)
                Extensions.AnonymousMap(this, Options);
        }

        public byte[] GetBytes()
        {
            using (var ms = new MemoryStream(7+5))
            {
                ms.WriteByte((byte)(BasicInfo.Number&0xff));

                var tmp = Plus << 3;
                ms.WriteByte((byte)tmp);
                ms.WriteByte(Durability);
                ms.WriteByte((byte)((BasicInfo.Number & 0x100) >> 1));
                ms.WriteByte(0); // Acient Option
                ms.WriteByte((byte)((BasicInfo.Number & 0x1E00) >> 5));
                ms.WriteByte((byte)Harmony); // Harmony
                foreach(var slot in Slots)
                {
                    ms.WriteByte((byte)slot);
                }
                for(var i = 0; i < 5 - Slots.Length; i++)
                {
                    ms.WriteByte((byte)SocketOption.None);
                }
                return ms.GetBuffer();
            }
        }
    }
}
