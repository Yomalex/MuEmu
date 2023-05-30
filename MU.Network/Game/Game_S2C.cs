using BlubLib.Serialization.Serializers;
using MU.Resources;
using MuEmu.Network.Data;
using MuEmu.Resources.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Game
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
    public class SSpellsS12Eng : IGameMessage
    {
        [WZMember(0)]
        public byte Count { get; set; }

        [WZMember(1)]
        public byte ListType { get; set; }

        [WZMember(2, SerializerType = typeof(ArraySerializer))]
        public SpellDto[] Spells { get; set; }

        public SSpellsS12Eng()
        {
            Spells = Array.Empty<SpellDto>();
        }

        public SSpellsS12Eng(byte listType, SpellDto[] spells)
        {
            Count = (byte)spells.Length;
            ListType = listType;
            Spells = spells;
        }

        public SSpellsS12Eng(byte listType, SpellDto spell)
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
        public SViewPortCreate(IEnumerable<VPCreateAbs> viewPort)
        {
            ViewPort = viewPort.Select(x => (VPCreateDto)x).ToArray();
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
        public SViewPortCreateS9(IEnumerable<VPCreateAbs> viewPort)
        {
            ViewPort = viewPort.Select(x => (VPCreateS9Dto)x).ToArray();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortCreateS12 : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public VPCreateS12Dto[] ViewPort { get; set; }

        public SViewPortCreateS12()
        {
            ViewPort = Array.Empty<VPCreateS12Dto>();
        }
        public SViewPortCreateS12(IEnumerable<VPCreateAbs> viewPort)
        {
            ViewPort = viewPort.Select(x => (VPCreateS12Dto)x).ToArray();
        }
    }
    [WZContract(LongMessage = true)]
    public class SViewPortCreateS16Kor : IGameMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public VPCreateS16KorDto[] ViewPort { get; set; }

        public SViewPortCreateS16Kor()
        {
            ViewPort = Array.Empty<VPCreateS16KorDto>();
        }
        public SViewPortCreateS16Kor(IEnumerable<VPCreateAbs> viewPort)
        {
            ViewPort = viewPort.Select(x => (VPCreateS16KorDto)x).ToArray();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortChange : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPChangeAbs[] ViewPort { get; set; }

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
    public class SViewPortChangeS12 : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPChangeS12Dto[] ViewPort { get; set; }

        public SViewPortChangeS12()
        {
            ViewPort = Array.Empty<VPChangeS12Dto>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortMonCreateS6Kor : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateDto[] ViewPort { get; set; }

        public SViewPortMonCreateS6Kor()
        {
            ViewPort = Array.Empty<VPMCreateDto>();
        }
        public SViewPortMonCreateS6Kor(IEnumerable<object> vp)
        {
            ViewPort = vp.Select(x => (VPMCreateDto)x).ToArray();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortMonCreateS9Eng : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateS9Dto[] ViewPort { get; set; }

        public SViewPortMonCreateS9Eng()
        {
            ViewPort = Array.Empty<VPMCreateS9Dto>();
        }
        public SViewPortMonCreateS9Eng(IEnumerable<object> vp)
        {
            ViewPort = vp.Select(x => (VPMCreateS9Dto)x).ToArray();
        }
    }

    [WZContract(LongMessage = true)]
    public class SViewPortMonCreateS12Eng : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateS12Dto[] ViewPort { get; set; }

        public SViewPortMonCreateS12Eng()
        {
            ViewPort = Array.Empty<VPMCreateS12Dto>();
        }
        public SViewPortMonCreateS12Eng(IEnumerable<object> vp)
        {
            ViewPort = vp.Select(x => (VPMCreateS12Dto)x).ToArray();
        }
    }

    [WZContract(LongMessage = true)]
    public class SVPortMonCreateS16Kor : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPMCreateS16KorDto[] ViewPort { get; set; }

        public SVPortMonCreateS16Kor()
        {
            ViewPort = Array.Empty<VPMCreateS16KorDto>();
        }
        public SVPortMonCreateS16Kor(IEnumerable<object> vp)
        {
            ViewPort = new VPMCreateS16KorDto[vp.Count()];
            for(var n =0; n < vp.Count(); n++)
            {
                ViewPort[n] = (VPMCreateS16KorDto)vp.ElementAt(n);
            }
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
        public uint[] QuestList { get; set; }

        public SNewQuestInfo()
        {
            QuestList = Array.Empty<uint>();
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
    public class SPositionSetS16Kor : IGameMessage
    {
        [WZMember(0)]
        public ushortle Number { get; set; }

        [WZMember(1)]
        public byte X { get; set; }

        [WZMember(2)]
        public byte Y { get; set; }

        public SPositionSetS16Kor() { }

        public SPositionSetS16Kor(ushort number, Point pos)
        {
            Number = number;
            X = (byte)pos.X;
            Y = (byte)pos.Y;
        }
    }

    [WZContract]
    public class SPositionSetS9Eng : IGameMessage
    {
        [WZMember(0)]
        public ushortle Number { get; set; }

        [WZMember(1)]
        public byte X { get; set; }

        [WZMember(2)]
        public byte Y { get; set; }

        public SPositionSetS9Eng() { }

        public SPositionSetS9Eng(ushort number, Point pos)
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

    [WZContract(Serialized = true/*, ExtraEncode = typeof(WZExtraPacketEncodeS16Kor)*/)]
    public class SMoveItemS16Kor : IGameMessage
    {
        [WZMember(0)] public byte junk1 { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte junk2 { get; set; }

        [WZMember(3)] public byte Position { get; set; }

        [WZMember(4, 12)]
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

    [WZContract()]
    public class SMonsterSoulShop : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }
    }

    [WZContract()]
    public class SMonsterSoulAvailableShop : IGameMessage
    {
        [WZMember(0)]
        public uint Amount { get; set; }
    }

    [WZContract(Serialized = true)]
    public class STalk : IGameMessage
    {
        [WZMember(0)] public NPCWindow Result { get; set; }

        [WZMember(1)] public byte Level1 { get; set; }

        [WZMember(2)] public byte Level2 { get; set; }

        [WZMember(3)] public byte Level3 { get; set; }

        [WZMember(4)] public byte Level4 { get; set; }

        [WZMember(5)] public byte Level5 { get; set; }

        [WZMember(6)] public byte Level6 { get; set; }

        [WZMember(7)] public byte Level7 { get; set; }
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
        [WZMember(0)] public byte Result { get; set; }
        /// <summary>
        /// Warehouse Money
        /// </summary>
        [WZMember(1)]
        public int wMoney { get; set; }

        /// <summary>
        /// Inventory Money
        /// </summary>
        [WZMember(2)]
        public uint iMoney { get; set; }

        public SWarehouseMoney()
        { }

        public SWarehouseMoney(bool _result, int _wMoney, uint _iMoney)
        {
            Result = (byte)(_result ? 1 : 0);
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

    [WZContract(Serialized = true)]
    public class SItemGet : IGameMessage
    {
        /// <summary>
        /// 0xFE: Zen
        /// </summary>
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1, 12)]
        public byte[] ItemInfo { get; set; }
        public SItemGet()
        {
            ItemInfo = Array.Empty<byte>();
        }

        public SItemGet(uint money, ushort itIndex)
        {
            Result = 0xFE;
            ItemInfo = BitConverter.GetBytes(money).Reverse().ToArray();
        }

        public SItemGet(byte result, byte[] info, ushort itIndex)
        {
            ItemInfo = info;
            Result = result;
        }
    }

    [WZContract(Serialized = true)]
    public class SItemGetS12Eng : IGameMessage
    {
        /// <summary>
        /// 0xFE: Zen
        /// </summary>
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public ushortle ItemRes { get; set; }

        [WZMember(2, 12)] public byte[] ItemInfo { get; set; }
        public SItemGetS12Eng()
        {
            ItemRes = 0xffff;
            ItemInfo = Array.Empty<byte>();
        }

        public SItemGetS12Eng(uint money, ushort itIndex)
        {
            ItemRes = itIndex;
            Result = 0xFE;
            ItemInfo = BitConverter.GetBytes(money).Reverse().ToArray();
        }

        public SItemGetS12Eng(byte result, byte[] info, ushort itIndex)
        {
            ItemInfo = info;
            ItemRes = itIndex;
            Result = result;
        }
    }

    [WZContract(Serialized = true)]
    public class SItemGetS16Kor : IGameMessage
    {
        /// <summary>
        /// 0xFE: Zen
        /// </summary>
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte ItemResH { get; set; }
        [WZMember(2)] public byte unk { get; set; }
        [WZMember(3)] public byte ItemResL { get; set; }
        [WZMember(4, 12)] public byte[] ItemInfo { get; set; }
        public SItemGetS16Kor()
        {
            ItemResH = 0xff;
            ItemResL = 0xff;
            ItemInfo = Array.Empty<byte>();
        }

        public SItemGetS16Kor(uint money, ushort itIndex)
        {
            ItemResH = (byte)(itIndex >> 8);
            ItemResL = (byte)(itIndex & 0xff);
            Result = 0xFE;
            ItemInfo = BitConverter.GetBytes(money).Reverse().ToArray();
        }

        public SItemGetS16Kor(byte result, byte[] info, ushort itIndex)
        {
            ItemResH = (byte)(itIndex >> 8);
            ItemResL = (byte)(itIndex & 0xff);
            ItemInfo = info;
            Result = result;
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
        public byte Map { get; set; } // 4

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
            Map = (byte)map;
            MapX = (byte)position.X;
            MapY = (byte)position.Y;
            Dir = dir;
        }
    };

    [WZContract(Serialized = true)]
    public class STeleportS12Eng : IGameMessage
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

        public STeleportS12Eng()
        { }

        public STeleportS12Eng(ushort type, Maps map, Point position, byte dir)
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
        public ushortle Number { get; set; }

        [WZMember(2)]
        public byte SkillIndex { get; set; }

        public SViewSkillState() { }

        public SViewSkillState(byte state, ushort number, byte skillIndex)
        {
            State = state;
            Number = number;
            SkillIndex = skillIndex;
        }
    }

    [WZContract]
    public class SPeriodicEffectS12Eng : IGameMessage
    {
        [WZMember(0)] public byte padding3 { get; set; }//3
        [WZMember(1)] public ushort group { get; set; }//4
        [WZMember(2)] public ushort value { get; set; }//6
        [WZMember(3)] public byte state { get; set; }//8
        [WZMember(4)] public byte padding9 { get; set; }//9
        [WZMember(5)] public ushort paddingA { get; set; }//10
        [WZMember(6)] public uint time { get; set; }//12
        [WZMember(7)] public ushort effect { get; set; } //16 Season 12 WORD, 9 BYTE
        [WZMember(8, 12)] public byte[] ItemInfo { get; set; } //18 Season 9
        [WZMember(9)] public ushort padding1E { get; set; }//30
        [WZMember(10)] public ushort wEffectValue { get; set; } //32 Season X addon
        [WZMember(11)] public ushort padding22 { get; set; }//34
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

    [WZContract(Serialized = true)]
    public class SKillPlayerEXT : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public byte padding { get; set; }

        [WZMember(2)]
        public ushort ExpH { get; set; }

        [WZMember(3)]
        public ushort ExpL { get; set; }

        [WZMember(4)]
        public ushort wzDamage { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
        public int Exp { get => ExpH<<16 | ExpL; set
            {
                ExpH = (ushort)(value >> 16);
                ExpL = (ushort)(value & 0xFFFF);
            }
        }
        public ushort Damage { get => wzDamage.ShufleEnding(); set => wzDamage = value.ShufleEnding(); }

        public SKillPlayerEXT() { }

        public SKillPlayerEXT(ushort number, int exp, ushort dmg)
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
        public byte DamageType { get; set; }

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
            DamageType = (byte)dmgType;
        }
    }

    [WZContract]
    public class SAttackResultS9Eng : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        [WZMember(1)]
        public ushort wzDamage { get; set; }

        [WZMember(2)]
        public ushort DamageType { get; set; }

        [WZMember(3)]
        public ushort wzDamageShield { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
        public ushort Damage { get => wzDamage.ShufleEnding(); set => wzDamage = value.ShufleEnding(); }
        public ushort DamageShield { get => wzDamageShield.ShufleEnding(); set => wzDamageShield = value.ShufleEnding(); }

        public SAttackResultS9Eng() { }

        public SAttackResultS9Eng(ushort number, ushort dmg, DamageType dmgType, ushort dmgShield)
        {
            Number = number;
            Damage = dmg;
            DamageShield = dmgShield;
            DamageType = (ushort)dmgType;
        }
    }

    [WZContract]
    public class SAttackResultS12Eng : IGameMessage
    {

        [WZMember(0)]
        public ushortle Number { get; set; }

        [WZMember(1)]
        public ushortle Damage { get; set; }

        [WZMember(2)]
        public ushort DamageType { get; set; }

        [WZMember(3)]
        public ushortle DamageShield { get; set; }

        public SAttackResultS12Eng() { }

        public SAttackResultS12Eng(ushort number, ushort dmg, DamageType dmgType, ushort dmgShield)
        {
            Number = number;
            Damage = dmg;
            DamageShield = dmgShield;
            DamageType = (ushort)dmgType;
        }
    }

    [WZContract]
    public class SAttackResultS16Kor : IGameMessage
    {

        [WZMember(0)] public ushortle Number { get; set; }
        [WZMember(1, typeof(ArraySerializer))] public byte[] unk1 { get; set; } = new byte[3];
        [WZMember(2)] public int Damage { get; set; }
        [WZMember(3)] public ushortle DamageType { get; set; }
        [WZMember(4)] public ushortle DamageShield { get; set; }
        [WZMember(5)] public Element Attribute { get; set; }

        public SAttackResultS16Kor() { }

        public SAttackResultS16Kor(ushort number, ushort dmg, DamageType dmgType, ushort dmgShield)
        {
            Number = number;
            Damage = dmg;
            DamageShield = dmgShield;
            DamageType = (ushort)dmgType;
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
    public class SMagicAttackS9Eng : IGameMessage
    {
        [WZMember(0)]
        public ushort wzSource { get; set; }

        [WZMember(1)]
        public ushort wzMagicNumber { get; set; }

        [WZMember(2)]
        public ushort wzTarget { get; set; }

        public SMagicAttackS9Eng() { }

        public SMagicAttackS9Eng(Spell magic, ushort source, ushort target)
        {
            wzMagicNumber = ((ushort)magic).ShufleEnding();
            wzSource = source.ShufleEnding();
            wzTarget = target.ShufleEnding();
        }
    }

    [WZContract(Serialized = true)]
    public class SMagicAttackS12Eng : SMagicAttackS9Eng, IGameMessage
    {
        public SMagicAttackS12Eng() { }
        public SMagicAttackS12Eng(Spell magic, ushort source, ushort target) : base(magic, source, target) { }
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

    [WZContract(Serialized = true)]
    public class SMagicDurationS9Eng : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; }

        [WZMember(1)]
        public byte Y { get; set; }

        [WZMember(2)]
        public byte Dis { get; set; }

        [WZMember(3)]
        public byte MagicNumberH { get; set; }

        [WZMember(4)]
        public byte NumberH { get; set; }

        [WZMember(5)]
        public byte MagicNumberL { get; set; }

        [WZMember(6)]
        public byte NumberL { get; set; }

        public SMagicDurationS9Eng() { }

        public SMagicDurationS9Eng(Spell magic, ushort Number, byte x, byte y, byte dis)
        {
            var mag = BitConverter.GetBytes((ushort)magic);
            MagicNumberH = mag[1];
            MagicNumberL = mag[0];
            mag = BitConverter.GetBytes(Number);
            NumberH = mag[1];
            NumberL = mag[0];
            X = x;
            Y = y;
            Dis = dis;
        }
    }

    [WZContract]
    public class SChainMagic : IGameMessage
    {
        [WZMember(0)] public ushortle Magic { get; set; }
        [WZMember(1)] public ushort UserIndex { get; set; }
        [WZMember(2)] public byte Padding { get; set; }
        [WZMember(3, typeof(ArrayWithScalarSerializer<byte>))] public ushort[] Targets { get; set; }
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

        public SPartyList(IEnumerable<IPartyDto> vs)
        {
            PartyMembers = vs.Select(x => (PartyDto)x).ToArray();
        }
    }

    //0xC1 0x42
    [WZContract]
    public class SPartyListS9 : IGameMessage
    {
        [WZMember(0)]
        public PartyResults Result { get; set; }

        [WZMember(1, typeof(ArrayWithScalarSerializer<byte>))]
        public PartyS9Dto[] PartyMembers { get; set; }

        public SPartyListS9()
        {
            PartyMembers = Array.Empty<PartyS9Dto>();
        }

        public SPartyListS9(IEnumerable<IPartyDto> vs)
        {
            PartyMembers = vs.Select(x => (PartyS9Dto)x).ToArray();
        }
    }
    //0xC1 0x42
    [WZContract]
    public class SPartyListS16 : IGameMessage
    {
        [WZMember(0)]
        public PartyResults Result { get; set; }

        [WZMember(1, typeof(ArrayWithScalarSerializer<byte>))]
        public PartyS16Dto[] PartyMembers { get; set; }

        public SPartyListS16()
        {
            PartyMembers = Array.Empty<PartyS16Dto>();
        }

        public SPartyListS16(IEnumerable<IPartyDto> vs)
        {
            PartyMembers = vs.Select(x => (PartyS16Dto)x).ToArray();
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
        [WZMember(0)] public byte MapX { get; set; }
        [WZMember(1)] public byte MapY { get; set; }
        [WZMember(2)] public byte MapNumber { get; set; }
        [WZMember(3)] public byte Dir { get; set; }
        [WZMember(4)] public ushort Life { get; set; }
        [WZMember(5)] public ushort Mana { get; set; }
        [WZMember(6)] public ushort wShield { get; set; }
        [WZMember(7)] public ushort BP { get; set; }
        //[WZMember(8)] public ulong unk1 { get; set; }
        [WZMember(9)] public ulong Exp { get; set; }
        [WZMember(10)] public ulong Money { get; set; }

        public SCharRegen()
        { }
        public SCharRegen(Maps map, byte x, byte y, byte dir, ushort life, ushort mana, ushort shield, ushort bp, uint exp, ulong money)
        {
            MapNumber = (byte)map;
            MapX = x;
            MapY = y;
            Dir = dir;
            Life = life;//.ShufleEnding();
            Mana = mana;//.ShufleEnding();
            wShield = shield;//.ShufleEnding();
            BP = bp;//.ShufleEnding();
            Exp = ((ulong)exp);//.ShufleEnding();
            Money = money;//.ShufleEnding();
        }
    }

    [WZContract]
    public class SCharRegenS12Eng : IGameMessage
    {
        [WZMember(0)] public byte MapX { get; set; }
        [WZMember(1)] public byte MapY { get; set; }
        [WZMember(2)] public ushort MapNumber { get; set; }
        [WZMember(3)] public byte Dir { get; set; }
        [WZMember(4)] public ushort Life { get; set; }
        [WZMember(5)] public ushort Mana { get; set; }
        [WZMember(6)] public ushort wShield { get; set; }
        [WZMember(7)] public ushort BP { get; set; }
        //[WZMember(8)] public ulong unk1 { get; set; }
        [WZMember(9)] public ulong Exp { get; set; }
        [WZMember(10)] public ulong Money { get; set; }

        public SCharRegenS12Eng()
        { }
        public SCharRegenS12Eng(Maps map, byte x, byte y, byte dir, ushort life, ushort mana, ushort shield, ushort bp, uint exp, ulong money)
        {
            MapNumber = (ushort)map;
            MapX = x;
            MapY = y;
            Dir = dir;
            Life = life;//.ShufleEnding();
            Mana = mana;//.ShufleEnding();
            wShield = shield;//.ShufleEnding();
            BP = bp;//.ShufleEnding();
            Exp = ((ulong)exp);//.ShufleEnding();
            Money = money;//.ShufleEnding();
        }
    }

    [WZContract]
    public class SCharRegenS16Kor : IGameMessage
    {
        [WZMember(0)] public byte MapX { get; set; }
        [WZMember(1)] public byte MapY { get; set; }
        [WZMember(2)] public ushort MapNumber { get; set; }
        [WZMember(3)] public byte Dir { get; set; }
        [WZMember(4)] public byte LabyrinthId { get; set; }
        [WZMember(5)] public ushort Life { get; set; }
        [WZMember(6)] public ushort Mana { get; set; }
        [WZMember(7)] public ushort wShield { get; set; }
        [WZMember(8)] public ushort BP { get; set; }
        //[WZMember(8)] public ulong unk1 { get; set; }
        [WZMember(9)] public ulong Exp { get; set; }
        [WZMember(10)] public ushort Align { get; set; }
        [WZMember(11)] public uint Money { get; set; }

        public SCharRegenS16Kor()
        { }
        public SCharRegenS16Kor(Maps map, byte x, byte y, byte dir, ushort life, ushort mana, ushort shield, ushort bp, uint exp, ulong money)
        {
            MapNumber = (ushort)map;
            MapX = x;
            MapY = y;
            Dir = dir;
            Life = life;//.ShufleEnding();
            Mana = mana;//.ShufleEnding();
            wShield = shield;//.ShufleEnding();
            BP = bp;//.ShufleEnding();
            Exp = ((ulong)exp);//.ShufleEnding();
            Money = (uint)money;//.ShufleEnding();
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
        [WZMember(1, 15)] public byte[] Item { get; set; } // Padding Size 3 item byte size 12
        [WZMember(4)] public uint Price { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopRequestList : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }
        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, typeof(BinaryStringSerializer), 10)] public string Name { get; set; }
        [WZMember(3, typeof(BinaryStringSerializer), 36)] public string ShopName { get; set; }
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
            Name = name;
            ShopName = shopName;
            Result = res;
            wzNumber = numb.ShufleEnding();
            Items = it;
        }
    }

    [WZContract]
    public class PShopItemS9Eng : PShopItem
    {
        [WZMember(5)] public ushort BlessValue { get; set; }
        [WZMember(7)] public ushort SoulValue { get; set; }
        [WZMember(9)] public ushort ChaosValue { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopRequestListS9Eng : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }
        [WZMember(3, 36)] public byte[] btShopName { get; set; }
        [WZMember(4, typeof(ArrayWithScalarSerializer<byte>))] public PShopItemS9Eng[] Items { get; set; }

        public SPShopRequestListS9Eng()
        {
            Result = PShopResult.Disabled;
            wzNumber = 0xffff;
            Items = Array.Empty<PShopItemS9Eng>();
        }
        public SPShopRequestListS9Eng(PShopResult res)
        {
            Result = res;
            wzNumber = 0xffff;
            Items = Array.Empty<PShopItemS9Eng>();
        }

        public SPShopRequestListS9Eng(PShopResult res, ushort numb, string name, string shopName, PShopItemS9Eng[] it)
        {
            btName = name.GetBytes();
            btShopName = shopName.GetBytes();
            Result = res;
            wzNumber = numb.ShufleEnding();
            Items = it;
        }
    }

    [WZContract]
    public class SPShopSearchDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 11)]
        public string Seller { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 45)]
        public string Description { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopSearch : IGameMessage
    {
        [WZMember(0)] public uint Number { get; set; }
        [WZMember(1)] public uint Count { get; set; }
        [WZMember(2)] public ushort Padding { get; set; }
        [WZMember(3, typeof(ArraySerializer))] public SPShopSearchDto[] List { get; set; }
    }

    [WZContract]
    public class SPShopItemSearchDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 11)]
        public string Seller { get; set; }
        [WZMember(1)] public byte Slot { get; set; }
        [WZMember(2)] public byte Bundle { get; set; }
        [WZMember(3, 12)] public byte[] ItemInfo{ get; set; }
        [WZMember(4)] public uint Zen { get; set; }
        [WZMember(5)] public uint JOBless { get; set; }
        [WZMember(6)] public uint JOSoul { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopItemSearch : IGameMessage
    {
        [WZMember(0)] public uint Number { get; set; }
        [WZMember(1)] public uint Count { get; set; }
        [WZMember(2)] public byte Padding { get; set; }
        [WZMember(3)] public byte Result { get; set; }
        [WZMember(4, typeof(ArraySerializer))] public SPShopItemSearchDto[] List { get; set; }
    }

    [WZContract]
    public class SPShopItemSellListDto
    {
        /*[WZMember(0, typeof(BinaryStringSerializer), 11)]
        public string Seller { get; set; }*/
        [WZMember(1)] public byte Slot { get; set; }
        [WZMember(2)] public byte Bundle { get; set; }
        [WZMember(3, 12)] public byte[] ItemInfo { get; set; }
        [WZMember(4)] public uint Zen { get; set; }
        [WZMember(5)] public uint JOBless { get; set; }
        [WZMember(6)] public uint JOSoul { get; set; }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SPShopSellList : IGameMessage
    {
        //0xC2 SS SS 7C SH:5
        [WZMember(0)] public uint Number { get; set; }//5
        [WZMember(1)] public byte Result { get; set; }//9
        [WZMember(2, typeof(BinaryStringSerializer), 45)]//10
        public string Description { get; set; }
        [WZMember(3)] public byte state { get; set; }//55
        [WZMember(4, typeof(ArrayWithScalarSerializer<uint>))] //public uint Count { get; set; }
        public SPShopItemSellListDto[] List { get; set; }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SPShopChangeStateS16Kor : IGameMessage
    {
        [WZMember(0)] public uint Number { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte State { get; set; }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SPShopSetItemPriceS16Kor : IGameMessage
    {
        [WZMember(0)] public byte Number { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte Slot { get; set; }
        [WZMember(3)] public byte Bundle { get; set; }
        [WZMember(4,12)] public byte[] ItemInfo { get; set; }
        [WZMember(5)] public uint Zen { get; set; }
        [WZMember(6)] public uint JOBless { get; set; }
        [WZMember(7)] public uint JOSoul { get; set; }
    }

    [WZContract]
    public class SPShopRequestBuy : IGameMessage
    {
        [WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public ushort wzNumber { get; set; }
        [WZMember(2, 12)] public byte[] Item { get; set; }
        [WZMember(3)] public byte ItemPos { get; set; }

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

        public SPShopRequestBuy(PShopResult res, ushort numb, byte[] it, byte itemPos)
        {
            Result = res;
            wzNumber = numb.ShufleEnding();
            Item = it;
            ItemPos = itemPos;
        }
    }

    [WZContract]
    public class SPShopRequestSold : IGameMessage
    {
        //[WZMember(0)] public PShopResult Result { get; set; }

        [WZMember(1)] public byte ItemPos { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }

        public SPShopRequestSold()
        {
            //Result = PShopResult.Disabled;
            ItemPos = 0xff;
            btName = Array.Empty<byte>();
        }
        public SPShopRequestSold(PShopResult res, byte itemPost, string name)
        {
            //Result = res;
            ItemPos = itemPost;
            btName = name.GetBytes();
        }
    }

    [WZContract]
    public class SPShopAlterVault : IGameMessage
    {
        [WZMember(0)] public byte type { get; set; }
    }

    [WZContract]
    public class SMasterInfo : IGameMessage
    {
        public SMasterInfo() { }
        public SMasterInfo(ushort level, long experience, long nextExperience, ushort points, ushort maxHealth, ushort maxShield, ushort maxMana, ushort maxStamina)
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

        [WZMember(1)] public long Experience { get; set; }// [8]
        [WZMember(2)] public long NextExperience { get; set; }// [8];

        [WZMember(3)] public ushort Points { get; set; }

        [WZMember(4)] public ushort MaxHealth { get; set; }
        [WZMember(5)] public ushort MaxMana { get; set; }
        [WZMember(6)] public ushort MaxShield { get; set; }
        [WZMember(7)] public ushort MaxBP { get; set; }
    }

    [WZContract]
    public class MajesticInfoDto
    {
        [WZMember(0)] public byte Section { get; set; }
        [WZMember(1)] public ushort Id { get; set; }
        [WZMember(2)] public byte Level { get; set; }
        [WZMember(3)] public float CurrentValue { get; set; }
        [WZMember(4)] public float NextValue { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SMajesticInfo : IGameMessage
    {
        [WZMember(0)] public ushort Points { get; set; }
        [WZMember(1, typeof(ArrayWithScalarSerializer<uint>))] public MajesticInfoDto[] SkillList { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SMajesticStatsInfo : IGameMessage
    {
        [WZMember(1, typeof(ArrayWithScalarSerializer<uint>))] public MajesticInfoDto[] SkillList { get; set; }
    }

    [WZContract]
    public class SMasterLevelUp : IGameMessage
    {
        public SMasterLevelUp() { }
        public SMasterLevelUp(ushort level, ushort pointLevelAdd, ushort points, ushort maxPoints, ushort maxHealth, ushort maxShield, ushort maxMana, ushort maxStamina)
        {
            Level = level;
            PointLevelAdd = pointLevelAdd;
            Points = points;
            MaxPoints = maxPoints;
            MaxHealth = maxHealth;
            MaxShield = maxShield;
            MaxMana = maxMana;
            MaxStamina = maxStamina;
        }

        //PBMSG_HEAD2 h;
        [WZMember(0)] public ushort Level { get; set; }
        [WZMember(1)] public ushort PointLevelAdd { get; set; }
        [WZMember(2)] public ushort Points { get; set; }
        [WZMember(3)] public ushort MaxPoints { get; set; }
        [WZMember(4)] public ushort MaxHealth { get; set; }
        [WZMember(5)] public ushort MaxMana { get; set; }
        [WZMember(6)] public ushort MaxShield { get; set; }
        [WZMember(7)] public ushort MaxStamina { get; set; }
    }

    [WZContract]
    public class SMasterLevelSkillS9ENG : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }//4
        [WZMember(1)] public byte padding { get; set; }//5
        [WZMember(2)] public ushort MasterLevelPoint { get; set; }//6,7
        [WZMember(3)] public byte MasterSkillUIIndex { get; set; }//8
        [WZMember(4)] public byte padding2 { get; set; }//9
        [WZMember(5)] public ushort padding3 { get; set; }//A,B
        [WZMember(6)] public int dwMasterSkillIndex { get; set; }     // C
        [WZMember(7)] public int dwMasterSkillLevel { get; set; }         // 10
        [WZMember(8)] public float fMasterSkillCurValue { get; set; }         // 14
        [WZMember(9)] public float fMasterSkillNextValue { get; set; }		// 18
    }

    [WZContract(LongMessage = true)]
    public class SMasterLevelSkillListS9ENG : IGameMessage
    {
        [WZMember(0)] public byte Padding1 { get; set; }//5
        [WZMember(1)] public ushort Padding2 { get; set; }//6,7
        [WZMember(2, typeof(ArrayWithScalarSerializer<int>))] public MasterSkillInfoDto[] Skills { get; set; }//8
    }

    [WZContract]
    public class MasterSkillInfoDto
    {
        [WZMember(0)] public byte MasterSkillUIIndex { get; set; }//0
        [WZMember(1)] public byte MasterSkillLevel { get; set; }//1
        [WZMember(2)] public ushort fill { get; set; }//2,3
        [WZMember(3)] public float MasterSkillCurValue { get; set; }//4
        [WZMember(4)] public float MasterSkillNextValue { get; set; }//8
        [WZMember(5)] public int btUnk { get; set; }//12
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

        public SMiniMapNPC(Point Position, byte ident, MiniMapTag tag, byte addType, string Name)
        {
            btPosX = (byte)Position.X;
            btPosY = (byte)Position.Y;
            btTag = tag;
            btType = addType;
            btIdentNo = ident;
            szName = Name;
            btIsNpc = 1;
        }

        public SMiniMapNPC(Rectangle Door, byte ident, MiniMapTag tag, byte addType, Maps map)
        {
            btPosX = (byte)Door.X;
            btPosY = (byte)Door.Y;
            btTag = tag;
            btType = addType;
            btIdentNo = ident;
            szName = "To " + map;
            btIsNpc = 0;
        }
    }

    [WZContract]
    public class SPeriodItemCount : IGameMessage
    {
        [WZMember(0)] public byte Count { get; set; }
    }

    [WZContract]
    public class SLifeInfo : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1)] public uint wzMaxLife { get; set; }
        [WZMember(2)] public uint wzLife { get; set; }
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
        public uint MaxLife { get => wzMaxLife.ShufleEnding(); set => wzMaxLife = value.ShufleEnding(); }
        public uint Life { get => wzLife.ShufleEnding(); set => wzLife = value.ShufleEnding(); }
    }

    [WZContract]
    public class SMuHelperState : IGameMessage
    {
        [WZMember(0)]
        public byte Time { get; set; }
        [WZMember(1)]
        public byte TimeMultipler { get; set; }
        [WZMember(2)]
        public ushort padding { get; set; }
        [WZMember(3)]
        public uint Money { get; set; }
        [WZMember(4)]
        public byte Status { get; set; }

        public ushort usTime { 
            get => (ushort)(TimeMultipler * 0xff + Time); 
            set
            {
                Time = (byte)(value % 0xff);
                TimeMultipler = (byte)(value / 0xff);
            }                
         }
    }

    [WZContract]
    public class SAttackSpeed : IGameMessage
    {
        [WZMember(0)]
        public uint AttackSpeed { get; set; }
        [WZMember(1)]
        public uint MagicSpeed { get; set; }
    }

    [WZContract(Serialized = true)]
    public class SNPCDialog : IGameMessage
    {
        [WZMember(0)] public ushort NPC { get; set; } //4
        [WZMember(1)] public ushort Alingment { get; set; }// 6
        [WZMember(2)] public uint Contribution { get; set; } // 8
    }

    [WZContract]
    public class SGremoryCaseOpen : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SGremoryCaseOpenS16 : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SGremoryCaseList : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public GCItemDto[] List { get; set; }
    }

    [WZContract]
    public class SGremoryCaseReceiveItem : IGameMessage
    {
        [WZMember(0)]
        public GCItemDto Item { get; set; }
    }

    [WZContract]
    public class SGremoryCaseNotice : IGameMessage
    {
        [WZMember(0)]
        public GremoryNotice Status { get; set; }
    }

    [WZContract]
    public class SGremoryCaseDelete : IGameMessage
    {
        [WZMember(0)] public GremoryStorage StorageType { get; set; }
        [WZMember(1)] public ushort ItemNumber { get; set; }
        [WZMember(2)] public uint AuthCode { get; set; }
        [WZMember(3)] public uint ItemGUID { get; set; }
    }

    [WZContract]
    public class SGremoryCaseDeleteS16 : IGameMessage
    {
        [WZMember(0)] public GremoryStorage StorageType { get; set; }
        [WZMember(1)] public byte Unk { get; set; }
        [WZMember(2)] public ushort ItemNumber { get; set; }
        [WZMember(3)] public uint AuthCode { get; set; }
        [WZMember(4)] public uint ItemGUID { get; set; }
        [WZMember(5)] public uint Slot { get; set; }
    }

    [WZContract]
    public class GCItemDto
    {
        [WZMember(0)] public GremoryStorage RewardInventory { get; set; }
        [WZMember(1)] public GremorySource RewardSource { get; set; }
        [WZMember(2)] public ushort Padding { get; set; }
        [WZMember(3)] public uint ItemGUID { get; set; }
        [WZMember(4, 12)] public byte[] ItemInfo { get; set; }
        [WZMember(5)] public uint AuthCode { get; set; }
        [WZMember(6)] public uint ExpireTime { get; set; }
        [WZMember(7)] public uint Unk { get; set; }
        [WZMember(8)] public uint Unk1 { get; set; }
        [WZMember(9)] public uint Unk2 { get; set; }
        [WZMember(10)] public uint Unk3 { get; set; }
    }

    [WZContract]
    public class SGremoryCaseUseItem : IGameMessage
    {
        public enum GCResult : byte {
            Success,
            Error,
            Error2,
            DatabaseError,
            NotEnoughtSpace,
            ClosingGremoryCaseError,
        }
        [WZMember(0)] public GCResult Result { get; set; }
        [WZMember(1)] public GremoryStorage Inventory { get; set; }
        [WZMember(2)] public ushort Item { get; set; }
        [WZMember(3)] public uint Unk { get; set; }
        [WZMember(4)] public uint Serial { get; set; }
        [WZMember(5)] public uint Slot { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopSearchItem : IGameMessage
    {
        [WZMember(0)] public int iPShopCnt { get; set; }
        [WZMember(1)] public byte btContinueFlag { get; set; }
        [WZMember(2, typeof(ArraySerializer))] public SPShopSearchItemDto[] List { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopSearchItemS16Kor : IGameMessage
    {
        [WZMember(0)] public int iPShopCnt { get; set; }
        [WZMember(1)] public byte btContinueFlag { get; set; }
        [WZMember(2, typeof(ArraySerializer))] public SPShopSearchItemDto[] List { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPShopCancelItemSaleS16Kor : IGameMessage
    {
        [WZMember(0)] public uint Data { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte Slot { get; set; }
    }

    [WZContract]
    public class SPShopSearchItemDto
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 11)] public string szName { get; set; } //11
        [WZMember(2, typeof(BinaryStringSerializer), 37)] public string szPShopText { get; set; } //[37];

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class GRefineJewel : IGameMessage
    {

    }

    [WZContract]
    public class PentagramJewelDto
    {
        [WZMember(1)] public byte JewelPos { get; set; }
        [WZMember(2)] public byte JewelIndex { get; set; }
        [WZMember(3)] public byte MainAttribute { get; set; }
        [WZMember(4)] public byte ItemType { get; set; }
        [WZMember(5)] public ushort ItemIndex { get; set; }
        [WZMember(6)] public byte Level { get; set; }
        [WZMember(7)] public byte Rank1OptionNum { get; set; }
        [WZMember(8)] public byte Rank1Level { get; set; }
        [WZMember(9)] public byte Rank2OptionNum { get; set; }
        [WZMember(10)] public byte Rank2Level { get; set; }
        [WZMember(11)] public byte Rank3OptionNum { get; set; }
        [WZMember(12)] public byte Rank3Level { get; set; }
        [WZMember(13)] public byte Rank4OptionNum { get; set; }
        [WZMember(14)] public byte Rank4Level { get; set; }
        [WZMember(15)] public byte Rank5OptionNum { get; set; }
        [WZMember(16)] public byte Rank5Level { get; set; }
    }

    [WZContract]
    public class SPentagramJewelIn : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public PentagramJewelDto Info { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPentagramJewelInfo : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte JewelCnt { get; set; }
        [WZMember(2)] public byte JewelPos { get; set; }
        [WZMember(3, typeof(ArraySerializer))] public PentagramJewelDto[] JewelsDto { get; set; }

        public SPentagramJewelInfo() { }
        public SPentagramJewelInfo(byte jPos, PentagramJewelDto[] array)
        {
            var subArray = array.Where(x => x.JewelPos == jPos).ToArray();
            JewelsDto = subArray;
            Result = (byte)(subArray.Length > 0 ? 1 : 0);
            JewelCnt = (byte)subArray.Length;
            JewelPos = jPos;
        }
    }

    [WZContract]
    public class SPentagramJewelInOut : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SElementalDamage : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1)] public Element Element { get; set; }
        [WZMember(2)] public ushort wzTarget { get; set; }
        [WZMember(3)] public uint Damage { get; set; }

        public ushort Number { set => wzNumber = value.ShufleEnding(); get => wzNumber.ShufleEnding(); }
        public ushort Target { set => wzTarget = value.ShufleEnding(); get => wzTarget.ShufleEnding(); }
    }

    [WZContract]
    public class SNeedSpiritMap : IGameMessage
    { }

    [WZContract]
    public class SPetInfo : IGameMessage
    {
        [WZMember(0)] public byte PetType { get; set; }   // 3
        [WZMember(1)] public byte InvenType { get; set; } // 4
        [WZMember(2)] public byte nPos { get; set; }  // 5
        [WZMember(3)] public byte Level { get; set; } // 6
        [WZMember(4)] public byte padding { get; set; } // 6
        [WZMember(5)] public int Exp { get; set; }    // 8
        [WZMember(6)] public byte Dur { get; set; }
    }

    [WZContract]
    public class SPetAttack:IGameMessage
    {
        [WZMember(0)] public byte PetType { get; set; }   //	3
        [WZMember(1)] public byte SkillType { get; set; } // 4
        [WZMember(2)] public ushort wzNumber { get; set; } // 5
        [WZMember(3)] public ushort wzTargetNumber { get; set; } // 7
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }   // 5
        public ushort TargetNumber { get => wzTargetNumber.ShufleEnding(); set => wzTargetNumber = value.ShufleEnding(); }   // 5
    }

    [WZContract]
    public class SExpEventInfo : IGameMessage
    {
        [WZMember(0)] public ushort PCBangRate { get; set; }
        [WZMember(1)] public ushort EventExp { get; set; }
        [WZMember(2)] public ushort GoldChannel { get; set; }
    }

    [WZContract]
    public class SEquipamentChange : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }   // 3
        [WZMember(1, 12)] public byte[] ItemInfo{ get; set; }   // 5
        [WZMember(2)] public Element Element { get; set; }
    }

    [WZContract]
    public class SXUpPront : IGameMessage
    {
        [WZMember(0)] public ushort Str{ get; set; }
        [WZMember(1)] public ushort AddStr{ get; set; }
        [WZMember(2)] public ushort Dex{ get; set; }
        [WZMember(3)] public ushort AddDex{ get; set; }
        [WZMember(4)] public ushort Vit{ get; set; }
        [WZMember(5)] public ushort AddVit{ get; set; }
        [WZMember(6)] public ushort Ene{ get; set; }
        [WZMember(7)] public ushort AddEne{ get; set; }
        [WZMember(8)] public ushort Leadership{ get; set; }
        [WZMember(9)] public ushort AddLeadership{ get; set; }
        [WZMember(10)] public float mPrec { get; set; }
    }

    [WZContract]
    public class SXElementalData : IGameMessage
    {
        [WZMember(0)] public int PVMDamageMin{ get; set; }
        [WZMember(1)] public int PVMDamageMax { get; set; }
        [WZMember(2)] public int PVPDamageMin{ get; set; }
        [WZMember(3)] public int PVPDamageMax { get; set; }
        [WZMember(4)] public int PVMAttackSuccessRate{ get; set; }
        [WZMember(5)] public int PVPAttackSuccessRate { get; set; }
        [WZMember(6)] public int PVMDefense{ get; set; }
        [WZMember(7)] public int PVPDefense{ get; set; }
        [WZMember(8)] public int PVMDefenseSuccessRate{ get; set; }
        [WZMember(9)] public int PVPDefenseSuccessRate { get; set; }
        [WZMember(10)] public int Unk1{ get; set; }
        [WZMember(11)] public int Unk2 { get; set; }
        [WZMember(12)] public int Unk3 { get; set; }
        [WZMember(13)] public int Unk4 { get; set; }
        [WZMember(14)] public int Unk5 { get; set; }
        [WZMember(15)] public int Unk6 { get; set; }
        [WZMember(16)] public int Unk7 { get; set; }
        [WZMember(17)] public int Unk8 { get; set; }
        [WZMember(18)] public int CriticalDamageRate{ get; set; }
        [WZMember(19)] public int PVMIncreaseDamage{ get; set; }
        [WZMember(20)] public int PVPIncreaseDamage { get; set; }
        [WZMember(21)] public int PVMAbsorbDamage{ get; set; }
        [WZMember(22)] public int PVPAbsorbDamage { get; set; }
        [WZMember(23)] public int AbsorbShield{ get; set; }
        [WZMember(24)] public int AbsorbHP{ get; set; }
        [WZMember(25)] public int BleedingDamage{ get; set; }
        [WZMember(26)] public int Paralyzing{ get; set; }
        [WZMember(27)] public int Bind{ get; set; }
        [WZMember(28)] public int Punish{ get; set; }
        [WZMember(29)] public int Blind{ get; set; }
        [WZMember(30)] public int Res_to_str_elem{ get; set; }
        [WZMember(31)] public int Res_to_elem_dmg{ get; set; }
        [WZMember(32)] public int AddAttackDamage{ get; set; }
        [WZMember(33)] public int AddDefense{ get; set; }
    }

    [WZContract]
    public class SXCharacterInfo : IGameMessage
    {
        //0-3
        /*237*/
        /// <summary>
        /// CriticalDamage Is %
        /// </summary>
        [WZMember(0)] public ushort CriticalDamage{ get; set; }//ÐÒÔËÒ»»÷Ôö¼ÓÉËº¦
        /*239*/
        /// <summary>
        /// ExcellentDamage Is +DMG
        /// </summary>
        [WZMember(1)] public ushort ExcellentDamage{ get; set; }//×¿Ô½Ò»»÷Ôö¼ÓÉËº¦
        /*241*/
        [WZMember(2)] public ushort SkillDamageBonus{ get; set; }//¼¼ÄÜ¹¥»÷Á¦Ôö¼Ó
        /*243*/
        [WZMember(3)] public ushort Defense{ get; set; }//»ù±¾·ÀÓùÁ¦
        /*245*/
        [WZMember(4)] public ushort Str{ get; set; }//Á¦Á¿    //12
        /*247*/
        [WZMember(5)] public ushort AddStr{ get; set; }//¸½¼ÓÁ¦Á¿
        /*249*/
        [WZMember(6)] public ushort Dex{ get; set; }//Ãô½Ý  //16
        /*251*/
        [WZMember(7)] public ushort AddDex{ get; set; }//¸½¼ÓÃô½Ý
        /*253*/
        [WZMember(8)] public ushort Vit{ get; set; }//ÌåÁ¦	 //20
        /*255*/
        [WZMember(9)] public ushort AddVit{ get; set; }//¸½¼ÓÌåÁ¦
        /*257*/
        [WZMember(10)] public ushort Energy{ get; set; }//ÖÇÁ¦  //24
        /*259*/
        [WZMember(11)] public ushort AddEnergy{ get; set; }//¸½¼ÓÖÇÁ¦
        /*261*/
        [WZMember(12)] public ushort Leadership{ get; set; }//Í³ÂÊ   //28
        /*263*/
        [WZMember(13)] public ushort AddLeadership{ get; set; }
        /*265*/
        [WZMember(14)] public ushort SDAttack{ get; set; }//Ï®»÷Ê±SD±ÈÂÊ%
        /*267*/
        [WZMember(15)] public ushort IgnoreShieldGaugeRate{ get; set; }//SDÎÞÊÓ¼¸ÂÊ%
        /*269*/
        [WZMember(16)] public ushort SDAttack1{ get; set; }//¹¥»÷Ê±SD±ÈÂÊ%
        /*271*/
        [WZMember(17)] public ushort MoneyAmountDropRate{ get; set; }//»ñµÃ½ð±ÒÔö¼ÓÂÊ%
        /*273*/
        [WZMember(18)] public float IgnoreDefenseRate{ get; set; }//ÎÞÊÓ·ÀÓùÁ¦¼¸ÂÊ%
        /*277*/
        [WZMember(19)] public float HPRecovery{ get; set; } //ÉúÃü×Ô¶¯»Ö¸´Á¿
        /*281*/
        [WZMember(20)] public float MPRecovery{ get; set; }//Ä§·¨»Ö¸´Á¿
        /*285*/
        [WZMember(21)] public float StunRate{ get; set; }//Êø¸¿¼¸ÂÊ
        /*289*/
        [WZMember(22)] public float ResistStunRate{ get; set; }//Êø¸¿µÖ¿¹¼¸ÂÊ%
        /*293*/
        [WZMember(23)] public float fTripleDamageRationInfo{ get; set; }
        /*297*/
        [WZMember(24)] public float ShieldDamageReduction{ get; set; }//¶ÜÅÆÎüÊÕÉËº¦
        /*301*/
        [WZMember(25)] public float fMonsterDieGetHP_info{ get; set; }//¹ÖÎïËÀÍöÉúÃü»Ö¸´Á¿
        /*305*/
        [WZMember(26)] public float fMonsterDieGetMana_info{ get; set; }//¹ÖÎïËÀÍöÄ§·¨»Ö¸´Á¿
        /*309*/
        [WZMember(27)] public float fMonsterDieGetSD_info{ get; set; }//¹ÖÎïËÀÍöSD»Ö¸´Á¿
        /*313*/
        [WZMember(28)] public float SDRecovery{ get; set; }//SD×Ô¶¯»Ö¸´Á¿
        /*317*/
        [WZMember(29)] public float DefensiveFullMPRestoreRate{ get; set; }//Ä§·¨ÖµÍêÈ«»Ö¸´¼¸ÂÊ
        /*321*/
        [WZMember(30)] public float DefensiveFullHPRestoreRate{ get; set; }//ÉúÃüÍêÈ«»Ö¸´¼¸ÂÊ
        /*325*/
        [WZMember(31)] public float OffensiveFullSDRestoreRate{ get; set; }//SDÍêÈ«»Ö¸´¼¸ÂÊ
        /*329*/
        [WZMember(32)] public float BPRecovery{ get; set; }//AG×Ô¶¯»Ö¸´Á¿
        /*333*/
        [WZMember(33)] public float fWingDamageAbsorb_info{ get; set; }//ÉËº¦ÎüÊÕÂÊ
        /*337*/
        [WZMember(34)] public float BlockRate{ get; set; }//¶Ü·ÀÓù¼¸ÂÊ
        /*341*/
        [WZMember(35)] public float ParryRate{ get; set; }//ÎäÆ÷¸ñµµ¼¸ÂÊ
        /*345*/
        [WZMember(36)] public float AbsorbLife{ get; set; }//ÉúÃüÁ¦ÎüÊÕÁ¿
        /*349*/
        [WZMember(37)] public float AbsorbSD{ get; set; }//SDÎüÊÕÁ¿
        /*353*/
        [WZMember(38)] public float FullDamageReflectRate{ get; set; }//·´µ¯¹¥»÷¼¸ÂÊ
        /*357*/
        [WZMember(39)] public float fWingDamageIncRate_info{ get; set; }//ÉËº¦Ìá¸ßÂÊ
        /*361*/
        [WZMember(40)] public float MPConsumptionRate{ get; set; }//Ä§·¨Ê¹ÓÃ¼õÉÙÂÊ
        /*365*/
        [WZMember(41)] public float CriticalDamageRate{ get; set; }//ÐÒÔËÒ»»÷ÉËº¦¼¸ÂÊ
        /*369*/
        [WZMember(42)] public float ExcellentDamageRate{ get; set; }//×¿Ô½Ò»»÷ÉËº¦¼¸ÂÊ
        /*373*/
        [WZMember(43)] public float DoubleDamageRate{ get; set; }//Ë«±¶ÉËº¦¼¸ÂÊ
        /*377*/
        [WZMember(44)] public float TripleDamageRate { get; set; }
        /*381*/
        [WZMember(45)] public byte DamageReduction{ get; set; }//ÉËº¦¼õÉÙÂÊ
        /*382*/
        [WZMember(46)] public byte BPConsumptionRate{ get; set; }//AGÊ¹ÓÃ¼õÉÙÂÊ
        /*383*/
        [WZMember(47)] public byte DamageReflect{ get; set; }//ÉËº¦·´ÉäÂÊ
        /*384*/	
        [WZMember(48)] public byte AGUsageRate { get; set; }
        /*384*/
        [WZMember(49)] public byte unk37 { get; set; }
        /*384*/
        [WZMember(50)] public byte unk38 { get; set; }
        /*384*/
        [WZMember(51)] public byte unk39 { get; set; }
        /*384*/
        [WZMember(52)] public byte unk3a { get; set; }
    }
    [WZContract]
    public class SNQWorldLoad : IGameMessage
    { }

    [WZContract]
    public class SNQWorldListDto
    {
        [WZMember(0)] public ushort QuestIndex { get; set; }
        [WZMember(1)] public byte TagetNumber { get; set; }
        [WZMember(2)] public byte QuestState { get; set; }
    }

    [WZContract]
    public class SNQWorldList : IGameMessage
    {        
        [WZMember(0)] public SNQWorldListDto Quest { get; set; }
    }

    [WZContract]
    public class SPKLevel : IGameMessage
    {
        [WZMember(0)] public ushortle Index { get; set; }
        [WZMember(1)] public PKLevel PKLevel { get; set; }
    }
    [WZContract]
    public class SMonsterSkillS9Eng : IGameMessage
    {
        [WZMember(0)] public ushort MonsterSkillNumber { get; set; }  // 3
        [WZMember(1)] public ushort ObjIndex { get; set; } // 4
        [WZMember(2)] public ushort TargetObjIndex { get; set; }	// 6
    }
    [WZContract]
    public class SEventNotificationS16Kor : IGameMessage
    {
        [WZMember(0)] public EventIcon EventID { get; set; }  // 3
        [WZMember(1)] public byte Active { get; set; } // 4
    }

    [WZContract]
    public class SOpenBox : IGameMessage
    {
        [WZMember(0)] public OBResult Result { get; set; }  // 3
        [WZMember(1)] public int Slot { get; set; } // 4
    }

    [WZContract]
    public class SItemSplit : IGameMessage
    {
        [WZMember(0)] public byte Id { get; set; }
        [WZMember(1)] public byte Result { get; set; }
    }

    [WZContract]
    public class SPartyMRegister : IGameMessage
    {
        [WZMember(0)] public int Result { get; set; }
    }

    [WZContract]
    public class PartyMSearchMemberDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 11)] public string Name { get; set; } //0
        [WZMember(1)] public ushort Level { get; set; } //0
        [WZMember(2)] public ushort Race { get; set; } //0
    }

    [WZContract]
    public class PartyMSearchDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 41)] public string Text { get; set; } //0
        [WZMember(1)] public byte Gens { get; set; } //0
        [WZMember(2)] public bool Password { get; set; } //0
        [WZMember(3)] public byte Count { get; set; } //0
        [WZMember(4)] public ushort MinLevel { get; set; } //0
        [WZMember(5)] public ushort MaxLevel { get; set; } //0
        [WZMember(6, typeof(ArraySerializer))] public PartyMSearchMemberDto[] Members { get; set; } //0
    }

    [WZContract(LongMessage = true)]
    public class SPartyMSearch : IGameMessage
    {
        [WZMember(0)] public uint Count { get; set; }
        [WZMember(1)] public uint Page { get; set; }
        [WZMember(2)] public uint MaxPage { get; set; }
        [WZMember(3)] public int Result { get; set; }
        [WZMember(4, typeof(ArraySerializer))] public PartyMSearchDto[] List { get; set; }
    }
    [WZContract]
    public class SPartyMJoin : IGameMessage
    {
        [WZMember(0)] public int Result { get; set; }
        [WZMember(1)] public bool UsePassword { get; set; }
        [WZMember(2)] public byte Gens { get; set; }
        [WZMember(3, typeof(BinaryStringSerializer), 11)] public string Name{ get; set; }
        [WZMember(4, typeof(BinaryStringSerializer), 41)] public string Text{ get; set; }
    }
    [WZContract]
    public class SPartyMJoinNotify : IGameMessage
    { }

    [WZContract]
    public class PartyMJoinListDto : IGameMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 11)] public string Name { get; set; }
        [WZMember(1)] public byte Race { get; set; }
        [WZMember(2)] public int Level { get; set; }
        [WZMember(3)] public int Data { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SPartyMJoinList : IGameMessage
    {
        [WZMember(0)] public byte unk5 { get; set; }
        [WZMember(1)] public ushort unk6 { get; set; }
        [WZMember(2)] public int Count { get; set; }
        [WZMember(3)] public int Result { get; set; }
        [WZMember(4, typeof(ArraySerializer))] public PartyMJoinListDto[] List { get; set; }
    }

    [WZContract]
    public class SPartyMCancel : IGameMessage
    {
        [WZMember(1)] public int Type { get; set; }
        [WZMember(2)] public int Result { get; set; }
    }

    [WZContract]
    public class SHuntingRecordDay : IGameMessage
    {
        [WZMember(1)] public byte Id { get; set; }
        [WZMember(2)] public int Year { get; set; }
        [WZMember(3)] public byte Month { get; set; }
        [WZMember(4)] public byte Day { get; set; }
        [WZMember(5)] public int Level { get; set; }
        [WZMember(6)] public int Duration { get; set; }
        [WZMember(7)] public long Damage { get; set; }
        [WZMember(8)] public long ElementalDamage { get; set; }
        [WZMember(9)] public int Healing { get; set; }
        [WZMember(10)] public int KilledCount { get; set; }
        [WZMember(11)] public long Experience { get; set; }

        public void SetDT(DateTime dt)
        {
            Year = dt.Year;
            Month = (byte)dt.Month;
            Day = (byte)dt.Day;
        }
    }

    [WZContract]
    public class HuntingRecordListDto
    {
        //[WZMember(0, typeof(ArraySerializer))] public byte[] Unk1 { get; set; } = new byte[4] { 1,2,3,4 }; //0
        [WZMember(1)] public uint Id { get; set; } //6,7
        [WZMember(2)] public uint Year { get; set; } //3
        [WZMember(3)] public byte Month { get; set; } //4
        [WZMember(4)] public byte Day { get; set; } //5
        [WZMember(5)] public uint Level { get; set; } //8, 00 05 00 00
        [WZMember(6)] public uint Duration { get; set; }
        [WZMember(7)] public long Damage { get; set; }
        [WZMember(8)] public long ElementalDamage { get; set; }
        [WZMember(9)] public uint Healing { get; set; }
        [WZMember(10)] public uint KilledCount { get; set; }
        [WZMember(11)] public ulong Experience { get; set; }
        //[WZMember(12, typeof(ArraySerializer))] public byte[] Unk1 { get; set; } = new byte[3] { 1, 2, 3 };
    }

    [WZContract]
    public class SHuntingRecordList : IGameMessage
    {
        [WZMember(0)] public ushortle Unk { get; set; } = new ushortle();
        //[WZMember(1)] public ushort Count { get; set; }
        [WZMember(3, typeof(ArrayWithScalarSerializer<ushort>))] public HuntingRecordListDto[] List { get; set; }
    }

    [WZContract]
    public class SHuntingRecordTime : IGameMessage
    {
        [WZMember(0)] public int Time { get; set; }
        [WZMember(1)] public long Damage { get; set; }
        [WZMember(2)] public long ElementalDamage { get; set; }
        [WZMember(3)] public int Healing { get; set; }
        [WZMember(4)] public int KilledCount { get; set; }
        [WZMember(5)] public long Experience { get; set; }
    }

    [WZContract(Dump = true)]
    public class SMossMerchantOpen : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public byte[] List { get; set; }
    }

    [WZContract]
    public class SMossMerchantOpenBox : IGameMessage
    {
        [WZMember(0, typeof(ArraySerializer))] public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class CancelItemSaleInfoDto
    {
        [WZMember(0)] public uint ExpireSec { get; set; }
        //[WZMember(1)] public byte Unk { get; set; }
        [WZMember(2, 12)] public byte[] ItemInfo{ get; set; } //[12]
        [WZMember(3)] public ushort ItemCount { get; set; }
        [WZMember(4)] public int RequireMoney { get; set; }
        [WZMember(5)] public byte IndexCode { get; set; }
}

    [WZContract(LongMessage = true)]
    public class SCancelItemSaleListS16 : IGameMessage
    {
        //PSWMSG_HEAD h;
        //[WZMember(0)] public byte Result { get; set; }
        //BYTE btItemCnt;
        [WZMember(1, typeof(ArrayWithScalarSerializer<ushortle>))] public CancelItemSaleInfoDto[] ItemList { get;set; }
    }

    [WZContract]
    public class SCancelItemSaleResult : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }


    [WZContract]
    public class ItemViewS16Dto
    {
        [WZMember(0, 12)] public byte[] ItemInfo { get; set; }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SPShopItemViewS16Kor : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte Slot { get; set; }
        [WZMember(2)] public uint Zen { get; set; }
        [WZMember(3)] public uint JOBless { get; set; }
        [WZMember(4)] public uint JOSoul { get; set; }

        [WZMember(5, typeof(ArrayWithScalarSerializer<uint>))] public ItemViewS16Dto[] Items { get; set; }
    }

    [WZContract()]
    public class SRuudBuy : IGameMessage
    {
        public byte Result { get; set; }
    }

    [WZContract()]
    public class SRuudSend : IGameMessage
    {
        public uint Ruud { get; set; }
        public uint AddRuud { get; set; }
        public byte Add { get; set; }
    }
}

