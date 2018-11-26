using MuEmu.Data;
using MuEmu.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public struct ItemNumber
    {
        public ushort Number { get; set; } 
        public ushort Index { get; set; }
        public byte Type { get; set; }
        public const ushort Invalid = 0xFFFF;

        public ItemNumber(ushort number)
        {
            Number = number;
            Type = (byte)(number >> 9);
            Index = (ushort)(number & 0x3ff);
        }

        public ItemNumber(byte type, ushort index)
        {
            Number = (ushort)(type << 9 | index);
            Type = type;
            Index = index;
        }

        public static implicit operator ItemNumber(ushort num)
        {
            return new ItemNumber(num);
        }

        public static bool operator ==(ItemNumber a, ItemNumber b)
        {
            return a.Number == b.Number;
        }

        public static bool operator !=(ItemNumber a, ItemNumber b)
        {
            return a.Number != b.Number;
        }

        public static bool operator ==(ItemNumber a, ushort b)
        {
            return a.Number == b;
        }

        public static bool operator !=(ItemNumber a, ushort b)
        {
            return a.Number != b;
        }

        public static implicit operator ushort(ItemNumber a)
        {
            return a.Number;
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"T{Type}-I{Index}";
        }
    }
    public class Item
    {
        public ItemNumber Number { get; set; }
        public Character Player { get; set; }
        public int Serial { get; set; }
        public ItemInfo BasicInfo { get; set; }
        public byte Plus { get; set; }
        public byte SmallPlus => (byte)(Plus > 0 ? (Plus - 1) / 2 : 0);
        public bool Luck { get; set; }
        public bool Skill { get; set; }
        public byte Durability { get; set; }
        public byte Option28 { get; set; }
        public byte OptionExe { get; set; }
        public byte SetOption { get; set; }
        public HarmonyOption Harmony { get; set; }
        public SocketOption[] Slots { get; set; }

        public Item(ushort number, int Serial, object Options = null)
        {
            var ItemDB = ResourceCache.Instance.GetItems();

            if (!ItemDB.ContainsKey(number))
                throw new Exception("Item don't exists " + number);

            BasicInfo = ItemDB[number];

            Durability = BasicInfo.Durability;
            Slots = Array.Empty<SocketOption>();

            if (Options != null)
                Extensions.AnonymousMap(this, Options);

            Number = number;
        }

        public byte[] GetBytes()
        {
            using (var ms = new MemoryStream(7+5))
            {
                ms.WriteByte((byte)(BasicInfo.Number&0xff));

                var tmp = (Plus << 3) | (Skill ? 128 : 0) | (Luck ? 4 : 0) | Option28 & 3;
                ms.WriteByte((byte)tmp);
                ms.WriteByte(Durability);
                ms.WriteByte((byte)(((BasicInfo.Number & 0x100) >> 1) | (Option28 > 3 ? 0x40 : 0)));
                ms.WriteByte(SetOption); // Acient Option
                ms.WriteByte((byte)(((BasicInfo.Number & 0x1E00) >> 5) | ((OptionExe & 0x80) >> 4)));
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
