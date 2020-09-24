using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Network.Game;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public enum ItemType : byte
    {
        Sword,
        Axe,
        Scepter,
        Spear,
        BowOrCrossbow,
        Staff,
        Shield,
        Helm,
        Armor,
        Pant,
        Gloves,
        Boots,
        Wing_Orb_Seed,
        Missellaneo,
        Potion,
        Scroll,
        End,

        Invalid = 0xff
    };
    public enum SpecialNumber : ushort
    {
        AditionalDamage = 80,
        AditionalMagic = 81,
        SuccessFullBlocking = 82,
        AditionalDefense = 83,
        CriticalDamage = 84,
        RecoverLife = 85,
        ExcellentOption = 86,
        AddLife = 100,
        AddMana = 101,
        AddStamina = 103,
        AddLeaderShip = 105,
        CurseDamage = 113,
        AddMaxMana = 172,
        AddMaxStamina = 173,
        SetAttribute = 0xC3,
        AddStrength = 196,
        AddAgility = 197,
        AddEnergy = 198,
        AddVitality = 199,
    }
    public struct ItemNumber
    {
        public ushort Number { get; set; }
        public ushort Index { get => (ushort)(Number % 512); set => Number = (ushort)((Number& ~511)|value); }
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

    public class Item : ICloneable
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Item));
        private static Random _rand = new Random();
        private byte _plus;
        private byte _durability;
        private byte _option;
        private int _aid;
        private int _cid;
        private int _vid;
        private int _slot;
        private float _durabilityDown;
        private SocketOption[] _slots;
        private JewelOfHarmony _jewelOfHarmony = new JewelOfHarmony();

        public int AccountId {
            get => _aid;
            set
            {
                if (_aid == value)
                    return;
                _aid = value;

                NeedSave = true;
            }
        }
        public int CharacterId
        {
            get => _cid;
            set
            {
                if (_cid == value)
                    return;
                _cid = value;

                NeedSave = true;
            }
        }
        public Character Character { get; set; }
        public int VaultId {
            get => _vid;
            set {
                if (_vid == value)
                    return;
                _vid = value;

                NeedSave = true;
            }
        }
        public int SlotId {
            get => _slot;
            set {
                if (_slot == value)
                    return;
                _slot = value;
                NeedSave = true;
            }
        }

        public ItemInfo BasicInfo { get; set; }
        public ItemNumber Number { get; set; }
        public int Serial { get; set; }
        public byte Plus {
            get => _plus;
            set
            {
                if (_plus == value)
                    return;
                _plus = value;

                NeedSave = true;
                OnItemChange();
            }
        }
        public byte SmallPlus => (byte)(Plus > 0 ? (Plus - 1) / 2 : 0);
        public bool Luck { get; set; }
        public bool Skill { get; set; }
        public Spell Spell { get; set; }
        public byte Durability {
            get =>
                _durability;
            set {
                if (_durability == value)
                    return;

                if (value > BasicInfo.Durability)
                    value = BasicInfo.Durability;

                _durability = value;
                OnDurabilityChange(false);
                NeedSave = true;
            }
        }
        public byte Option28
        {
            get => _option;
            set
            {
                if (_option == value)
                    return;

                _option = value;
                OnItemChange();
            }
        }
        public byte OptionExe { get; set; }
        public byte SetOption { get; set; }
        public uint BuyPrice { get; private set; }
        public uint SellPrice { get; private set; }
        public int RepairPrice => RepairItemPrice();
        //public HarmonyOption Harmony { get; set; }
        public SocketOption[] Slots {
            get => _slots;
            set
            {
                _slots = value;
                NeedSave = true;
                OnItemChange();
            }
        }
        public List<SpecialNumber> Special { get; set; } = new List<SpecialNumber>();
        public JewelOfHarmony Harmony { get => _jewelOfHarmony;
            set
            {
                _jewelOfHarmony = value;
                NeedSave = true;
            }
        }

        public uint PShopValue { get; set; }

        public bool NeedSave { get; set; }

        // Needed Stats
        public int ReqStrength { get; set; }
        public int ReqAgility { get; set; }
        public int ReqVitality { get; set; }
        public int ReqEnergy { get; set; }
        public int ReqCommand { get; set; }

        // Options
        public int CriticalDamage => Special.Contains(SpecialNumber.CriticalDamage) ? 4 : 0;
        public int AditionalDamage => Special.Contains(SpecialNumber.AditionalDamage) ? Option28 * 4 : 0;
        public int AditionalMagic => Special.Contains(SpecialNumber.AditionalMagic) ? Option28 * 4 : 0;
        public int AditionalDefense => Special.Contains(SpecialNumber.AditionalDefense) ? Option28 * 4 : 0;
        public int AddLife => Special.Contains(SpecialNumber.AddLife) ? Character.Level * 5 + 50 : 0;
        public int AddMana => Special.Contains(SpecialNumber.AddMana) ? Character.Level * 5 + 50 : 0;
        public int AddStamina => Special.Contains(SpecialNumber.AddStamina) ? 50 : 0;
        public int AddLeaderShip => Special.Contains(SpecialNumber.AddLeaderShip) ? Character.Level * 5 + 10 : 0;

        // Weapon Excellent Effects
        public float ExcellentDmgRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.ExcellentDmgRate) != 0 ? 0.1f : 0.0f) : 0.0f;
        public int IncreaseWizardry => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseWizardry) != 0 ? (Character?.Level??0)/20 : 0) : 0;
        public float IncreaseWizardryRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseWizardryRate) != 0 ? 0.2f : 0) : 0;
        public float IncreaseLifeRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseLifeRate) != 0 ? 1.0f/8.0f : 0) : 0;
        public float IncreaseManaRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseManaRate) != 0 ? 1.0f/8.0f : 0) : 0;
        
        // Armor Excellent Effects
        public float IncreaseZenRate => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.IncreaseZen) != 0 ? 0.4f : 0.0f) : 0.0f;
        public float DefenseSuccessRate => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.DefenseSuccessRate) != 0 ? 0.1f : 0.0f) : 0.0f;
        public float ReflectDamage => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.ReflectDamage) != 0 ? 0.05f : 0.0f) : 0.0f;
        public float DamageDecrease => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.DamageDecrease) != 0 ? 0.04f : 0.0f) : 0.0f;
        public float IncreaseMana => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.IncreaseMana) != 0 ? 0.04f : 0.0f) : 0.0f;
        public float IncreaseHP => Number.Type > ItemType.Shield && Number.Type <= ItemType.Boots ? ((((ExcellentOptionArmor)OptionExe) & ExcellentOptionArmor.IncreaseHP) != 0 ? 0.04f : 0.0f) : 0.0f;

        public int AttackMin { get; private set; }
        public int AttackMax { get; private set; }
        public bool Attack => AttackMax - AttackMin > 0;
        public int Defense { get; private set; }
        public int DefenseRate { get; private set; }
        public int MagicDefense { get; private set; }

        public static Item Zen(uint BuyPrice)
        {
            return new Item(ItemNumber.Zen, 0, new { BuyPrice });
        }

        public Item(ItemNumber number, int Serial = 0, object Options = null)
        {
            var ItemDB = ResourceCache.Instance.GetItems();

            if (!ItemDB.ContainsKey(number))
                throw new Exception("Item don't exists " + number);

            BasicInfo = ItemDB[number];
            _durability = BasicInfo.Durability;
            _slots = Array.Empty<SocketOption>();

            if (Options != null)
                Extensions.AnonymousMap(this, Options);

            Harmony.Item = this;

            Number = number;
            GetValue();
            CalcItemAttributes();
        }

        public Item(ItemDto dto)
        {
            var ItemDB = ResourceCache.Instance.GetItems();

            if (!ItemDB.ContainsKey(dto.Number))
                throw new Exception("Item don't exists " + dto.Number);

            BasicInfo = ItemDB[dto.Number];

            _aid = dto.AccountId;
            _cid = dto.CharacterId;
            _vid = dto.VaultId;
            _slot = dto.SlotId;
            Serial = dto.ItemId;
            Skill = dto.Skill;
            Number = dto.Number;
            _plus= dto.Plus;
            Luck = dto.Luck;
            _option = dto.Option;
            OptionExe = dto.OptionExe;
            
            _durability = dto.Durability;
            if(string.IsNullOrEmpty(dto.SocketOptions))
            {
                _slots = Array.Empty<SocketOption>();
            }else
            {
                var tmp = dto.SocketOptions.Split(",");
                _slots = tmp.Select(x => Enum.Parse<SocketOption>(x)).ToArray();
            }
            Harmony = dto.HarmonyOption;
            Harmony.Item = this;

            GetValue();
            CalcItemAttributes();
            NeedSave = false;
        }

        public byte[] GetBytes()
        {
            using (var ms = new MemoryStream(7+5))
            {
                ms.WriteByte((byte)(Number & 0xff));

                // Is ZEN?
                if (Number == ItemNumber.Zen)
                {
                    // 0 1 2 3
                    // 3 2 1 0
                    var arr = BitConverter.GetBytes(BuyPrice);
                    ms.WriteByte(arr[2]);
                    ms.WriteByte(arr[1]);
                    ms.WriteByte(0);
                    ms.WriteByte(arr[0]);
                    ms.WriteByte((byte)((Number & 0x1E00) >> 5));
                    ms.WriteByte(0);

                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                }
                else
                {
                    var tmp = (Plus << 3) | (Skill ? 128 : 0) | (Luck ? 4 : 0) | Option28 & 3;
                    ms.WriteByte((byte)tmp);
                    ms.WriteByte(Durability);
                    ms.WriteByte((byte)(((Number & 0x100) >> 1) | (Option28 > 3 ? 0x40 : 0) | (byte)OptionExe));
                    ms.WriteByte(SetOption); // Acient Option
                    ms.WriteByte((byte)(((Number & 0x1E00) >> 5) | (((byte)OptionExe & 0x80) >> 4)));
                    ms.WriteByte(Harmony); // Harmony
                    foreach (var slot in Slots)
                    {
                        ms.WriteByte((byte)slot);
                    }
                    for (var i = 0; i < 5 - Slots.Length; i++)
                    {
                        ms.WriteByte((byte)SocketOption.None);
                    }
                }
                return ms.GetBuffer();
            }
        }

        private void GetValue()
        {
            if (BuyPrice != 0)
                return;

            if (BasicInfo.Zen > 0)
            {
                var res = (int)Math.Log10(BasicInfo.Zen) - 1;
                if (res > 0)
                {
                    var div = (uint)Math.Pow(10, res);
                    BuyPrice = (uint)(BasicInfo.Zen / div);
                    BuyPrice *= div;
                }
                else
                {
                    BuyPrice = (uint)BasicInfo.Zen;
                }

                res = (int)Math.Log10(BasicInfo.Zen / 3.0) - 1;
                if (res > 0)
                {
                    var div = (uint)Math.Pow(10, res);
                    SellPrice = (uint)(BasicInfo.Zen / (3.0 * div));
                    SellPrice *= div;
                }
                else
                {
                    SellPrice = (uint)(BasicInfo.Zen / 3.0);
                }
            }
            else
            {
                var Gold = 0;
                var level2 = BasicInfo.Level + Plus*3;

                if (((byte)ExcellentOptionArmor.FullItem & OptionExe) != 0)
                {
                    level2 += 25;
                }

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
                    case 7190: // Creation
                        Gold = 18000000;
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
                        Gold = 30000000;
                        break;
                    case 14 * 512 + 7: // Siege Potion
                        Gold = Durability * (Plus == 0 ? 900000 : 450000);
                        break;
                    case 13 * 512 + 11: // Order(Guardian/Life Stone)
                        Gold = Plus == 0 ? 1000000 : 2400000;
                        break;
                    case 13 * 512 + 7: // Contract(Summon)
                        Gold = Plus == 0 ? 1500000 : 1200000;
                        break;
                    case 13 * 512 + 32: // Splinter of Armor
                        Gold = Durability * 150;
                        break;
                    case 13 * 512 + 33: // Bless of Guardian
                        Gold = Durability * 300;
                        break;
                    case 13 * 512 + 34: // Claw of Beast
                        Gold = Durability * 3000;
                        break;
                    case 13 * 512 + 35: // Piece of Horn
                        Gold = 30000;
                        break;
                    case 13 * 512 + 36: // Broken Horn
                        Gold = 90000;
                        break;
                    case 13 * 512 + 37: // Horn of Fenrir
                        Gold = 150000;
                        break;
                    case 14 * 512 + 35: // Small SD Potion
                        Gold = Durability * 2000;
                        break;
                    case 14 * 512 + 36: // SD Potion
                        Gold = Durability * 4000;
                        break;
                    case 14 * 512 + 37: // Large SD Potion
                        Gold = Durability * 6000;
                        break;
                    case 14 * 512 + 38: // Small Complex Potion
                        Gold = Durability * 2500;
                        break;
                    case 14 * 512 + 39: // Complex Potion
                        Gold = Durability * 5000;
                        break;
                    case 14 * 512 + 40: // Large Complex Potion
                        Gold = Durability * 7500;
                        break;
                    case 13 * 512 + 109:
                    case 13 * 512 + 110:
                    case 13 * 512 + 111:
                    case 13 * 512 + 112:
                    case 13 * 512 + 113:
                    case 13 * 512 + 114:
                    case 13 * 512 + 115:
                        Gold = 3000;
                        break;
                    case 13 * 512 + 3: // Dinorant
                        Gold = 960000;
                        break;
                    case 14 * 512 + 17: // Devil Eye
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
                        switch(Plus)
                        {
                            case 0:
                                Gold = 9000;
                                Durability = BasicInfo.Durability;
                                break;
                            case 1:
                                Gold = 9000;
                                Durability = BasicInfo.Durability;
                                break;
                            case 3:
                                Gold = 3900 * Durability;
                                break;
                            default:
                                Durability = BasicInfo.Durability;
                                Gold = 9000;
                                break;
                        }
                        break;
                    case 14 * 512 + 9: // Ale
                        Gold = 1000;
                        break;
                    case 13 * 512 + 18: // Invisibility Cloak
                        Gold = 200000 + (Plus > 1 ? 20000 * (Plus-1) : -150000);
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
                    case 13 * 512 + 31: // Spirit
                        Gold = 30000000;
                        break;
                    case 14 * 512 + 28: // Lost Map
                        Gold = 600000;
                        break;
                    case 14 * 512 + 29: // Simbol of Kundum
                        Gold = (int)(10000.0f * Durability * 3.0f);
                        break;
                    case 14 * 512 + 45: // Haloween
                    case 14 * 512 + 46: // Haloween
                    case 14 * 512 + 47: // Haloween
                    case 14 * 512 + 48: // Haloween
                    case 14 * 512 + 49: // Haloween
                    case 14 * 512 + 50: // Haloween
                        Gold = (int)(50.0f * Durability * 3.0f);
                        break;
                    case 12 * 512 + 26: // Gem of Secret
                        Gold = 60000;
                        break;
                    case 14 * 512 + 51: // Sky Event Invitation
                        Gold = 200000;
                        break;
                    case 14 * 512 + 55: // Green Chaos Box
                    case 14 * 512 + 56: // Red Chaos Box
                    case 14 * 512 + 57: // Purple Chaos Box
                        Gold = 9000;
                        break;
                    case 13 * 512 + 49: // Scroll of Illusion
                    case 13 * 512 + 50: // Potion of Illusion
                    case 13 * 512 + 51: // Illusion's Plataform
                        switch (Plus)
                        {
                            case 1:
                                Gold = 500000;
                                break;
                            case 2:
                                Gold = 600000;
                                break;
                            case 3:
                                Gold = 800000;
                                break;
                            case 4:
                                Gold = 1000000;
                                break;
                            case 5:
                                Gold = 1200000;
                                break;
                            case 6:
                                Gold = 1400000;
                                break;
                            default:
                                Gold = 9000;
                                break;
                        }
                        break;
                    case 13 * 512 + 52: // Flame of Condor
                    case 13 * 512 + 53: // Condor's Feathers
                        Gold = 3000000;
                        break;
                    case 13 * 512 + 71: // Sword / Spear/ Blade / Axe
                    case 13 * 512 + 72: // Staff
                    case 13 * 512 + 73: // Bow / Crossbow
                    case 13 * 512 + 74: // Scepter
                    case 13 * 512 + 75: // Sticks
                        Gold = 1000000;
                        break;
                    case 14 * 512 + 23: // Scroll of the Emperor
                    case 14 * 512 + 24: // Broken Sword
                    case 14 * 512 + 25: // Tear of Elf
                    case 14 * 512 + 26: // Soul of Wizard
                    case 14 * 512 + 65: // Flame of Death Beam Knight
                    case 14 * 512 + 66: // Horn of Hell Maine
                    case 14 * 512 + 67: // Feather of Phoenix of Darkness
                    case 14 * 512 + 68: // Eye of the Abyss
                        Gold = 9000;
                        break;
                    case 12 * 512 + 136: // life boundle
                        Gold = (Plus + 1) * 22500000 * 10;
                        break;
                    case 12 * 512 + 137: // creation bundle
                        Gold = (Plus + 1) * 18000000 * 10;
                        break;
                    case 12 * 512 + 138: // guardian bundle
                        Gold = (Plus + 1) * 30000000 * 10;
                        break;
                    case 12 * 512 + 139: // gemstone bundle
                        Gold = (Plus + 1) * 18600 * 10;
                        break;
                    case 12 * 512 + 140: // harmony boundle
                        Gold = (Plus + 1) * 18600 * 10;
                        break;
                    case 12 * 512 + 141: // chaos bundle
                        Gold = (Plus + 1) * 810000 * 10;
                        break;
                    case 12 * 512 + 142: //  bundle
                        Gold = (Plus + 1) * 18600 * 10;
                        break;
                    case 12 * 512 + 143: //  bundle
                        Gold = (Plus + 1) * 18600 * 10;
                        break;
                    case 14 * 512 + 63: // Fireworks
                        Gold = 200000;
                        break;
                    case 14 * 512 + 85: // Cherry Blossom Wine
                    case 14 * 512 + 86: // Cherry Blossom Dumpling
                    case 14 * 512 + 87: // Cherry Blossom Petal
                    case 14 * 512 + 90: // White Cherry Blossom
                        Gold = Durability*300;
                        break;
                    case 14 * 512 + 110: // 
                        Gold = Durability * 30000;
                        break;
                    case 14 * 512 + 111: // 
                        Gold = 600000;
                        break;
                    default:
                        if((Number.Type == ItemType.Wing_Orb_Seed && ((Number.Index > 6 && Number.Index < 36)) 
                            || (Number.Index > 43 && Number.Index < 440))
                            || Number.Type == ItemType.Missellaneo || Number.Type == ItemType.Scroll)
                        {
                            Gold = level2 * level2 * level2 + 100;
                            break;
                        }

                        switch (Plus)
                        {
                            case 5: level2 += 4; break;
                            case 6: level2 += 10; break;
                            case 7: level2 += 25; break;
                            case 8: level2 += 45; break;
                            case 9: level2 += 65; break;
                            case 10: level2 += 95; break;
                            case 11: level2 += 135; break;
                            case 12: level2 += 185; break;
                            case 13: level2 += 245; break;
                        }

                        if(Number.Type == ItemType.Wing_Orb_Seed && Number.Index <= 6) // Wings
                        {
                            Gold = (level2 + 40) * level2 * level2 * 11 + 40000000;
                            break;
                        }

                        if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index >= 36 && Number.Index <= 43) // Wings
                        {
                            Gold = (level2 + 40) * level2 * level2 * 11 + 40000000;
                            break;
                        }

                        if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index == 50) // Wings
                        {
                            Gold = (level2 + 40) * level2 * level2 * 11 + 40000000;
                            break;
                        }

                        Gold = ((level2 + 40) * level2 * level2 / 8 + 100);

                        if(Number.Type >= ItemType.Sword && Number.Type <= ItemType.Shield)
                        {
                            if(BasicInfo.Size.Width == 1)
                            {
                                Gold = Gold * 80 / 100;
                            }
                        }

                        foreach(var sp in Special)
                        {
                            switch(sp)
                            {
                                case (SpecialNumber)18:
                                case (SpecialNumber)19:
                                case (SpecialNumber)20:
                                case (SpecialNumber)21:
                                case (SpecialNumber)22:
                                case (SpecialNumber)23:
                                case (SpecialNumber)24:
                                case (SpecialNumber)56:
                                    Gold = (int)(Gold*1.5f);
                                    break;
                                case SpecialNumber.AditionalDamage:
                                case SpecialNumber.AditionalMagic:
                                case SpecialNumber.AditionalDefense:
                                case SpecialNumber.RecoverLife:
                                case SpecialNumber.CurseDamage:
                                    switch (Option28)
                                    {
                                        case 1:
                                            Gold += (int)(Gold * 6.0 / 10.0);
                                            break;

                                        case 2:
                                            Gold += (int)(Gold * 14.0 / 10.0);
                                            break;

                                        case 3:
                                            Gold += (int)(Gold * 28.0 / 10.0);
                                            break;

                                        case 4:
                                            Gold += (int)(Gold * 56.0 / 10.0);
                                            break;
                                    }
                                    break;
                                case SpecialNumber.SuccessFullBlocking:
                                    Gold += (int)(Gold * 25.0 / 100.0);
                                    break;

                                case (SpecialNumber)86:
                                case (SpecialNumber)87:
                                case (SpecialNumber)88:
                                case (SpecialNumber)89:
                                case (SpecialNumber)90:
                                case (SpecialNumber)91:
                                case (SpecialNumber)92:
                                case (SpecialNumber)93:
                                case (SpecialNumber)94:
                                case (SpecialNumber)95:
                                case (SpecialNumber)96:
                                case (SpecialNumber)97:
                                case (SpecialNumber)98:
                                case (SpecialNumber)99:
                                    Gold += Gold;
                                    break;

                                case (SpecialNumber)100:
                                case (SpecialNumber)101:
                                case (SpecialNumber)102:
                                case (SpecialNumber)103:
                                case (SpecialNumber)104:
                                case (SpecialNumber)108:
                                case (SpecialNumber)109:
                                case (SpecialNumber)110:
                                case (SpecialNumber)111:
                                    Gold += (int)(Gold * 25.0 / 100.0);
                                    break;
                            }
                        }
                        break;
                }

                if(BasicInfo.Zen > 0)
                {
                    Gold += (BasicInfo.Zen * BasicInfo.Zen * 10) / 12;

                    if(Number >= 14 * 512 + 0 && Number <= 14 * 512 + 8)
                    {
                        if((int)Number == 14 * 512 + 3 || (int)Number == 14 * 512 + 6)
                        {
                            Gold *= 2;
                        }

                        if(Plus > 0)
                        {
                            Gold *= Plus * Plus; ;
                        }
                    }
                }

                BuyPrice = (uint)Gold;
                SellPrice = (uint)(Gold / 3.0);
            }
        }

        public void ApplyEffects(Character tTarget)
        {
            if (tTarget == null)
                return;

            Character = tTarget;
            if (Skill && Spell != Spell.None)
                Character.Spells.ItemSkillAdd(this.Spell);
        }

        public void RemoveEffects()
        {
            if (Skill && Spell != Spell.None)
                Character.Spells.ItemSkillDel(this.Spell);

            Character.CalcStats();
        }

        public async Task Save(GameContext db)
        {
            ItemDto item = null;
            var log = Logger;
            if (Character != null)
                log = Logger.ForAccount(Character.Player.Session);

            if (Serial != 0 && NeedSave)
            {
                try
                {
                    item = db.Items.First(x => x.ItemId == Serial);
                    item.AccountId = _aid;
                    item.CharacterId = _cid;
                    item.VaultId = _vid;
                    item.Durability = _durability;
                    item.HarmonyOption = _jewelOfHarmony;
                    item.Option = _option;
                    item.Plus = _plus;
                    item.SlotId = _slot;
                    item.SocketOptions = string.Join(",", _slots.Select(x => x.ToString()));
                    db.Items.Update(item);
                }
                catch(Exception) //?? Don't exists any more?
                {
                    NeedSave = false;
                    log.Information("[A{2}:{3}{4}]Item Deleted?:[{5}] {0} {1}", Number, ToString(), _aid, _vid == 0 ? "C":"V", _vid == 0 ? _cid : _vid, Serial);

                    return;
                }
            }
            else if(Serial == 0)
            {
                item = new ItemDto
                {
                    Number = Number,
                    Luck = Luck,
                    OptionExe = (byte)OptionExe,
                    Skill = Skill,
                    AccountId = _aid,
                    CharacterId = _cid,
                    VaultId = _vid,
                    Durability = _durability,
                    HarmonyOption = _jewelOfHarmony,
                    Option = _option,
                    Plus = _plus,
                    SlotId = _slot,
                    DateCreation = DateTime.Now,
                    SocketOptions = string.Join(",", _slots.Select(x => x.ToString()))
                };
                await db.Items.AddAsync(item);
                Serial = item.ItemId;
            }
            else
            {
                return;
            }

            log.Information("[A{2}:{3}{4}:S{5}]Item Saved:{0} {1}", item.Number, ToString(), item.AccountId, item.VaultId == 0 ? "C" : "V", item.VaultId == 0 ? item.CharacterId : item.VaultId, SlotId);

            NeedSave = false;
        }

        private void CalcItemAttributes()
        {
            var itemLevel = BasicInfo.Level;
            if (SetOption != 0)
                itemLevel += 25;

            if (BasicInfo.Str != 0)
                ReqStrength = (BasicInfo.Str * (itemLevel + Plus * 3) * 3) / 100 + 20;

            if (BasicInfo.Ene != 0)
                ReqEnergy = (BasicInfo.Ene * (itemLevel + Plus * 3) * 3) / 100 + 20;

            if (BasicInfo.Agi != 0)
                ReqAgility = (BasicInfo.Agi * (itemLevel + Plus * 3) * 3) / 100 + 20;

            if (BasicInfo.Vit != 0)
                ReqVitality = (BasicInfo.Vit * (itemLevel + Plus * 3) * 3) / 100 + 20;

            if (BasicInfo.Cmd != 0)
                ReqCommand = (BasicInfo.Cmd * (itemLevel + Plus * 3) * 3) / 100 + 20;

            AttackMax = BasicInfo.Damage.Y + Plus * 3;
            AttackMin = BasicInfo.Damage.X + Plus * 3;
            Defense = BasicInfo.Def + Plus * 3;
            DefenseRate = BasicInfo.DefRate + Plus * 3;

            //if(Number == ItemNumber.FromTypeIndex(13,5)) // Dark Spirit
            //{
            //    ReqCommand = 
            //}

            switch (Harmony.Type)
            {
                case 1:
                    switch(Harmony.Option)
                    {
                        case 3: //DECREASE_REQUIRE_STR
                            ReqStrength -= Harmony.EffectValue;
                            break;
                        case 4: //DECREASE_REQUIRE_DEX
                            ReqAgility -= Harmony.EffectValue;
                            break;
                    }
                    break;
                case 2:
                    switch (Harmony.Option)
                    {
                        case 2: //DECREASE_REQUIRE_STR
                            ReqStrength -= Harmony.EffectValue;
                            break;
                        case 3: //DECREASE_REQUIRE_DEX
                            ReqAgility -= Harmony.EffectValue;
                            break;
                    }
                    break;
            }


            if (Skill && BasicInfo.Skill != 0)
            {
                Spell = BasicInfo.Skill;
                if (Spell == Spell.ForceWave)
                {
                    Special.Add(0);
                }else
                {
                    Special.Add((SpecialNumber)Spell);
                }
            }
            else
            {
                Skill = false;
            }

            switch (Number)
            {
                // Dinorant
                case 13 * 512 + 3:
                    Skill = true;
                    Spell = Spell.FireBreath;
                    break;
                // DarkHorse
                case 13 * 512 + 4:
                    Skill = true;
                    Spell = Spell.Earthshake;
                    break;
                // Fenrir
                case 13 * 512 + 37:
                    Skill = true;
                    Spell = Spell.PlasmaStorm;
                    break;
                // Sahamut
                case 5 * 512 + 21:
                    Skill = true;
                    Spell = Spell.Sahamutt;
                    break;
                // Neil
                case 5 * 512 + 22:
                    Skill = true;
                    Spell = Spell.Neil;
                    break;
                // Ghost Phantom
                case 5 * 512 + 23:
                    Skill = true;
                    Spell = Spell.GhostPhantom;
                    break;
            }

            if (Luck)
            {
                if (Number.Type < ItemType.Wing_Orb_Seed)
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
                else if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index <= 6) // Wings
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
                else if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index >= 130 && Number.Index <= 135) // Wings
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
                else if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index >= 36 && Number.Index <= 43) // Wings S3
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
                else if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index == 50) // Wings S3
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
                else if (Number == ItemNumber.FromTypeIndex(13, 30) || Number == ItemNumber.FromTypeIndex(12, 49)) // Cape of Lord
                {
                    Special.Add(SpecialNumber.CriticalDamage);
                }
            }

            if (Option28 > 0)
            {
                if (Number.Type < ItemType.Staff)
                {
                    Special.Add(SpecialNumber.AditionalDamage);
                    ReqStrength += Option28 * 4;
                }
                else if (Number.Type >= ItemType.Staff && Number.Type < ItemType.Shield)
                {
                    Special.Add(SpecialNumber.AditionalMagic);
                    ReqStrength += Option28 * 4;
                }
                else if (Number.Type >= ItemType.Shield && Number.Type < ItemType.Helm)
                {
                    Special.Add(SpecialNumber.AditionalDefense);
                    ReqStrength += Option28 * 4;
                }
                else if (Number.Type >= ItemType.Helm && Number.Type < ItemType.Wing_Orb_Seed)
                {
                    Special.Add(SpecialNumber.AditionalDefense);
                    ReqStrength += Option28 * 4;
                }
                else if (Number == ItemNumber.FromTypeIndex(12, 0)) // Wing elf
                {
                    Special.Add(SpecialNumber.RecoverLife);
                }
                else if (Number == ItemNumber.FromTypeIndex(12, 1)) // Wing Heaven
                {
                    Special.Add(SpecialNumber.AditionalMagic);
                    ReqStrength += Option28 * 4;
                }
                else if (Number == ItemNumber.FromTypeIndex(12, 2)) // Wing devil
                {
                    Special.Add(SpecialNumber.AditionalDamage);
                    ReqStrength += Option28 * 4;
                }
                else if (Number == ItemNumber.FromTypeIndex(12, 3)) // Wing spitits
                {
                    Special.Add(SpecialNumber.AditionalDamage);
                    ReqStrength += Option28 * 4;
                }
            }

            Defense = BasicInfo.Def;

            if (Defense > 0)
            {
                if (Number.Type == ItemType.Shield)
                {
                    Defense += Plus;
                }
                else
                {
                    if (SetOption != 0)
                    {
                        Defense += (Defense * 12) / BasicInfo.Level + (BasicInfo.Level / 5) + 4;
                        //Defense += (Defense * 3) / ItemLevel + (ItemLevel / 30) + 2;
                    }
                    else if (OptionExe != 0)
                    {
                        Defense = (Defense * 12) / BasicInfo.Level + BasicInfo.Level / 5 + 4;
                    }

                    switch (Number)
                    {
                        case 12 * 512 + 3:
                        case 12 * 512 + 4:
                        case 12 * 512 + 5:
                        case 12 * 512 + 6:
                        case 13 * 512 + 30:
                        case 12 * 512 + 49:
                        case 13 * 512 + 4:
                        case 12 * 512 + 42:
                            Defense += Plus * 2;
                            break;
                        //Third Wings Defense * 4
                        case 12 * 512 + 36:
                        case 12 * 512 + 37:
                        case 12 * 512 + 38:
                        case 12 * 512 + 39:
                        case 12 * 512 + 40:
                        case 12 * 512 + 43:
                        case 12 * 512 + 50:
                            Defense += Plus * 4;
                            break;
                        default:
                            Defense += Plus * 3;
                            if (Plus >= 10)
                            {
                                Defense += (Plus - 9) * (Plus - 8) / 2;
                            }
                            break;
                    }

                    switch (Number)
                    {
                        case 13 * 512 + 30:
                        case 12 * 512 + 49:
                            Defense += Plus * 2 + 15;
                            if (Plus >= 10)
                            {
                                Defense += (Plus - 9) * (Plus - 8) / 2;
                            }
                            break;
                        //Wings S3 FIX EXC 1
                        case 12 * 512 + 36:
                        case 12 * 512 + 37:
                        case 12 * 512 + 38:
                        case 12 * 512 + 39:
                        case 12 * 512 + 40:
                        case 12 * 512 + 41:
                        case 12 * 512 + 42:
                        case 12 * 512 + 43:
                            if (Plus >= 10)
                            {
                                Defense += (Plus - 9) * (Plus - 8) / 2;
                            }
                            break;
                    }
                }
            }
        }

        private void OnItemChange()
        {
            CalcItemAttributes();

            Character?.Player.Session.SendAsync(new SInventoryItemSend
            {
                Pos = (byte)SlotId,
                ItemInfo = GetBytes()
            });
        }

        private void OnDurabilityChange(bool flag)
        {
            var p = new SInventoryItemDurSend
            {
                IPos = (byte)SlotId,
                Dur = _durability,
                Flag = (byte)(flag ? 1 : 0)
            };
            Character?.Player.Session.SendAsync(p);
        }

        public override string ToString()
        {
            return $"[{Number}]" + BasicInfo.Name + (Plus > 0 ? " +" + Plus.ToString() : "") + (Luck ? " +Luck" : "") + (Skill ? " +Skill" : "") + (Option28 > 0 ? " +Option" : "");
        }

        private int RepairItemPrice()
        {
            var baseDur = (float)BasicInfo.Durability;
            var currDur = (float)Durability;

            if (baseDur == 0)
                return 0;

            var basePrice = 0u;

            float fixFactor = 1.0f - currDur / baseDur;

            if (Number.Type == ItemType.Wing_Orb_Seed && (Number.Index == 4 || Number.Index == 5))
                basePrice = BuyPrice;
            else
                basePrice = BuyPrice / 3;

            if (basePrice > 400000000)
                basePrice = 400000000;

            if (basePrice >= 1000)
                basePrice = basePrice / 100 * 100;
            else if (basePrice >= 100)
                basePrice = basePrice / 10 * 10;

            var repairPrice = 3.0f * Math.Sqrt(basePrice) * Math.Sqrt(Math.Sqrt(basePrice));
            repairPrice *= fixFactor;
            repairPrice += 1.0f;

            if (repairPrice >= 1000)
                repairPrice = repairPrice / 100 * 100;
            else if (repairPrice >= 100)
                repairPrice = repairPrice / 10 * 10;

            return (int)repairPrice;
        }

        public byte GetLevel(int level)
        {
            ushort itemlevel;
            if (level < 0)
                level = 0;

            if (BasicInfo.Level == 0xffff || BasicInfo.Level == 0)
                return 0xff;

            if(Number.Type == ItemType.Potion)
            {
                itemlevel = BasicInfo.Level;

                if (Number.Index == 15)
                    return 0xff;

                if (itemlevel >= (level - 8))
                {
                    if (itemlevel <= level)
                        return 0;
                }

                return 0xff;
            }

            if (Number.Type == ItemType.Missellaneo && Number.Index == 10)
            {
                byte ilevel;

                if (_rand.Next(10) == 0)
                {
                    if (level < 0)
                        level = 0;

                    ilevel = (byte)(level / 10);

                    if (ilevel > 0)
                        ilevel--;

                    if (ilevel > 5)
                        ilevel = 5;

                    return ilevel;
                }

                return 0xff;
            }

            if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index == 11)
            {
                byte ilevel;

                if (_rand.Next(10) == 0)
                {
                    if (level < 0)
                        level = 0;

                    ilevel = (byte)(level / 10);

                    if (ilevel > 0)
                        ilevel--;

                    if (ilevel > 6)
                        ilevel = 6;

                    return ilevel;
                }

                return 0xff;
            }

            itemlevel = BasicInfo.Level;

            if (itemlevel >= level - 18 && itemlevel <= level)
            {
                if (Number.Type == ItemType.Scroll)
                    return 0;

                itemlevel = (byte)((level - itemlevel) / 3);

                if (Number.Type == ItemType.Missellaneo)
                {
                    if (Number.Index == 8 || Number.Index == 9 || Number.Index == 12 || Number.Index == 13 || Number.Index == 20 || Number.Index == 21 || Number.Index == 22 || Number.Index == 23 || Number.Index == 24 || Number.Index == 25 || Number.Index == 26 || Number.Index == 27 || Number.Index == 28)
                    {
                        if (itemlevel > 4)
                            itemlevel = 4;
                    }
                }

                return (byte)itemlevel;
            }
            return 0xff;
        }

        public int NormalWeaponDurabilityDown(int Defense)
        {
            if(Durability == 0)
            {
                return 0;
            }

            var div = BasicInfo.Damage.X * 1.5f;

            if(div == 0)
            {
                return 0;
            }

            var DurDecrease = Defense * 2 / div;

            _durabilityDown += DurDecrease;
            if(_durabilityDown > 564)
            {
                _durabilityDown = 0;
                if (Durability > 0)
                {
                    Durability -= 1;
                    return 2;
                }

                return 1;
            }

            return 0;
        }

        public int BowWeaponDurabilityDown(int Defense)
        {
            if (Durability == 0)
            {
                return 0;
            }

            var div = BasicInfo.Damage.X * 1.5f;

            if (div == 0)
            {
                return 0;
            }

            var DurDecrease = Defense * 2 / div;

            _durabilityDown += DurDecrease;
            if (_durabilityDown > 780)
            {
                _durabilityDown = 0;
                if (Durability > 0)
                {
                    Durability -= 1;
                    return 2;
                }

                return 1;
            }

            return 0;
        }

        public int ArmorDurabilityDown(int Attack)
        {
            if (Durability == 0)
            {
                return 0;
            }

            var div = Defense * 2;

            if (div == 0)
            {
                return 0;
            }

            var DurDecrease = Attack / div;

            _durabilityDown += DurDecrease;
            if (_durabilityDown > 69)
            {
                _durabilityDown = 0;
                if (Durability > 0)
                {
                    Durability -= 1;
                    return 2;
                }

                return 1;
            }

            return 0;
        }

        public object Clone()
        {
            return new Item(Number, 0, new { Plus, Luck, Skill, Durability, Option28, OptionExe });
        }

        public void NewOptionRand()
        {
            var randOp = _rand.Next(100);
            if (_rand.Next(6) == 0)
            {
                int NOption;
                NOption = 1 << _rand.Next(6);

                if ((NOption & 2) != 0)
                {
                    if (_rand.Next(2) != 0)
                    {
                        NOption = 1 << _rand.Next(6);
                    }
                }

                if (_rand.Next(4) == 0)
                {
                    NOption |= 1 << _rand.Next(6);
                }

                OptionExe = (byte)NOption;
            }

            if (((OptionExe & (byte)ExcellentOptionArmor.FullItem) != 0 && _rand.Next(100) == 0) || _rand.Next(6) == 0)
            {
                Luck = true;
            }
            else
            {
                Luck = false;
            }

            if (((OptionExe & (byte)ExcellentOptionArmor.FullItem) != 0 && _rand.Next(2) == 0) || _rand.Next(4) == 0 && Spell != Spell.None)
            {
                Skill = true;
            }
            else
            {
                Skill = false;
            }

            if (_rand.Next(randOp) == 0)
            {
                Option28 = (byte)_rand.Next(4);
            }
        }
    }
}
