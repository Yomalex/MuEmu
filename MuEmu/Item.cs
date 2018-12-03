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
            Type = (byte)(number / 512);
            Index = (ushort)(number % 512);
        }

        public ItemNumber(byte type, ushort index)
        {
            Number = (ushort)(type * 512 + index);
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

        public static ItemNumber FromTypeIndex(byte type, ushort index)
        {
            return new ItemNumber(type, index);
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
        public uint BuyPrice { get; private set; }
        public uint SellPrice { get; private set; }
        public HarmonyOption Harmony { get; set; }
        public SocketOption[] Slots { get; set; }

        public Item(ItemNumber number, int Serial, object Options = null)
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
            GetValue();
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

        private void GetValue()
        {
            if (BasicInfo.Zen > 0)
            {
                var res = Math.Floor(Math.Log10(BasicInfo.Zen)) - 1;
                if (res > 0)
                {
                    BuyPrice = (uint)(BasicInfo.Zen / Math.Pow(10, res));
                    BuyPrice *= (uint)Math.Pow(10, res);
                }
                else
                {
                    BuyPrice = (uint)BasicInfo.Zen;
                }

                res = Math.Floor(Math.Log10(BasicInfo.Zen / 3.0)) - 1;
                if (res > 0)
                {
                    SellPrice = (uint)(BasicInfo.Zen / (3.0 * Math.Pow(10, res)));
                    BuyPrice *= (uint)Math.Pow(10, res);
                }
                else
                {
                    SellPrice = (uint)(BasicInfo.Zen / 3.0);
                }
            }
            else
            {
                var l = Math.Sqrt(Plus);
                var Gold = 0;

                switch (Number)
                {
                    case 2063: //Arrow
                        Gold = Plus == 0 ? 70 : 1200 + (Plus - 1) * 800;
                        Gold *= Durability / BasicInfo.Durability;
                        break;
                    case 2055: //Arrow Crossbow
                        Gold = Plus == 0 ? 100 : 1400 + (Plus - 1) * 800;
                        Gold *= Durability / BasicInfo.Durability;
                        break;
                    case 7181: // Bless
                        Gold = 9000000;
                        break;
                    case 7182: // Soul
                        Gold = 6000000;
                        break;
                    case 6159: // Chaos
                        Gold = 8100000;
                        break;
                    case 7184: // Life
                        Gold = 45000000;
                        break;
                    case 6174: // Pack of Bless
                        Gold = (Plus + 1) * 9000000 * 10;
                        break;
                    case 6175: // Pack of Soul
                        Gold = (Plus + 1) * 9000000 * 10;
                        break;
                    case 6671: // Fruits
                        Gold = 33000000;
                        break;
                    case 6670: // Blue Feather | Crest ofMonarch
                        Gold = Plus == 1 ? 7500000 : 180000;
                        break;
                    case 7199: // Jewel of Guardian
                        Gold = 60000000;
                        break;
                    case 14 * 512 + 7: // Siege Potion
                        Gold = Durability * (Plus == 0 ? 900000 : 450000);
                        break;
                    case 13 * 512 + 11: // Order(Guardian/Life Stone)
                        Gold = 2400000;
                        break;
                    case 13 * 512 + 7: // Order(Guardian/Life Stone)
                        Gold = Plus == 0 ? 1500000 : 1200000;
                        break;
                    case 13 * 512 + 32: // Siege Potion
                        Gold = Durability * 150;
                        break;
                    case 13 * 512 + 33: // Siege Potion
                        Gold = Durability * 300;
                        break;
                    case 13 * 512 + 34: // Siege Potion
                        Gold = Durability * 3000;
                        break;
                    case 13 * 512 + 35: // Siege Potion
                        Gold = 30000;
                        break;
                    case 13 * 512 + 36: // Siege Potion
                        Gold = 90000;
                        break;
                    case 13 * 512 + 37: // Siege Potion
                        Gold = 150000;
                        break;
                    case 14 * 512 + 35: // Siege Potion
                        Gold = Durability * 2000;
                        break;
                    case 14 * 512 + 36: // Siege Potion
                        Gold = Durability * 4000;
                        break;
                    case 14 * 512 + 37: // Siege Potion
                        Gold = Durability * 6000;
                        break;
                    case 14 * 512 + 38: // Siege Potion
                        Gold = Durability * 2500;
                        break;
                    case 14 * 512 + 39: // Siege Potion
                        Gold = Durability * 5000;
                        break;
                    case 14 * 512 + 40: // Siege Potion
                        Gold = Durability * 7500;
                        break;
                    case 13 * 512 + 3: // Dinorant
                        Gold = 960000;
                        break;
                    case 14 * 512 + 17: // Devil Eye
                        Gold = (int)(15000 + (6000 * (Plus > 2 ? (Plus - 2) * 2.5 : 1)));
                        break;
                    case 14 * 512 + 18: // Devil Key
                        Gold = (int)(15000 + (6000 * (Plus > 2 ? (Plus - 2) * 2.5 : 1)));
                        break;
                    case 14 * 512 + 19: // Devil Invitation
                        Gold = (int)(60000 + (24000 * (Plus > 2 ? (Plus - 2) * 2.5 : 1)));
                        break;
                    case 14 * 512 + 20: // Remedy of Love
                        Gold = 900;
                        break;
                    case 14 * 512 + 21: // Rena
                        Gold = 900;
                        break;
                    case 14 * 512 + 9: // Ale
                        Gold = 1000;
                        break;
                    case 13 * 512 + 18: // Invisibility Cloak
                        Gold = 150000 + (Plus > 1 ? 504000 + 60000 * Plus : 0);
                        break;
                    case 13 * 512 + 16: // Blood and Paper of BloodCastle
                    case 13 * 512 + 17:
                        switch (Plus)
                        {
                            case 1: Gold = 15000; break;
                            case 2: Gold = 21000; break;
                            case 3: Gold = 30000; break;
                            case 4: Gold = 39000; break;
                            case 5: Gold = 48000; break;
                            case 6: Gold = 60000; break;
                            case 7: Gold = 75000; break;
                        }
                        break;
                    case 13 * 512 + 29: // Armor of Guardman
                        Gold = 5000;
                        break;
                    case 13 * 512 + 20: // Wizards Ring
                        Gold = 30000;
                        break;
                    case 14 * 512 + 28: // Lost Map
                        Gold = 600000;
                        break;
                    case 13 * 512 + 31: // Simbol of Kundum
                        Gold = (int)(((10000.0f) * Durability) * 3.0f);
                        break;
                    case 14 * 512 + 45: // Haloween
                    case 14 * 512 + 46: // Haloween
                    case 14 * 512 + 47: // Haloween
                    case 14 * 512 + 48: // Haloween
                    case 14 * 512 + 49: // Haloween
                    case 14 * 512 + 50: // Haloween
                        Gold = (int)(((50.0f) * Durability) * 3.0f);
                        break;
                    case 12 * 512 + 26: // Gem of Secret
                        Gold = 60000;
                        break;
                    default:
                        switch (Plus)
                        {
                            case 5: l += 4; break;
                            case 6: l += 10; break;
                            case 7: l += 25; break;
                            case 8: l += 45; break;
                            case 9: l += 65; break;
                            case 10: l += 95; break;
                            case 11: l += 135; break;
                            case 12: l += 185; break;
                            case 13: l += 245; break;
                        }

                        Gold = (int)((l + 40) * l * l / 8 + 100);
                        break;
                }

                var res = Math.Floor(Math.Log10(Gold)) - 1;
                if (res > 0)
                {
                    BuyPrice = (uint)(Gold / Math.Pow(10, res));
                    BuyPrice *= (uint)Math.Pow(10, res);
                }
                else
                {
                    BuyPrice = (uint)Gold;
                }

                res = Math.Floor(Math.Log10(Gold / 3.0)) - 1;
                if (res > 0)
                {
                    SellPrice = (uint)(Gold / (3.0 * Math.Pow(10, res)));
                    SellPrice *= (uint)Math.Pow(10, res);
                }
                else
                {
                    SellPrice = (uint)(Gold / 3.0);
                }
            }
        }
    }
}
