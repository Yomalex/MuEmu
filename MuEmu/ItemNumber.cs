using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public struct ItemNumber
    {
        public ushort Number { get; set; }
        public ushort Index { get => (ushort)(Number % 512); set => Number = (ushort)((Number & ~511) | value); }
        public ItemType Type { get => (ItemType)(Number / 512); set => Number = (ushort)((Number & 511) | (int)value * 512); }
        public const ushort Invalid = 0xFFFF;

        public static readonly ItemNumber Zen = FromTypeIndex(14, 15);

        public ItemNumber(ushort number)
        {
            Number = number;
        }

        public ItemNumber(ItemType type, ushort index)
        {
            Number = (ushort)((byte)type * 512 + (index & 0x1FF));
        }

        public ItemNumber(byte type, ushort index)
        {
            Number = (ushort)(type * 512 + (index & 0x1FF));
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
            return $"{Type}-I{Index}";
        }

        public static ItemNumber FromTypeIndex(byte type, ushort index)
        {
            return new ItemNumber(type, index);
        }

        public static ItemNumber FromTypeIndex(ItemType type, ushort index)
        {
            return new ItemNumber(type, index);
        }
    }
}
