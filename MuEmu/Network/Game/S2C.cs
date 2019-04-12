using MuEmu.Network.Data;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        [WZMember(2, typeof(ArraySerializer))]
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

    [WZContract]
    public class SViewPortItemDestroy : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public VPDestroyDto[] ViewPort { get; set; }
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
            Flag = (byte)(flag?1:0);
        }
    }

    [WZContract]
    public class SManaUpdate : IGameMessage
    {
        [WZMember(0)] public RefillInfo Pos { get; set; }

        [WZMember(1)] public ushort MP { get; set; }

        //[WZMember(2)] public byte Flag { get; set; }

        [WZMember(3)] public ushort BP { get; set; }

        public ushort Mana { get => MP.ShufleEnding(); set => MP = value.ShufleEnding(); }

        public ushort Stamina { get => BP.ShufleEnding(); set => BP = value.ShufleEnding(); }

        public SManaUpdate()
        { }

        public SManaUpdate(RefillInfo pos, ushort hp, ushort sd/*, bool flag*/)
        {
            Pos = pos;
            Mana = hp;
            Stamina = sd;
            //Flag = (byte)(flag ? 1 : 0);
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
        public ushort Number { get; set; }   // 3,4

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
            Number = number.ShufleEnding();
            X = x;
            Y = y;
            Path = path;
        }
    }

    [WZContract]
    public class SPositionSet : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; }

        [WZMember(1)]
        public byte X { get; set; }

        [WZMember(2)]
        public byte Y { get; set; }
    }

    [WZContract]
    public class SPointAdd : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public ushort MaxLifeAndMana { get; set; }

        [WZMember(2)]
        public ushort MaxShield { get; set; }

        [WZMember(3)]
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

    [WZContract(Serialized =true)]
    public class SMoveItem : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte Position { get; set; }

        [WZMember(2,12)]
        public byte[] ItemInfo { get; set; }
    }

    [WZContract]
    public class SEventEnterCount :IGameMessage
    {
        [WZMember(0)]
        public EventEnterType Type { get; set; }

        [WZMember(1)]
        public byte Left { get; set; }
    }

    [WZContract(Serialized =true)]
    public class SCloseMsg : IGameMessage
    {
        [WZMember(0)]
        public ClientCloseType Type { get; set; }
    }

    [WZContract(LongMessage = true)]
    public class SShopItemList:IGameMessage
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

    [WZContract(Serialized =true)]
    public class STalk : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

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

    [WZContract(Serialized =true)]
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
            if(args.Length > 1)
                Arg1 = args[1];
        }
    }

    [WZContract]
    public class SBuy : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1,12)]
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
    public class SItemGet :IGameMessage
    {
        /// <summary>
        /// 0xFE: Zen
        /// </summary>
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1,12)]
        public byte[] ItemInfo { get; set; }

        public uint Money { get => BitConverter.ToUInt32(ItemInfo, 0).ShufleEnding(); set => ItemInfo = BitConverter.GetBytes(value.ShufleEnding()); }

        public SItemGet()
        {
            ItemInfo = Array.Empty<byte>();
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

    [WZContract(Serialized =true)]
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
    public class SDamage :IGameMessage
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
    public class SKillPlayer :IGameMessage
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
        public Spell MagicNumber { get; set; }

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
            MagicNumber = magic;
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
}

