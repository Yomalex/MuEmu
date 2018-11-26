using MuEmu.Network.Data;
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
        ushort Number { get; set; }   // 3,4

        [WZMember(1)]
        byte Dir { get; set; }  // 5

        [WZMember(2)]
        byte ActionNumber { get; set; }  // 6

        [WZMember(3)]
        ushort Target { get; set; } // 7,8

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
        ushort Number { get; set; }   // 3,4

        [WZMember(0)]
        byte X{ get; set; } // 5

        [WZMember(0)]
        byte Y { get; set; } // 6

        [WZMember(0)]
        byte Path { get; set; }	// 7

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
    public class SKill
    {
        [WZMember(0)]
        public ushort Target { get; set; }

        [WZMember(1)]
        public ushort Experience { get; set; }

        [WZMember(2)]
        public ushort Damage { get; set; }
    }

    [WZContract]
    public class SDie
    {
        [WZMember(0)]
        public ushort Target { get; set; }

        [WZMember(1)]
        public ushort Spell { get; set; }

        [WZMember(2)]
        public ushort Killer { get; set; }
    }

    [WZContract]
    public class SEventEnterCount :IGameMessage
    {
        [WZMember(0)]
        public EventEnterType Type { get; set; }

        [WZMember(1)]
        public byte Left { get; set; }
    }
}

