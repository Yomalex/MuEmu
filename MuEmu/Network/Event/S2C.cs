using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

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

    [WZContract]
    public class BCScore
    {
        [WZMember(0, 12)]
        public byte[] btName { get; set; }

        [WZMember(1)]
        public int Score { get; set; }

        [WZMember(2)]
        public int Experience { get; set; }

        [WZMember(3)]
        public int Zen { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SBloodCastleReward : IEventMessage
    {
        [WZMember(0)]
        public byte Winner { get; set; }

        [WZMember(1)]
        public byte Type { get; set; }

        [WZMember(2, SerializerType = typeof(ArraySerializer))]
        public BCScore[] ScoreTable { get; set; }

        public SBloodCastleReward() { ScoreTable = Array.Empty<BCScore>(); }

        public SBloodCastleReward(bool winner, byte type, BCScore[] scores)
        {
            Winner = (byte)(winner?0x01:0x00);
            Type = type;
            ScoreTable = scores;
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

    [WZContract]
    public class DevilSquareScoreInfo
    {
        public byte rank;
        public object player;
        [WZMember(0,12)] public byte[] btName { get; set; }  // 0
        [WZMember(1)] public int TotalScore { get; set; } // C
        [WZMember(2)] public int BonusExp { get; set; }   // 10
        [WZMember(3)] public int BonusZen { get; set; }	// 14

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SDevilSquareResult :IEventMessage
    {
        [WZMember(0)]
        public byte MyRank { get; set; }    // 3
        //public byte Count; // 4
        [WZMember(1, typeof(ArrayWithScalarSerializer<byte>))]
        public DevilSquareScoreInfo[] Score { get; set; }	// 5
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

    // Kanturu
    [WZContract]
    public class SKanturuStateInfo : IEventMessage
    {
        [WZMember(0)] public KanturuState State { get; set; }
        [WZMember(1)] public byte btDetailState { get; set; } // 5
        [WZMember(2)] public byte btEnter { get; set; }   // 6
        [WZMember(3)] public byte btUserCount { get; set; }   // 7
        [WZMember(4)] public int iRemainTime { get; set; }	// 8
    }
}
