using MuEmu.Monsters;
using MuEmu.Network.Data;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Game
{
    [WZContract(LongMessage = true, Serialized = true)]
    public class SInventory : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public InventoryDto[] Inventory { get; set; }

        public SInventory()
        {
            Inventory = Array.Empty<InventoryDto>();
        }

        public SInventory(InventoryDto[] inv)
        {
            Inventory = inv;
        }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SMuunInventory : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public InventoryDto[] Inventory { get; set; }

        public SMuunInventory()
        {
            Inventory = Array.Empty<InventoryDto>();
        }

        public SMuunInventory(InventoryDto[] inv)
        {
            Inventory = inv;
        }
    }

    [WZContract]
    public class SEquipament : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; }

        [WZMember(1, 18)]
        public byte[] CharSet { get; set; }

        public SEquipament()
        {
            CharSet = Array.Empty<byte>();
        }
    }

    [WZContract]
    public class SCheckSum : IGameMessage
    {
        [WZMember(0)]
        public byte Padding { get; set; }

        [WZMember(1)]
        public ushort Key { get; set; }
    }

    [WZContract]
    public class SMapMoveCheckSum:IGameMessage
    {
        [WZMember(0)] public uint key { get; set; }
    }

    [WZContract]
    public class SWeather : IGameMessage
    {
        [WZMember(0)]
        public byte Weather { get; set; }

        public SWeather() { }
        public SWeather(byte weather)
        {
            Weather = weather;
        }
    }

    [WZContract]
    public class SQuestInfo : IGameMessage
    {
        [WZMember(0)]
        public byte Count { get; set; }

        [WZMember(1, SerializerType = typeof(ArraySerializer))]
        public byte[] State { get; set; }
    }

    [WZContract]
    public class SSpells : IGameMessage
    {
        [WZMember(0)]
        public byte Count { get; set; }

        [WZMember(1)]
        public byte ListType { get; set; }

        [WZMember(2, SerializerType = typeof(ArraySerializer))]
        public SpellDto[] Spells { get; set; }

        public SSpells()
        {
            Spells = Array.Empty<SpellDto>();
        }

        public SSpells(byte listType, SpellDto[] spells)
        {
            Count = (byte)spells.Length;
            ListType = listType;
            Spells = spells;
        }

        public SSpells(byte listType, SpellDto spell)
        {
            Count = (byte)0xFE;
            ListType = listType;
            Spells = new SpellDto[] { spell };
        }
    }

    [WZContract(LongMessage = true)]
    public class SFriends : IGameMessage
    {
        [WZMember(0)]
        public byte MemoCount { get; set; }

        [WZMember(1)]
        public byte MailTotal { get; set; }

        [WZMember(4, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public FriendDto[] Friends { get; set; }

        public SFriends()
        {
            Friends = Array.Empty<FriendDto>();
            MailTotal = 50;
        }
    }

    [WZContract]
    public class SFriendReques : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] btName { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SLetter : IGameMessage
    {
        [WZMember(0)]
        public ushort Index { get; set; }

        [WZMember(1, 10)]
        public byte[] btSender { get; set; }

        [WZMember(2, 30)]
        public byte[] btDate { get; set; }

        [WZMember(3, 32)]
        public byte[] btSubject { get; set; }

        [WZMember(4)]
        public LetterStatus Status { get; set; }

        public string Sender { get => btSender.MakeString(); set => btSender = value.GetBytes(); }
        public DateTimeOffset Date { get => DateTimeOffset.Parse(btDate.MakeString()); set => btDate = value.ToString().GetBytes(); }
        public string Subject { get => btSubject.MakeString(); set => btSubject = value.GetBytes(); }
    }

    [WZContract]
    public class SKillCount : IGameMessage
    {
        [WZMember(0)]
        public byte KillCount { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortCreate : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public VPCreateDto[] ViewPort { get; set; }

        public SViewPortCreate()
        {
            ViewPort = Array.Empty<VPCreateDto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortCreateS9 : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public VPCreateS9Dto[] ViewPort { get; set; }

        public SViewPortCreateS9()
        {
            ViewPort = Array.Empty<VPCreateS9Dto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortChange : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPChangeDto[] ViewPort { get; set; }

        public SViewPortChange()
        {
            ViewPort = Array.Empty<VPChangeDto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortChangeS9 : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPChangeS9Dto[] ViewPort { get; set; }

        public SViewPortChangeS9()
        {
            ViewPort = Array.Empty<VPChangeS9Dto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortMonCreate : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateDto[] ViewPort { get; set; }

        public SViewPortMonCreate()
        {
            ViewPort = Array.Empty<VPMCreateDto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortMonCreateS9 : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateS9Dto[] ViewPort { get; set; }

        public SViewPortMonCreateS9()
        {
            ViewPort = Array.Empty<VPMCreateS9Dto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortItemCreate : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPICreateDto[] ViewPort { get; set; }

        public SViewPortItemCreate()
        {
            ViewPort = Array.Empty<VPICreateDto>();
        }

        public SViewPortItemCreate(VPICreateDto[] array)
        {
            ViewPort = array;
        }
    }

    [WZContract]
    public class SViewPortDestroy : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPDestroyDto[] ViewPort { get; set; }

        public SViewPortDestroy()
        {
            ViewPort = Array.Empty<VPDestroyDto>();
        }

        public SViewPortDestroy(VPDestroyDto[] VPDelete)
        {
            ViewPort = VPDelete;
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortItemDestroy : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPDestroyDto[] ViewPort { get; set; }
    }

    [WZContract]
    public class VPPShopDto
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1, 36)] public byte[] btName { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortPShop : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public VPPShopDto[] VPShops { get; set; }
    }

    [WZContract]
    public class SNotice : IGameMessage
    {
        [WZMember(0)]
        public NoticeType type { get; set; }//3

        [WZMember(1)]
        public byte btCount { get; set; }//4

        [WZMember(2)]
        public byte Padding { get; set; }//5

        [WZMember(3)]
        public ushort wDelay { get; set; }//6,7

        [WZMember(4)]
        public int dwColor { get; set; }//8,9,A,B

        [WZMember(5)]
        public byte btSpeed { get; set; }//C

        [WZMember(6, SerializerType = typeof(ArraySerializer))]
        public byte[] btNotice { get; set; } // D,D+(1-256)

        [WZMember(7)]
        public byte nullTerm { get; set; }//C

        public SNotice()
        {
            btNotice = Array.Empty<byte>();
        }

        public SNotice(NoticeType _type, string text)
        {
            type = _type;
            Notice = text;
        }

        public string Notice
        {
            get => btNotice.MakeString();
            set => btNotice = value.GetBytes();
        }
    }

    [WZContract]
    public class SEventState : IGameMessage
    {
        [WZMember(0)]
        public byte State { get; set; }

        [WZMember(1)]
        public MapEvents Event { get; set; }

        public SEventState()
        { }

        public SEventState(MapEvents @event, bool running)
        {
            State = (byte)(running ? 0x01 : 0x00);
            Event = @event;
        }
    }

    [WZContract]
    public class SNewQuestInfo : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public NewQuestInfoDto[] QuestList { get; set; }

        public SNewQuestInfo()
        {
            QuestList = Array.Empty<NewQuestInfoDto>();
        }
    }

    [WZContract]
    public class SHeatlUpdate : IGameMessage
    {
        [WZMember(0)] public RefillInfo Pos { get; set; }

        [WZMember(1)] public ushort HP { get; set; }

        [WZMember(2)] public byte Flag { get; set; }

        [WZMember(3)] public ushort SD { get; set; }

        public ushort Health { get => HP.ShufleEnding(); set => HP = value.ShufleEnding(); }

        public ushort Shield { get => SD.ShufleEnding(); set => SD = value.ShufleEnding(); }

        public SHeatlUpdate()
        { }

        public SHeatlUpdate(RefillInfo pos, ushort hp, ushort sd, bool flag)
        {
            Pos = pos;
            Health = hp;
            Shield = sd;
            Flag = 0;
        }
    }

    [WZContract]
    public class SManaUpdate : IGameMessage
    {
        [WZMember(0)] public RefillInfo Pos { get; set; }

        [WZMember(1)] public ushort MP { get; set; }

        [WZMember(2)] public ushort BP { get; set; }

        public ushort Mana { get => MP.ShufleEnding(); set => MP = value.ShufleEnding(); }

        public ushort Stamina { get => BP.ShufleEnding(); set => BP = value.ShufleEnding(); }

        public SManaUpdate()
        { }

        public SManaUpdate(RefillInfo pos, ushort mana, ushort bp/*, bool flag*/)
        {
            Pos = pos;
            Mana = mana;
            Stamina = bp;
        }
    }

    [WZContract]
    public class SSkillKey : IGameMessage
    {
        [WZMember(0, 20)]
        public byte[] SkillKey { get; set; }

        [WZMember(1)]
        public byte GameOption { get; set; }

        [WZMember(2)]
        public byte Q_Key { get; set; }

        [WZMember(3)]
        public byte W_Key { get; set; }

        [WZMember(4)]
        public byte E_Key { get; set; }

        [WZMember(5)]
        public byte ChatWindow { get; set; }

        [WZMember(6)]
        public byte R_Key { get; set; }

        public SSkillKey()
        {
            SkillKey = new byte[20];
            for (var i = 0; i < 20; i++)
                SkillKey[i] = 0xFF;
        }
    }

    [WZContract]
    public class SAction : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; }   // 3,4

        [WZMember(1)]
        public byte Dir { get; set; }  // 5

        [WZMember(2)]
        public byte ActionNumber { get; set; }  // 6

        [WZMember(3)]
        public ushort Target { get; set; } // 7,8

        public SAction()
        { }

        public SAction(ushort number, byte dir, byte action, ushort target)
        {
            Number = number.ShufleEnding();
            Dir = dir;
            ActionNumber = action;
            Target = target.ShufleEnding();
        }
    }

    [WZContract]
    public class SMove : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }   // 3,4

        [WZMember(1)]
        public byte X { get; set; } // 5

        [WZMember(2)]
        public byte Y { get; set; } // 6

        [WZMember(3)]
        public byte Path { get; set; }	// 7

        public SMove()
        { }

        public SMove(ushort number, byte x, byte y, byte path)
        {
            wzNumber = number.ShufleEnding();
            X = x;
            Y = y;
            Path = path;
        }
    }

    [WZContract]
    public class SPositionSet : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public byte X { get; set; }

        [WZMember(2)]
        public byte Y { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }

        public SPositionSet() { }

        public SPositionSet(ushort number, Point pos)
        {
            Number = number;
            X = (byte)pos.X;
            Y = (byte)pos.Y;
        }
    }

    [WZContract]
    public class SPointAdd : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte Padding { get; set; }

        [WZMember(2)]
        public ushort MaxLifeAndMana { get; set; }

        [WZMember(3)]
        public ushort MaxShield { get; set; }

        [WZMember(4)]
        public ushort MaxStamina { get; set; }
    }

    [WZContract]
    public class SLevelUp : IGameMessage
    {
        [WZMember(0)]
        public ushort Level { get; set; }

        [WZMember(1)]
        public ushort LevelUpPoints { get; set; }

        [WZMember(2)]
        public ushort MaxLife { get; set; }

        [WZMember(3)]
        public ushort MaxMana { get; set; }

        [WZMember(4)]
        public ushort MaxShield { get; set; }

        [WZMember(5)]
        public ushort MaxBP { get; set; }

        [WZMember(6)]
        public ushort AddPoint { get; set; }

        [WZMember(7)]
        public ushort MaxAddPoint { get; set; }

        [WZMember(8)]
        public ushort MinusPoint { get; set; }

        [WZMember(9)]
        public ushort MaxMinusPoint { get; set; }
    }

    [WZContract]
    public class SClinetClose : IGameMessage
    {
        [WZMember(0)]
        public ClientCloseType Type { get; set; }
    }

    [WZContract(Serialized = true)]
    public class SMoveItem : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte Position { get; set; }

        [WZMember(2, 12)]
        public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class SEventEnterCount : IGameMessage
    {
        [WZMember(0)]
        public EventEnterType Type { get; set; }

        [WZMember(1)]
        public byte Left { get; set; }
    }

    [WZContract(Serialized = true)]
    public class SCloseMsg : IGameMessage
    {
        [WZMember(0)]
        public ClientCloseType Type { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SShopItemList : IGameMessage
    {
        [WZMember(0)]
        public byte ListType { get; set; }

        [WZMember(1, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public InventoryDto[] Inventory { get; set; }

        public SShopItemList()
        {
            Inventory = Array.Empty<InventoryDto>();
        }

        public SShopItemList(InventoryDto[] inv)
        {
            Inventory = inv;
        }
    }

    [WZContract(Serialized = true)]
    public class STalk : IGameMessage
    {
        [WZMember(0)]
        public NPCWindow Result { get; set; }

        [WZMember(1)]
        public byte Level1 { get; set; }

        [WZMember(2)]
        public byte Level2 { get; set; }

        [WZMember(3)]
        public byte Level3 { get; set; }

        [WZMember(4)]
        public byte Level4 { get; set; }

        [WZMember(5)]
        public byte Level5 { get; set; }

        [WZMember(6)]
        public byte Level6 { get; set; }

        [WZMember(7)]
        public byte Level7 { get; set; }
    }

    [WZContract]
    public class STax : IGameMessage
    {
        [WZMember(0)]
        public TaxType Type { get; set; }

        [WZMember(1)]
        public byte Rate { get; set; }
    }

    [WZContract]
    public class SWarehouseMoney : IGameMessage
    {
        /// <summary>
        /// Warehouse Money
        /// </summary>
        [WZMember(0)]
        public int wMoney { get; set; }

        /// <summary>
        /// Inventory Money
        /// </summary>
        [WZMember(1)]
        public uint iMoney { get; set; }

        public SWarehouseMoney()
        { }

        public SWarehouseMoney(int _wMoney, uint _iMoney)
        {
            wMoney = _wMoney;
            iMoney = _iMoney;
        }
    }

    [WZContract(Serialized = true)]
    public class SQuestWindow : IGameMessage
    {
        [WZMember(0)]
        public byte Type { get; set; }

        [WZMember(1)]
        public byte SubType { get; set; }

        [WZMember(2, 6)]
        public byte[] Unknow { get; set; }

        public SQuestWindow()
        {
            Unknow = Array.Empty<byte>();
        }
    }

    [WZContract]
    public class SCommand : IGameMessage
    {
        [WZMember(0)]
        public ServerCommandType Type { get; set; }

        [WZMember(1)]
        public byte Arg1 { get; set; }

        [WZMember(2)]
        public byte Arg2 { get; set; }

        public SCommand() { }

        public SCommand(ServerCommandType type, params byte[] args)
        {
            Type = type;
            if (args.Length > 0)
                Arg1 = args[0];
            if (args.Length > 1)
                Arg1 = args[1];
        }
    }

    [WZContract]
    public class SBuy : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class SSell : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public uint Money { get; set; }
    }

    [WZContract]
    public class SItemGet : IGameMessage
    {
        /// <summary>
        /// 0xFE: Zen
        /// </summary>
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }

        public uint Money { get => BitConverter.ToUInt32(ItemInfo, 0).ShufleEnding(); set => ItemInfo = BitConverter.GetBytes(value.ShufleEnding()); }

        public SItemGet()
        {
            ItemInfo = Array.Empty<byte>();
        }
    }

    [WZContract]
    public class SChatNickName : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] NickName { get; set; }

        [WZMember(1, typeof(ArraySerializer))]
        public byte[] Message { get; set; }

        [WZMember(2)]
        public byte NullTerminator { get; set; }

        public SChatNickName()
        {
            NickName = Array.Empty<byte>();
            Message = Array.Empty<byte>();
        }

        public SChatNickName(string Target, string message)
        {
            NickName = Target.GetBytes();
            Message = message.GetBytes();
        }
    }

    [WZContract]
    public class SChatTarget : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; }

        [WZMember(1, typeof(ArraySerializer))]
        public byte[] Message { get; set; }

        [WZMember(2)]
        public byte NullTerminator { get; set; }

        public SChatTarget()
        {
            Message = Array.Empty<byte>();
        }

        public SChatTarget(ushort Target, string message)
        {
            Number = Target.ShufleEnding();
            Message = message.GetBytes();
        }
    }

    [WZContract(Serialized = true)]
    public class STeleport : IGameMessage
    {
        // C3:1C
        [WZMember(0)]
        public byte Unk { get; set; }

        [WZMember(1)]
        public ushort Type { get; set; }

        [WZMember(2)]
        public Maps Map { get; set; } // 4

        [WZMember(3)]
        public byte MapX { get; set; }  // 5

        [WZMember(4)]
        public byte MapY { get; set; }  // 6

        [WZMember(5)]
        public byte Dir { get; set; }   // 7

        public STeleport()
        { }

        public STeleport(ushort type, Maps map, Point position, byte dir)
        {
            Type = type;
            Map = map;
            MapX = (byte)position.X;
            MapY = (byte)position.Y;
            Dir = dir;
        }
    };

    [WZContract]
    public class SViewSkillState : IGameMessage
    {
        [WZMember(0)]
        public byte State { get; set; }

        [WZMember(1)]
        public ushort Number { get; set; }

        [WZMember(2)]
        public byte SkillIndex { get; set; }

        public SViewSkillState() { }

        public SViewSkillState(byte state, ushort number, byte skillIndex)
        {
            State = state;
            Number = number.ShufleEnding();
            SkillIndex = skillIndex;
        }
    }

    [WZContract]
    public class SInventoryItemDelete : IGameMessage
    {
        [WZMember(0)] public byte IPos { get; set; }    // 3
        [WZMember(1)] public byte Flag { get; set; }    // 4

        public SInventoryItemDelete() { }
        public SInventoryItemDelete(byte pos, byte flag)
        {
            IPos = pos;
            Flag = flag;
        }
    }

    [WZContract]
    public class SInventoryItemSend : IGameMessage
    {
        [WZMember(0)]
        public byte Pos { get; set; }   // 4

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }	// 5
    }

    [WZContract]
    public class SInventoryItemDurSend : IGameMessage
    {
        [WZMember(0)]
        public byte IPos { get; set; }  // 3

        [WZMember(1)]
        public byte Dur { get; set; }   // 4

        [WZMember(2)]
        public byte Flag { get; set; }	// 5
    }

    [WZContract]
    public class SJewelMix : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }    // 3

        public SJewelMix() { }

        public SJewelMix(byte result)
        {
            Result = result;
        }
    }

    [WZContract]
    public class SSetMapAttribute : IGameMessage
    {
        [WZMember(0)]
        public byte Type { get; set; }

        [WZMember(1)]
        public MapAttributes MapAtt { get; set; }

        [WZMember(2)]
        public byte MapSetType { get; set; }

        [WZMember(3, typeof(ArrayWithScalarSerializer<byte>))]
        //public byte Count { get; set; }
        public MapRectDto[] Changes { get; set; }


        public SSetMapAttribute() { Changes = Array.Empty<MapRectDto>(); }

        public SSetMapAttribute(byte type, MapAttributes att, byte setType, MapRectDto[] changes)
        {
            Type = type;
            MapAtt = att;
            MapSetType = setType;
            Changes = changes;
        }
    }

    [WZContract]
    public class MapRectDto
    {
        [WZMember(0)]
        public byte StartX { get; set; }

        [WZMember(1)]
        public byte StartY { get; set; }

        [WZMember(2)]
        public byte EndX { get; set; }

        [WZMember(3)]
        public byte EndY { get; set; }
    }

    [WZContract]
    public class SItemThrow : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte Source { get; set; }
    }

    [WZContract]
    public class SChaosBoxItemMixButtonClick : IGameMessage
    {
        [WZMember(0)]
        public ChaosBoxMixResult Result { get; set; }

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class SDamage : IGameMessage
    {
        [WZMember(0)]
        public ushort wzDamage { get; set; }

        public ushort Damage { get => wzDamage.ShufleEnding(); set => wzDamage = value.ShufleEnding(); }

        public SDamage() { }

        public SDamage(ushort dmg)
        {
            Damage = dmg;
        }
    }

    [WZContract(Serialized = true)]
    public class SKillPlayer : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public ushort wzExp { get; set; }

        [WZMember(2)]
        public ushort wzDamage { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
        public ushort Exp { get => wzExp.ShufleEnding(); set => wzExp = value.ShufleEnding(); }
        public ushort Damage { get => wzDamage.ShufleEnding(); set => wzDamage = value.ShufleEnding(); }

        public SKillPlayer() { }

        public SKillPlayer(ushort number, ushort exp, ushort dmg)
        {
            Number = (ushort)(number | 0x80);
            Exp = exp;
            Damage = dmg;
        }
    }

    [WZContract]
    public class SAttackResult : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public ushort wzDamage { get; set; }

        [WZMember(2)]
        public DamageType DamageType { get; set; }

        [WZMember(3)]
        public ushort wzDamageShield { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
        public ushort Damage { get => wzDamage.ShufleEnding(); set => wzDamage = value.ShufleEnding(); }
        public ushort DamageShield { get => wzDamageShield.ShufleEnding(); set => wzDamageShield = value.ShufleEnding(); }

        public SAttackResult() { }

        public SAttackResult(ushort number, ushort dmg, DamageType dmgType, ushort dmgShield)
        {
            Number = number;
            Damage = dmg;
            DamageShield = dmgShield;
            DamageType = dmgType;
        }
    }

    [WZContract(Serialized = true)]
    public class SMagicAttack : IGameMessage
    {
        [WZMember(0)]
        public ushort wzMagicNumber { get; set; }

        [WZMember(1)]
        public ushort wzSource { get; set; }

        [WZMember(2)]
        public ushort wzTarget { get; set; }

        public SMagicAttack() { }

        public SMagicAttack(Spell magic, ushort source, ushort target)
        {
            wzMagicNumber = ((ushort)magic).ShufleEnding();
            wzSource = source.ShufleEnding();
            wzTarget = target.ShufleEnding();
        }
    }

    [WZContract(Serialized = true)]
    public class SMagicDuration : IGameMessage
    {
        [WZMember(0)]
        public ushort MagicNumber { get; set; }

        [WZMember(1)]
        public ushort wzNumber { get; set; }

        [WZMember(2)]
        public byte X { get; set; }

        [WZMember(3)]
        public byte Y { get; set; }

        [WZMember(4)]
        public byte Dis { get; set; }

        public SMagicDuration() { }

        public SMagicDuration(Spell magic, ushort Number, byte x, byte y, byte dis)
        {
            MagicNumber = ((ushort)magic).ShufleEnding();
            wzNumber = Number.ShufleEnding();
            X = x;
            Y = y;
            Dis = dis;
        }
    }

    [WZContract]
    public class SDiePlayer : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public byte Skill { get; set; }

        [WZMember(2)]
        public ushort wzKiller { get; set; }

        public SDiePlayer() { }
        public SDiePlayer(ushort number, byte skill, ushort killer)
        {
            wzNumber = number.ShufleEnding();
            Skill = skill;
            wzKiller = killer.ShufleEnding();
        }
    }

    [WZContract]
    public class SEffect : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1)] public ClientEffect Effect { get; set; }

        public SEffect()
        {

        }

        public SEffect(ushort Target, ClientEffect effect)
        {
            wzNumber = Target.ShufleEnding();
            Effect = effect;
        }
    }

    [WZContract]
    public class SItemModify : IGameMessage
    {
        [WZMember(0)] public byte Padding { get; set; }

        [WZMember(1)] public int Money { get; set; }
    }

    [WZContract]
    public class SItemUseSpecialTime : IGameMessage
    {
        [WZMember(0)] public byte Number { get; set; }
        [WZMember(1)] public ushort Time { get; set; }
    }

    //0xC1 0x40
    // SAME CPartyRequest

    //0xC1 0x41
    [WZContract]
    public class SPartyResult : IGameMessage
    {
        [WZMember(0)]
        public PartyResults Result { get; set; }

        public SPartyResult() { }

        public SPartyResult(PartyResults res)
        {
            Result = res;
        }
    }

    //0xC1 0x42
    [WZContract]
    public class SPartyList : IGameMessage
    {
        [WZMember(0)]
        public PartyResults Result { get; set; }

        [WZMember(1, typeof(ArrayWithScalarSerializer<byte>))]
        public PartyDto[] PartyMembers { get; set; }

        public SPartyList()
        {
            PartyMembers = Array.Empty<PartyDto>();
        }
    }

    [WZContract]
    public class SPartyDelUser : IGameMessage
    {
    }

    [WZContract]
    public class SPartyLife
    {
        [WZMember(0)] public byte Life { get; set; }
        [WZMember(1)] public byte Mana { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }
        [WZMember(3)] public byte Channel { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SPartyLifeAll : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public SPartyLife[] PartyLives { get; set; }

        public SPartyLifeAll()
        {
            PartyLives = Array.Empty<SPartyLife>();
        }
    }

    [WZContract]
    public class SCharRegen : IGameMessage
    {
        [WZMember(0)]
        public byte MapX { get; set; }
        /*<thisrel this+0x5>*/ /*|0x1|*/
        [WZMember(1)]
        public byte MapY { get; set; }
        /*<thisrel this+0x6>*/ /*|0x1|*/
        [WZMember(2)]
        public Maps MapNumber { get; set; }
        /*<thisrel this+0x7>*/ /*|0x1|*/
        [WZMember(3)]
        public byte Dir { get; set; }
        /*<thisrel this+0x8>*/ /*|0x2|*/
        [WZMember(4)]
        public ushort Life { get; set; }
        /*<thisrel this+0xa>*/ /*|0x2|*/
        [WZMember(5)]
        public ushort Mana { get; set; }
        /*<thisrel this+0xc>*/ /*|0x2|*/
        [WZMember(6)]
        public ushort wShield { get; set; }
        /*<thisrel this+0xe>*/ /*|0x2|*/
        [WZMember(7)]
        public ushort BP { get; set; }
        /*<thisrel this+0x10>*/ /*|0x4|*/ //unsigned long Exp;
        [WZMember(8)]
        public ulong unk1 { get; set; }
        /*<thisrel this+0x10>*/ /*|0x4|*/
        [WZMember(9)]
        public uint Exp { get; set; }
        /*<thisrel this+0x14>*/ /*|0x4|*/
        [WZMember(10)]
        public ulong Money { get; set; }

        public SCharRegen()
        { }
        public SCharRegen(Maps map, byte x, byte y, byte dir, ushort life, ushort mana, ushort shield, ushort bp, uint exp, ulong money)
        {
            MapNumber = map;
            MapX = x;
            MapY = y;
            Dir = dir;
            Life = life.ShufleEnding();
            Mana = mana.ShufleEnding();
            wShield = shield.ShufleEnding();
            BP = bp.ShufleEnding();
            Exp = exp.ShufleEnding();
            Money = money.ShufleEnding();
        }
    }

    // 0xC1 0xAA 0x01
    [WZContract]
    public class SDuelAnsDuelInvite : IGameMessage
    {
        [WZMember(0)] public DuelResults Result { get; set; }
        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }

        public SDuelAnsDuelInvite() { }
        public SDuelAnsDuelInvite(DuelResults result, ushort number, string name)
        {
            Result = result;
            wzNumber = number.ShufleEnding();
            btName = name.GetBytes();
        }
    }

    // 0xC1 0xAA 0x02
    [WZContract]
    public class SDuelAnswerReq : CDuelRequest
    {
        public SDuelAnswerReq() { }
        public SDuelAnswerReq(ushort number, string name)
        {
            wzNumber = number.ShufleEnding();
            btName = name.GetBytes();
        }
    }

    // 0xC1 0xAA 0x03
    [WZContract]
    public class SDuelAnsExit : SDuelAnsDuelInvite
    {
        public SDuelAnsExit() { }
        public SDuelAnsExit(DuelResults results)
        {
            Result = results;
        }
        public SDuelAnsExit(DuelResults results, ushort id, string name)
        {
            Result = results;
            wzNumber = id.ShufleEnding();
            btName = name.GetBytes();
        }
    }

    // 0xC1 0xAA 0x04
    [WZContract]
    public class SDuelBroadcastScore : IGameMessage
    {
        [WZMember(0)] public ushort wzChallenger { get; set; }
        [WZMember(1)] public ushort wzChallenged { get; set; }
        [WZMember(2)] public byte ChallengerScore { get; set; }
        [WZMember(3)] public byte ChallengedScore { get; set; }

        public SDuelBroadcastScore() { }
        public SDuelBroadcastScore(ushort challenger, ushort challenged, byte challengerScore, byte challengedScore)
        {
            wzChallenger = challenger.ShufleEnding();
            wzChallenged = challenged.ShufleEnding();
            ChallengerScore = challengerScore;
            ChallengedScore = challengedScore;
        }
    }

    // 0xC1 0xAA 0x05
    [WZContract]
    public class SDuelBroadcastHP : IGameMessage
    {
        [WZMember(0)] public ushort wzChallenger { get; set; }
        [WZMember(1)] public ushort wzChallenged { get; set; }
        [WZMember(2)] public byte ChallengerHP { get; set; }
        [WZMember(3)] public byte ChallengedHP { get; set; }
        [WZMember(4)] public byte ChallengerShield { get; set; }
        [WZMember(5)] public byte ChallengedShield { get; set; }

        public SDuelBroadcastHP() { }
        public SDuelBroadcastHP(ushort challenger, ushort challenged, byte challengerHP, byte challengedHP, byte challengerShield, byte challengedShield)
        {
            wzChallenger = challenger.ShufleEnding();
            wzChallenged = challenged.ShufleEnding();
            ChallengerHP = challengerHP;
            ChallengedHP = challengedHP;
            ChallengerShield = challengerShield;
            ChallengedShield = challengedShield;
        }
    }

    [WZContract]
    public class DuelChannel
    {
        [WZMember(0, 10)]
        public byte[] btNameA { get; set; }
        [WZMember(1, 10)]
        public byte[] btNameB { get; set; }
        [WZMember(2)]
        public byte bStart { get; set; }
        [WZMember(3, 10)]
        public byte bWatch { get; set; }

        public DuelChannel()
        {
            btNameA = Array.Empty<byte>();
            btNameB = Array.Empty<byte>();
        }

        public DuelChannel(string nameA, string nameB, bool start, bool watch)
        {
            btNameA = nameA.GetBytes();
            btNameB = nameB.GetBytes();
            bStart = (byte)(start ? 1 : 0);
            bWatch = (byte)(watch ? 1 : 0);
        }
    }

    // 0xC1 0xAA 0x06
    [WZContract]
    public class SDuelChannelList : IGameMessage
    {
        [WZMember(0, typeof(ArraySerializer))]
        public DuelChannel[] Channels { get; set; }

        public SDuelChannelList()
        {
            Channels = new DuelChannel[4];
        }
        public SDuelChannelList(DuelChannel[] channels)
        {
            if (channels.Length != 4)
                throw new Exception("Channels != 4");

            Channels = channels;
        }
    }

    // 0xC1 0xAA 0x07
    [WZContract]
    public class SDuelRoomJoin : IGameMessage
    {
        [WZMember(0)]
        public DuelResults Results { get; set; }
        [WZMember(1)]
        public byte Room { get; set; }
        [WZMember(2,10)]
        public byte[] btChallenger { get; set; }
        [WZMember(3,10)]
        public byte[] btChallenged { get; set; }
        [WZMember(4)]
        public ushort wzChallenger { get; set; }
        [WZMember(5)]
        public ushort wzChallenged { get; set; }

        public SDuelRoomJoin() { }
        public SDuelRoomJoin(DuelResults result, byte room, string challengerName, string challengedName, ushort challengerId, ushort challengedId)
        {
            Results = result;
            Room = room;
            btChallenged = challengedName.GetBytes();
            btChallenger = challengerName.GetBytes();
            wzChallenged = challengedId.ShufleEnding();
            wzChallenger = challengerId.ShufleEnding();
        }
    }

    // 0xC1 0xAA 0x08
    [WZContract]
    public class SDuelRoomBroadcastJoin : IGameMessage
    {
        [WZMember(0,10)]
        public byte[] btObserver { get; set; }

        public SDuelRoomBroadcastJoin() { }
        public SDuelRoomBroadcastJoin(string observer)
        {
            btObserver = observer.GetBytes();
        }
    }

    // 0xC1 0xAA 0x09
    [WZContract]
    public class SDuelRoomLeave : IGameMessage
    {
        [WZMember(0)]
        public DuelResults Results { get; set; }

        public SDuelRoomLeave() { }
        public SDuelRoomLeave(DuelResults result)
        {
            Results = result;
        }
    }

    // 0xC1 0xAA 0x0A
    [WZContract]
    public class SDuelRoomBroadcastLeave : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] btObserver { get; set; }

        public SDuelRoomBroadcastLeave() { }
        public SDuelRoomBroadcastLeave(string observer)
        {
            btObserver = observer.GetBytes();
        }
    }

    [WZContract]
    public class SDuelRoomObserver
    {
        [WZMember(0, 10)]
        public byte[] btObserver { get; set; }

        public SDuelRoomObserver()
        {
            btObserver = Array.Empty<byte>();
        }
    }

    // 0xC1 0xAA 0x0B
    [WZContract]
    public class SDuelRoomBroadcastObservers : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public SDuelRoomObserver[] Observers { get; set; }
        public SDuelRoomBroadcastObservers()
        {
            Array.Empty<SDuelRoomObserver>();
        }
        public SDuelRoomBroadcastObservers(string[] observer)
        {
            Observers = observer.Select(x => new SDuelRoomObserver() { btObserver = x.GetBytes() }).ToArray();
        }
    }

    // 0xC1 0xAA 0x0C
    [WZContract]
    public class SDuelBroadcastResult : IGameMessage
    {
        [WZMember(0, 10)] public byte[] btWinner { get; set; }
        [WZMember(1, 10)] public byte[] btLoser { get; set; }
        public SDuelBroadcastResult()
        { 
            btWinner = Array.Empty<byte>();
            btLoser = Array.Empty<byte>();
        }
        public SDuelBroadcastResult(string winner, string loser)
        {
            btWinner = winner.GetBytes();
            btLoser = loser.GetBytes();
        }
    }

    // 0xC1 0xAA 0x0D
    [WZContract]
    public class SDuelBroadcastRound : IGameMessage
    {
        [WZMember(0)] public byte Flag { get; set; }
    }

    [WZContract]
    public class SPShopSetItemPrice : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }
        [WZMember(1)] public byte Position { get; set; }

        public SPShopSetItemPrice()
        { }

        public SPShopSetItemPrice(PShopResult res, byte pos)
        {
            Result = res;
            Position = pos;
        }
    }

    [WZContract]
    public class SPShopRequestOpen : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        public SPShopRequestOpen()
        { }

        public SPShopRequestOpen(PShopResult res)
        {
            Result = res;
        }
    }

    [WZContract]
    public class SPShopRequestClose : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }

        public SPShopRequestClose()
        { }

        public SPShopRequestClose(PShopResult res, ushort numb)
        {
            Result = res;
            wzNumber = numb.ShufleEnding();
        }
    }


    [WZContract]
    public class PShopItem
    {
        [WZMember(0)] public byte Pos { get; set; }
        [WZMember(1, 12)] public byte[] Item { get; set; }
        [WZMember(2)] public uint wzPrice { get; set; }
    }

    [WZContract]
    public class SPShopRequestList : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2,10)] public byte[] btName { get; set; }
        [WZMember(3,36)] public byte[] btShopName { get; set; }
        [WZMember(4, typeof(ArrayWithScalarSerializer<byte>))] public PShopItem[] Items { get; set; }

        public SPShopRequestList()
        {
            Result = PShopResult.Disabled;
            wzNumber = 0xffff;
            Items = Array.Empty<PShopItem>();
        }
        public SPShopRequestList(PShopResult res)
        {
            Result = res;
            wzNumber = 0xffff;
            Items = Array.Empty<PShopItem>();
        }

        public SPShopRequestList(PShopResult res, ushort numb, string name, string shopName, PShopItem[] it)
        {
            btName = name.GetBytes();
            btShopName = shopName.GetBytes();
            Result = res;
            wzNumber = numb.ShufleEnding();
            Items = it;
        }
    }

    [WZContract]
    public class SPShopRequestBuy : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, 12)] public byte[] Item { get; set; }

        public SPShopRequestBuy()
        {
            Result = PShopResult.Disabled;
            wzNumber = 0xffff;
            Item = Array.Empty<byte>();
        }
        public SPShopRequestBuy(PShopResult res)
        {
            Result = res;
            wzNumber = 0xffff;
            Item = Array.Empty<byte>();
        }

        public SPShopRequestBuy(PShopResult res, ushort numb, byte[] it)
        {
            Result = res;
            wzNumber = numb.ShufleEnding();
            Item = it;
        }
    }

    [WZContract]
    public class SPShopRequestSold : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }

        public SPShopRequestSold()
        {
            Result = PShopResult.Disabled;
            wzNumber = 0xffff;
            btName = Array.Empty<byte>();
        }
        public SPShopRequestSold(PShopResult res, ushort numb, string name)
        {
            Result = res;
            wzNumber = numb.ShufleEnding();
            btName = name.GetBytes();
        }
    }

    [WZContract]
    public class SMasterInfo : IGameMessage
    {
        public SMasterInfo() { }
        public SMasterInfo(ushort level, ulong experience, ulong nextExperience, ushort points, ushort maxHealth, ushort maxShield, ushort maxMana, ushort maxStamina)
        {
            Level = level;
            Experience = experience.ShufleEnding();
            NextExperience = nextExperience.ShufleEnding();
            Points = points;
            MaxHealth = maxHealth;
            MaxShield = maxShield;
            MaxMana = maxMana;
            MaxBP = maxStamina;
        }

        //PBMSG_HEAD2 h;
        [WZMember(0)] public ushort Level { get; set; }

        [WZMember(1)] public ulong Experience { get; set; }// [8]
        [WZMember(2)] public ulong NextExperience { get; set; }// [8];

        [WZMember(3)] public ushort Points { get; set; }

        [WZMember(4)] public ushort MaxHealth { get; set; }
        [WZMember(5)] public ushort MaxMana { get; set; }
        [WZMember(6)] public ushort MaxShield { get; set; }
        [WZMember(7)] public ushort MaxBP { get; set; }
    }

    [WZContract]
    public class SMasterLevelUp : IGameMessage
    {
        public SMasterLevelUp() { }
        public SMasterLevelUp(ushort level, ushort levelAdd, ushort points, ushort maxPoints, ushort maxHealth, ushort maxShield, ushort maxMana, ushort maxStamina)
        {
            Level = level;
            LevelAdd = levelAdd;
            Points = points;
            MaxPoints = maxPoints;
            MaxHealth = maxHealth;
            MaxShield = maxShield;
            MaxMana = maxMana;
            MaxStamina = maxStamina;
        }

        //PBMSG_HEAD2 h;
        [WZMember(0)] public ushort Level { get; set; }
        [WZMember(1)] public ushort LevelAdd { get; set; }
        [WZMember(2)] public ushort Points { get; set; }
        [WZMember(3)] public ushort MaxPoints { get; set; }
        [WZMember(4)] public ushort MaxHealth { get; set; }
        [WZMember(5)] public ushort MaxMana { get; set; }
        [WZMember(6)] public ushort MaxShield { get; set; }
        [WZMember(7)] public ushort MaxStamina { get; set; }
    }

    [WZContract]
    public class SMasterLevelSkill : IGameMessage
    {
        [WZMember(0)]
        public byte type { get; set; }
        [WZMember(1)]
        public byte flag { get; set; }
        [WZMember(2)]
        public ushort MasterPoint { get; set; }
        [WZMember(3)]
        public Spell MasterSkill { get; set; }
        [WZMember(4)]
        public ushort MasterEmpty { get; set; }
        [WZMember(5)]
        public uint ChkSum { get; set; }
    }

    [WZContract(Serialized = true)]
    public class STradeRequest : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] szId { get; set; }

        public string Id { get => szId.MakeString(); set => szId = value.GetBytes(); }
    }

    [WZContract]
    public class STradeMoney: IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }
    }

    [WZContract]
    public class STradeOtherMoney : IGameMessage
    {
        [WZMember(0)]
        public uint Money { get; set; }
    }

    [WZContract]
    public class STradeResult : IGameMessage
    {
        [WZMember(0)]
        public TradeResult Result { get; set; }
    }

    [WZContract]
    public class STradeOtherAdd : IGameMessage
    {
        [WZMember(0)]
        public byte Position { get; set; }

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class STradeResponce : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1, 10)]
        public byte[] szId { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public int GuildNumber { get; set; }
    }

    [WZContract]
    public class SFriendAddReq : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, 10)] public byte[] btName { get; set; } // 4
        [WZMember(2)] public byte State { get; set; } // E

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SFriendAddSin : IGameMessage
    {
        [WZMember(0, 10)] public byte[] btName { get; set; } // 4

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SMiniMapNPC : IGameMessage
    {
        [WZMember(0)] public byte btIdentNo { get; set; }
        [WZMember(1)] public byte btIsNpc { get; set; }
        [WZMember(2)] public MiniMapTag btTag { get; set; }
        [WZMember(3)] public byte btType { get; set; }
        [WZMember(4)] public byte btPosX { get; set; }
        [WZMember(5)] public byte btPosY { get; set; }
        [WZMember(6, typeof(BinaryStringSerializer), 31)] public string szName { get; set; }

        public SMiniMapNPC()
        {
            szName = "";
        }

        public SMiniMapNPC(Monster monster, byte ident, MiniMapTag tag, byte addType)
        {
            btPosX = (byte)monster.Position.X;
            btPosY = (byte)monster.Position.Y;
            btTag = tag;
            btType = addType;
            btIdentNo = ident;
            szName = monster.Info.Name;
            btIsNpc = (byte)((monster.Type == ObjectType.NPC) ? 1 : 0);
        }
    }

    [WZContract]
    public class SPeriodItemCount : IGameMessage
    {
        [WZMember(0)] public byte Count { get; set; }
    }
}

