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
        public ushort Key { get; set; }
    }

    [WZContract]
    public class SWeather : IGameMessage
    {
        [WZMember(0)]
        public byte Weather { get; set; }
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
        public byte type { get; set; }

        [WZMember(1)]
        public byte btCount { get; set; }

        [WZMember(2)]
        public ushort wDelay { get; set; }

        [WZMember(3)]
        public int dwColor { get; set; }

        [WZMember(4)]
        public byte btSpeed { get; set; }

        [WZMember(5, SerializerType = typeof(ArraySerializer))]
        public byte[] btNotice { get; set; } // 256

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
}
