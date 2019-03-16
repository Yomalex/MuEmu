using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Event
{
    [WZContract]
    public class SEventRemainTime : IEventMessage
    {
        [WZMember(0)]
        public EventEnterType EventType { get; set; }

        [WZMember(1)]
        public byte RemainTime { get; set; }

        [WZMember(2)]
        public byte EnteredUser { get; set; }

        [WZMember(3)]
        public byte RemainTime_LOW { get; set; }
    }

    // LuckyCoins
    [WZContract]
    public class SLuckyCoinsCount : IEventMessage
    {
        [WZMember(0)]
        public uint Count { get; set; }

        public SLuckyCoinsCount()
        { }

        public SLuckyCoinsCount(uint count)
        {
            Count = count;
        }
    }

    [WZContract]
    public class SLuckyCoinsRegistre : IEventMessage
    { }

    // EventChip
    [WZContract]
    public class SEventChipInfo : IEventMessage
    {
        [WZMember(0)]
        public byte Type { get; set; }

        [WZMember(1)]
        public ushort ChipCount { get; set; }

        [WZMember(2)]
        public short MutoNum1 { get; set; }

        [WZMember(3)]
        public short MutoNum2 { get; set; }

        [WZMember(4)]
        public short MutoNum3 { get; set; }

        public SEventChipInfo() { }

        public SEventChipInfo(byte type, ushort cp, short[] MutoNum)
        {
            Type = type;
            ChipCount = cp;
            MutoNum1 = MutoNum[0];
            MutoNum2 = MutoNum[1];
            MutoNum3 = MutoNum[2];
        }
    }

    // BloodCastle
    [WZContract]
    public class SBloodCastleMove : IEventMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        public SBloodCastleMove() { }

        public SBloodCastleMove(byte result)
        {
            Result = result;
        }
    }

    [WZContract]
    public class SBloodCastleState : IEventMessage
    {
        [WZMember(0)]
        public byte State { get; set; }

        [WZMember(1)]
        public ushort RemainSec { get; set; }

        [WZMember(2)]
        public ushort MaxKillMonster { get; set; }

        [WZMember(3)]
        public ushort CurKillMonster { get; set; }

        [WZMember(4)]
        public ushort UserHaveWeapon { get; set; }
        
        [WZMember(5)]
        public byte Weapon { get; set; }


        public SBloodCastleState() { }

        public SBloodCastleState(byte state, ushort remainSec, ushort maxKillMonster, ushort curKillMonster, ushort userHaveWeapon, byte weapon)
        {
            State = state;
            RemainSec = remainSec;
            MaxKillMonster = maxKillMonster;
            CurKillMonster = curKillMonster;
            UserHaveWeapon = userHaveWeapon;
            Weapon = weapon;
        }
    }

    // Devil Square
    [WZContract]
    public class SDevilSquareSet : IEventMessage
    {
        [WZMember(0)]
        public DevilSquareState Type { get; set; }

        public SDevilSquareSet() { }

        public SDevilSquareSet(DevilSquareState type)
        {
            Type = type;
        }
    }

    // Crywolf
    [WZContract]
    public class SCrywolfBenefit : IEventMessage
    {
        [WZMember(0)]
        public byte PlusChaosRate { get; set; }

        public SCrywolfBenefit() { }

        public SCrywolfBenefit(byte pcr)
        {
            PlusChaosRate = pcr;
        }
    }
}
