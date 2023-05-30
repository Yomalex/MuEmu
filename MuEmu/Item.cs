using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MU.Network.Game;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MU.Resources;
using MuEmu.Util;

namespace MuEmu
{
    public enum ItemState
    {
        Created,
        CreatedAndChanged,
        Saved,
        SavedAndChanged,
        Deleting,
        Deleted,
    }
    public class Item : ICloneable
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Item));
        public static Dictionary<int, Item> s_ItemDB = new Dictionary<int, Item>();
        private byte _plus;
        private byte _durability;
        private byte _option;
        private StorageID _vid;
        private int _slot;
        private float _durabilityDown;
        private SocketOption[] _slots;
        private JewelOfHarmony _jewelOfHarmony = new JewelOfHarmony();
        private byte _petLevel;
        private ItemNumber _number;
        private Account _account;
        private Character _character;

        private ItemState _state;
        public ItemState State { get => _state; private set
            {
                _state = value;
            }
        }

        private void OnCreate()
        {
            State = ItemState.Created;
        }
        private void OnChange()
        {
            State = State switch
            {
                ItemState.Created => ItemState.CreatedAndChanged,
                ItemState.Saved => Serial==0? ItemState.CreatedAndChanged:ItemState.SavedAndChanged,
                _ => State,
            };
        }
        private void OnDelete()
        {
            State = State switch
            {
                ItemState.Created => ItemState.Deleted,
                ItemState.CreatedAndChanged => ItemState.Deleted,
                ItemState.Saved => ItemState.Deleting,
                ItemState.SavedAndChanged => ItemState.Deleting,
                _ => State,
            };
        }

        public Account Account { get => _account; set
            {
                if (_account == value)
                    return;
                _account = value;
                OnChange();
            }
        }
        public Character Character
        {
            get => _character; set
            {
                if (_character == value)
                    return;
                _character = value;
                OnChange();
            }
        }
        public StorageID Storage
        {
            get => _vid;
            set
            {
                if (_vid == value)
                    return;
                _vid = value;
                OnChange();
            }
        }
        public int SlotId
        {
            get => _slot;
            set
            {
                if (_slot == value)
                    return;
                _slot = value;
                OnChange();
            }
        }
        public bool IsZen => ItemNumber.Zen == Number;

        public ItemInfo BasicInfo { get; set; }
        public ItemNumber Number
        {
            get => _number;
            set
            {
                if (_number == value)
                    return;

                _number = value;
                OnChange();
                OnItemChange();
            }
        }
        public long Serial { get; private set; }
        public byte Plus
        {
            get => _plus;
            set
            {
                if (_plus == value)
                    return;
                _plus = value;

                OnChange();
                OnItemChange();
            }
        }
        public byte SmallPlus => (byte)(Plus > 0 ? (Plus - 1) / 2 : 0);
        public bool Luck { get; set; }
        public bool Skill { get; set; }
        public Spell Spell { get; set; }
        public byte Durability
        {
            get => _durability;
            set
            {
                if (_durability == value)
                    return;

                if (BasicInfo.MaxStack != 0)
                    value = Math.Min(value, BasicInfo.MaxStack);
                else if (DurabilityBase > 0)
                    value = Math.Min(DurabilityBase, value);

                _durability = value;
                OnDurabilityChange(false); 
                OnChange();
            }
        }
        public byte DurabilityBase => GetDurabilityBase();
        public byte Option28
        {
            get => _option;
            set
            {
                if (_option == value)
                    return;

                _option = value;
                OnChange();
                OnItemChange();
            }
        }
        public DateTime ExpireTime { get; set; } = DateTime.MinValue;
        public byte OptionExe { get; set; }

        public byte ExcellentCount => CountOfExcellent();
        public byte SetOption { get; set; }

        public bool Option380 { get => (OptionExe & 0x80) == 1; set => OptionExe = (byte)((OptionExe & 0x7F) | (value ? 0x80 : 0x00)); }
        public uint BuyPrice { get; set; }
        public uint SellPrice { get; set; }
        public int RepairPrice => RepairItemPrice();
        //public HarmonyOption Harmony { get; set; }
        public SocketOption[] Slots
        {
            get => _slots;
            set
            {
                _slots = value;
                OnChange();
                OnItemChange();
            }
        }
        public byte[] RankLevel
        {
            get => _slots.Select(a => (byte)(((byte)a & 0xF0) >> 4)).ToArray();
        }
        public byte[] RankOption
        {
            get => _slots.Select(a => (byte)((byte)a & 0x0F)).ToArray();
        }
        public List<SpecialNumber> Special { get; set; } = new List<SpecialNumber>();
        public JewelOfHarmony Harmony
        {
            get => _jewelOfHarmony;
            set
            {
                _jewelOfHarmony = value;
                OnChange();
            }
        }
        public byte BonusSocket { get; set; }

        public bool IsPentagramItem => (Number.Type == ItemType.Wing_Orb_Seed && (Number.Index >= 200 && Number.Index <= 220));
        public bool IsPentagramJewel => (Number.Type == ItemType.Wing_Orb_Seed && ((Number.Index >= 144 && Number.Index <= 145) || Number.Index == 148 || (Number.Index >= 221 && Number.Index <= 270)));
        public Element PentagramaMainAttribute { get => (Element)(BonusSocket & 0x0f); set => BonusSocket = (byte)(((byte)value) | (BonusSocket & 0xF0)); }
        public long[] PentagramJewels { get; set; } = new long[5];

        public int WingType => ((ushort)Number) switch
        {
            6144 => 1,
            6145 => 1,
            6146 => 1,
            6147 => 2,
            6148 => 2,
            6149 => 2,
            6150 => 2,
            6180 => 3,
            6181 => 3,
            6182 => 3,
            6183 => 3,
            6184 => 3,
            6185 => 1,
            6186 => 2,
            6187 => 3,
            6406 => 4,
            6407 => 4,
            6408 => 4,
            6409 => 4,
            6410 => 4,
            6411 => 4,
            _ => 0,
        };

        /// <summary>
        ///  Wing Damage Absorb in percent
        /// </summary>
        public float WingDmgAbsorb => WingType switch
        {
            1 => 0.12f + Plus * 0.02f,
            2 => 0.25f + Plus * 0.02f,
            3 => 0.39f + Plus * 0.02f,
            4 => 0.43f + Plus * 0.02f,
            _ => 0.0f,
        };

        /// <summary>
        /// Wing Damage increment in percent
        /// </summary>
        public float WingDmgAdd => WingType switch
        {
            1 => 0.12f + Plus * 0.02f,
            2 => 0.32f + Plus * 0.01f,
            3 => 0.39f + Plus * 0.02f,
            4 => 0.55f + Plus * 0.01f,
            _ => 0.0f,
        };

        public uint PShopValueZ { get; set; }
        public ushort PShopValueB { get; set; }
        public ushort PShopValueS { get; set; }
        public ushort PShopValueC { get; set; }

        // Needed Stats
        public int ReqStrength { get; private set; }
        public int ReqAgility { get; private set; }
        public int ReqVitality { get; private set; }
        public int ReqEnergy { get; private set; }
        public int ReqCommand { get; private set; }

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
        public int IncreaseWizardry => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseWizardry) != 0 ? (Character?.Level ?? 0) / 20 : 0) : 0;
        public float IncreaseWizardryRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseWizardryRate) != 0 ? 0.2f : 0) : 0;
        public float IncreaseLifeRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseLifeRate) != 0 ? 1.0f / 8.0f : 0) : 0;
        public float IncreaseManaRate => Number.Type <= ItemType.Shield ? ((((ExcellentOptionWeapons)OptionExe) & ExcellentOptionWeapons.IncreaseManaRate) != 0 ? 1.0f / 8.0f : 0) : 0;

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
        public long PetEXP { get; internal set; }
        public long PetNextEXP => GetExperienceFromLevel(_petLevel + 1);
        public byte PetLevel
        {
            get => _petLevel;
            internal set
            {
                _petLevel = value;
                if (Character == null)
                    return;

                AttackMin = _petLevel * 15 + Character.CommandTotal / 8 + 180;
                AttackMax = _petLevel * 15 + Character.CommandTotal / 4 + 200;
                AttackSpeed = _petLevel * 4 / 5 + Character.CommandTotal / 50 + 20;
                OnChange();
            }
        }

        public int AttackSpeed { get; internal set; }
        public bool IsMount => BasicInfo.IsMount;

        public static Item Zen(uint BuyPrice)
        {
            return new Item(ItemNumber.Zen, new { BuyPrice });
        }

        public Item(byte[] data)
        {
            using(var ms = new MemoryStream(data))
            {
                ItemNumber number = 0;
                number.Number = (ushort)ms.ReadByte();

                var tmp = ms.ReadByte();
                Skill = (tmp & 128) != 0;
                Luck = (tmp & 4) != 0;
                _option = (byte)(tmp & 3);
                _plus = (byte)((tmp >> 3) & 0xFF);

                _durability = (byte)ms.ReadByte();

                tmp = ms.ReadByte();
                OptionExe = (byte)(tmp & 0x3F);
                _option = (tmp & 0x40) != 0 ? (byte)4 : Option28;
                number.Number |= (ushort)((tmp << 1) & 0x100);

                SetOption = (byte)ms.ReadByte();

                tmp = ms.ReadByte();
                OptionExe |= (byte)((tmp << 4) & 0x80);
                number.Number |= (ushort)((tmp << 5) & 0x1E00);

                _number = number;
                var ItemDB = ResourceCache.Instance.GetItems();

                if (!ItemDB.ContainsKey(number))
                    throw new Exception("Item don't exists " + number);
                BasicInfo = ItemDB[number];

                tmp = ms.ReadByte();
                if (IsPentagramItem || IsPentagramJewel)
                    BonusSocket = (byte)tmp;
                else
                    Harmony = (byte)tmp;

                var l = new List<SocketOption>();
                tmp = ms.ReadByte();
                if (tmp != 0xff)
                    l.Add((SocketOption)tmp);
                tmp = ms.ReadByte();
                if (tmp != 0xff)
                    l.Add((SocketOption)tmp);
                tmp = ms.ReadByte();
                if (tmp != 0xff)
                    l.Add((SocketOption)tmp);
                tmp = ms.ReadByte();
                if (tmp != 0xff)
                    l.Add((SocketOption)tmp);
                tmp = ms.ReadByte();
                if (tmp != 0xff)
                    l.Add((SocketOption)tmp);

                Slots = l.ToArray();

                GetValue();
                CalcItemAttributes();
                State = ItemState.Created;
            }
        }

        public Item(ItemNumber number, object Options = null)
        {
            var ItemDB = ResourceCache.Instance.GetItems();

            if (!ItemDB.ContainsKey(number))
                throw new Exception("Item don't exists " + number);

            BasicInfo = ItemDB[number];
            _durability = BasicInfo.Durability;
            if (_durability == 0)
                _durability = 1;
            _slots = Array.Empty<SocketOption>();

            if (Options != null)
                Extensions.AnonymousMap(this, Options);

            Harmony.Item = this;

            Number = number;
            GetValue();
            CalcItemAttributes();
            State = ItemState.Created;
        }

        public Item(ItemDto dto, Account acc = null, Character @char = null)
        {
            var ItemDB = ResourceCache.Instance.GetItems();

            if (!ItemDB.ContainsKey(dto.Number))
                throw new Exception("Item don't exists " + dto.Number);

            _account = acc;
            _character = @char;
            BasicInfo = ItemDB[dto.Number];
            _slot = dto.SlotId;
            Serial = dto.ItemId;
            State = ItemState.Saved;
            Skill = dto.Skill;
            _number = dto.Number;
            _plus = dto.Plus;
            Luck = dto.Luck;
            _option = dto.Option;
            OptionExe = dto.OptionExe;
            _durability = dto.Durability;
            _vid = (StorageID)dto.VaultId;
            BonusSocket = dto.SocketBonus;
            PetEXP = dto.PetEXP;
            SetOption = dto.SetOption;
            _petLevel = dto.PetLevel;

            if (string.IsNullOrEmpty(dto.SocketOptions))
            {
                _slots = Array.Empty<SocketOption>();
            }
            else
            {
                var tmp = dto.SocketOptions.Split(",");
                _slots = tmp.Select(x => Enum.Parse<SocketOption>(x)).ToArray();
            }

            if (!string.IsNullOrEmpty(dto.PJewels))
            {
                var tmp = dto.PJewels.Split(",");
                PentagramJewels = tmp.Select(x => long.Parse(x)).ToArray();
            }

            _jewelOfHarmony = dto.HarmonyOption;
            Harmony.Item = this;

            GetValue();
            CalcItemAttributes();
            if (_durability == BasicInfo.MaxStack && BasicInfo.OnMaxStack != ItemNumber.Invalid)
            {
                Number = BasicInfo.OnMaxStack;
            }
            //OnChange();
        }

        internal void AddExperience(int gain)
        {
            byte type = 0;
            if (PetEXP < GetExperienceFromLevel(71))
                switch (Number)
                {
                    case 6660: //Dark Horse
                        PetEXP += gain;
                        type = 1;
                        break;
                    case 6661: //Dark Raven
                        PetEXP += gain;
                        break;
                }
            else
                PetEXP = GetExperienceFromLevel(71);

            if (PetEXP < 0)
                PetEXP = 0;

            var levelUp = false;
            while (PetEXP > PetNextEXP && PetLevel < 70)
            {
                PetLevel++;
                levelUp = true;
                OnChange();
            }

            if (levelUp)
                _ = Character.Player.Session.SendAsync(new SPetInfo
                {
                    Dur = Durability,
                    Exp = (int)PetEXP,
                    InvenType = 0xFE,
                    Level = PetLevel,
                    nPos = (byte)SlotId,
                    PetType = type,
                });
            else
            {
                _ = Character.Player.Session.SendAsync(new SPetInfo
                {
                    Dur = Durability,
                    Exp = (int)PetEXP,
                    InvenType = 0,
                    Level = PetLevel,
                    nPos = (byte)SlotId,
                    PetType = type,
                });
            }
        }

        internal long GetExperienceFromLevel(int level)
        {
            return ((long)100) * (level + 10) * level * level * level;
        }

        public byte[] GetBytes()
        {
            using (var ms = new MemoryStream(7 + 5))
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
                    ms.WriteByte((byte)(((Number & 0x100) >> 1) | (Option28 > 3 ? 0x40 : 0) | (byte)(OptionExe & 0x3f)));
                    ms.WriteByte(SetOption); // Acient Option

                    byte itemPeriod = 0;
                    if (ExpireTime != DateTime.MinValue)
                    {
                        itemPeriod |= 0x01;
                        itemPeriod |= (byte)((DateTime.Now > ExpireTime) ? 0x02 : 0x00);
                        itemPeriod <<= 1;
                    }

                    byte Option380 = (byte)((OptionExe & 0x80) >> 4);
                    byte LeftItemType = (byte)((Number & 0x1E00) >> 5);
                    byte complete = (byte)(LeftItemType | Option380 | itemPeriod);

                    ms.WriteByte(complete);
                    if (IsPentagramItem || IsPentagramJewel)
                    {
                        ms.WriteByte(BonusSocket);
                    }
                    else
                    {
                        ms.WriteByte(Harmony); // Harmony
                    }
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
                var level2 = BasicInfo.Level + Plus * 3;

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
                        switch (Plus)
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
                        Gold = 200000 + (Plus > 1 ? 20000 * (Plus - 1) : -150000);
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
                        Gold = Durability * 300;
                        break;
                    case 14 * 512 + 110: // 
                        Gold = Durability * 30000;
                        break;
                    case 14 * 512 + 111: // 
                        Gold = 600000;
                        break;
                    default:
                        if ((Number.Type == ItemType.Wing_Orb_Seed && ((Number.Index > 6 && Number.Index < 36))
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

                        if (Number.Type == ItemType.Wing_Orb_Seed && Number.Index <= 6) // Wings
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

                        if (Number.Type >= ItemType.Sword && Number.Type <= ItemType.Shield)
                        {
                            if (BasicInfo.Size.Width == 1)
                            {
                                Gold = Gold * 80 / 100;
                            }
                        }

                        foreach (var sp in Special)
                        {
                            switch (sp)
                            {
                                case (SpecialNumber)18:
                                case (SpecialNumber)19:
                                case (SpecialNumber)20:
                                case (SpecialNumber)21:
                                case (SpecialNumber)22:
                                case (SpecialNumber)23:
                                case (SpecialNumber)24:
                                case (SpecialNumber)56:
                                    Gold = (int)(Gold * 1.5f);
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

                if (BasicInfo.Zen > 0)
                {
                    Gold += (BasicInfo.Zen * BasicInfo.Zen * 10) / 12;

                    if (Number >= 14 * 512 + 0 && Number <= 14 * 512 + 8)
                    {
                        if ((int)Number == 14 * 512 + 3 || (int)Number == 14 * 512 + 6)
                        {
                            Gold *= 2;
                        }

                        if (Plus > 0)
                        {
                            Gold *= Plus * Plus; ;
                        }
                    }
                }

                BuyPrice = (uint)Gold;
                SellPrice = (uint)(Gold / 3.0);
            }
        }

        internal async Task<DateTimeOffset> Drop(byte mapX, byte mapY)
        {
            await Character.Player.Session
                .SendAsync(new SItemThrow { Source = (byte)SlotId, Result = 1 });

            var map = Character.Map;
            var output = map.AddItem(mapX, mapY, Clone() as Item, Character);
            //Delete();
            await Character?.Inventory.Delete(this);

            return output;
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

        public ItemDto Save(GameContext db)
        {
            var log = Logger;
            ItemDto _db = null;
            
            if(Account != null)
                log = Logger.ForAccount(Account.Player.Session);

            if (State == ItemState.Deleted) return null;

            if(State == ItemState.Saved || State == ItemState.SavedAndChanged || State == ItemState.Deleting)
                _db = db.Items.Find(Serial);
            else if (State == ItemState.Created || State == ItemState.CreatedAndChanged)
                _db = new ItemDto();

            if (State == ItemState.Saved)
                return _db;

            if(State == ItemState.Deleting)
            {
                log.Information($"[A{_db.AccountId}->{_vid}:{_slot}]Item Removed:{ToString()}" + " {0}", State);
                State = ItemState.Deleted;
                db.Remove(_db);
                return null;
            }

            _db.AccountId = Account?.ID ?? 0;
            _db.CharacterId = Character?.Id ?? 0;
            _db.VaultId = (int)_vid;
            _db.SlotId = _slot;
            _db.Number = Number;
            _db.Plus = _plus;
            _db.Luck = Luck;
            _db.Skill = Skill;
            _db.Option = Option28;
            _db.OptionExe = OptionExe;
            _db.HarmonyOption = Harmony;
            _db.SocketOptions = string.Join(",", _slots.Select(x => x.ToString()));
            _db.Durability = Durability;
            _db.PJewels = string.Join(",", PentagramJewels.Select(x => x.ToString()));
            _db.PetLevel = PetLevel;
            _db.PetEXP = PetEXP;
            _db.SetOption = SetOption;

            var str = $"[A{_db.AccountId}->{_vid}:{_slot}]Item Saved:{ToString()}";
            log.Information(str+" {0}", State);

            db.Update(_db);
            if(State == ItemState.Created || State == ItemState.CreatedAndChanged)
            {
                db.SaveChanges();
                Serial = _db.ItemId;
            }
            State = ItemState.Saved;
            return _db;
        }

        public void Delete()
        {
            var log = Logger;

            if (Account != null)
                log = Logger.ForAccount(Account.Player.Session);

            OnDelete();
            log.Information($"[A{Account?.ID??0}->{_vid}:{_slot}]Item Deleting:{ToString()}" + " {0}", State);
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
                    switch (Harmony.Option)
                    {
                        case 1:
                            AttackMin += Harmony.EffectValue;
                            break;
                        case 2:
                            AttackMax += Harmony.EffectValue;
                            break;
                        case 3: //DECREASE_REQUIRE_STR
                            ReqStrength -= Harmony.EffectValue;
                            break;
                        case 4: //DECREASE_REQUIRE_DEX
                            ReqAgility -= Harmony.EffectValue;
                            break;
                        case 5:
                            AttackMax += Harmony.EffectValue;
                            AttackMin += Harmony.EffectValue;
                            break;
                        case 6:
                            //CriticalDamage += Harmony.EffectValue;
                            break;
                        case 7:
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
                case 3:// Defense
                    switch (Harmony.Option)
                    {
                        case 1:
                            Defense += Harmony.EffectValue;
                            break;
                        case 2:
                            break;
                        case 3:
                            //IncreaseHP += Harmony.EffectValue;
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
                }
                else
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

        public void OnItemChange()
        {
            CalcItemAttributes();
            if (Character == null)
                return;

            var session = Character.Player.Session;

            //session?.SendAsync(new SInventoryItemDelete((byte)SlotId, 1));
            session?.Player.Character.Inventory.SendInventory();
            session?.Player.Character.Inventory.SendEventInventory();
        }

        private void OnDurabilityChange(bool flag)
        {
            if (Storage != StorageID.Inventory && Storage != StorageID.Equipament)
                return;

            var p = new SInventoryItemDurSend
            {
                IPos = (byte)SlotId,
                Dur = Durability,
                Flag = (byte)(flag ? 1 : 0)
            };
            Character?.Player.Session.SendAsync(p);

            OnChange();
        }

        public override string ToString()
        {
            return $"[{Serial}]" + BasicInfo.Name + (Plus > 0 ? " +" + Plus.ToString() : "") + (Luck ? " +Luck" : "") + (Skill ? " +Skill" : "") + (Option28 > 0 ? " +Option" : "") + (PentagramaMainAttribute != Element.None ? " (" + PentagramaMainAttribute.ToString() + ")" : "");
        }

        private byte CountOfExcellent()
        {
            byte count = 0;
            for (byte i = 0; i < 8; i++)
            {
                if (OptionExe.GetBit(i))
                {
                    count++;
                }
            }
            return count;
        }
        private int RepairItemPrice()
        {
            var baseDur = (float)DurabilityBase;
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

        private byte GetDurabilityBase()
        {
            var dur = BasicInfo.Durability + BasicInfo.MagicDur;
            if (Plus < 5)
            {
                dur += Plus;
            }
            else
            {
                switch (Plus)
                {
                    case 10:
                        dur += Plus * 2 - 3;
                        break;
                    case 11:
                        dur += Plus * 2 - 1;
                        break;
                    case 12:
                        dur += Plus * 2 + 2;
                        break;
                    case 13:
                        dur += Plus * 2 + 6;
                        break;
                    case 14:
                        dur += Plus * 2 + 9;
                        break;
                    case 15:
                        dur += Plus * 2 + 12;
                        break;
                    default:
                        dur += Plus * 2 - 4;
                        break;
                }
            }

            if (SetOption != 0)
                dur += 20;
            else if (OptionExe != 0)
                dur += 15;

            if (dur > 255)
                dur = 255;

            return (byte)dur;
        }

        public byte GetLevel(int level)
        {
            ushort itemlevel;
            if (level < 0)
                level = 0;

            if (BasicInfo.Level == 0xffff || BasicInfo.Level == 0)
                return 0xff;

            if (Number.Type == ItemType.Potion)
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

                if (Program.RandomProvider(10) == 0)
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

                if (Program.RandomProvider(10) == 0)
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
            if (_durabilityDown > 564)
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
            var it = new Item(Number, new { Plus, Luck, Skill, Durability, Option28, OptionExe });
            Extensions.AnonymousMap(it, this);
            it.State = ItemState.Created;
            it.Serial = 0;
            it.Durability = Durability;
            it.Character = null;
            it.Account = null;
            it.SlotId = 0;
            if (it.DurabilityBase == 0 && Durability == 0)
            {
                it.Durability = 1;
            }
            return it;
        }

        public void NewOptionRand()
        {
            var randOp = Program.RandomProvider(100);
            OptionExe = 0;
            Option28 = 0;
            if (Program.RandomProvider(6) == 0)
            {
                int NOption;
                NOption = 1 << Program.RandomProvider(6);

                if ((NOption & 2) != 0)
                {
                    if (Program.RandomProvider(2) != 0)
                    {
                        NOption = 1 << Program.RandomProvider(6);
                    }
                }

                if (Program.RandomProvider(4) == 0)
                {
                    NOption |= 1 << Program.RandomProvider(6);
                }

                OptionExe = (byte)NOption;
            }

            if (((OptionExe & (byte)ExcellentOptionArmor.FullItem) != 0 && Program.RandomProvider(100) == 0) || Program.RandomProvider(6) == 0)
            {
                Luck = true;
            }
            else
            {
                Luck = false;
            }

            if (((OptionExe & (byte)ExcellentOptionArmor.FullItem) != 0 && Program.RandomProvider(2) == 0) || Program.RandomProvider(4) == 0 && Spell != Spell.None)
            {
                Skill = BasicInfo.Skill != Spell.None;
            }
            else
            {
                Skill = false;
            }

            if (BasicInfo.Option && Program.RandomProvider(randOp) == 0)
            {
                Option28 = (byte)Program.RandomProvider(4);
            }

            if (
                (Number.Type == ItemType.Helm && Number.Number >= 3629) || // Helm S4
                (Number.Type == ItemType.Armor && Number.Number >= 4141) || // Armor S4
                (Number.Type == ItemType.Pant && Number.Number >= 4653) || // Pants S4
                (Number.Type == ItemType.Gloves && Number.Number >= 5165) || // Gloves S4
                (Number.Type == ItemType.Boots && Number.Number >= 5677) ||// Boots S4
                (Number.Number >= 26 && Number.Number <= 28) || // Swords S4
                Number.Number == 1040 || // Frost Mace S4
                Number.Number == 2071 || // Dark Stinger Bow S4
                (Number.Number >= 2590 && Number.Number <= 2592) || // Imperial Staff S4
                (Number.Type == ItemType.Shield && Number.Number >= 3089) // Shields S4
                )
            {
                var randSocketNumber = Program.RandomProvider(5, 1);
                Slots = new SocketOption[randSocketNumber];
                for (var i = 0; i < randSocketNumber; i++)
                {
                    Slots[i] = SocketOption.EmptySocket;
                }

                OptionExe = 0;
            }
        }

        public byte Overlap(byte count)
        {
            byte left = 0;
            var tmpDurability = _durability;
            if (tmpDurability + count <= BasicInfo.MaxStack)
            {
                tmpDurability += count;
            }
            else
            {
                tmpDurability = BasicInfo.MaxStack;
                left = (byte)(count + tmpDurability - BasicInfo.MaxStack);
            }
            if (tmpDurability == BasicInfo.MaxStack && BasicInfo.OnMaxStack != ItemNumber.Invalid)
            {
                _durability = 1;
                Number = BasicInfo.OnMaxStack;
            }
            else
            {
                Durability = tmpDurability;
            }
            return left;
        }
        public void Overlap(Item item)
        {
            if (item.Number != Number || item.Plus != Plus || _durability >= BasicInfo.MaxStack)
                throw new Exception($"Item {item} to {this} Can't be stacked {item.Number != Number} {item.Plus != Plus} {_durability >= BasicInfo.MaxStack}");

            item.Durability = Overlap(item.Durability);
            if (item.Durability == 0)
                Character?.Inventory.Delete(this).Wait();
                //item.Delete();
        }
    }
}
