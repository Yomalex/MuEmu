using MU.Network.Game;
using MU.Resources;
using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Event
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
    public class SCrywolfState : IEventMessage
    {
        [WZMember(0)]
        public byte Occupation { get; set; }
        [WZMember(1)]
        public byte State { get; set; }
    }
    [WZContract]
    public class SCrywolfLeftTime : IEventMessage
    {
        [WZMember(0)]
        public byte Hour { get; set; }
        [WZMember(1)]
        public byte Minute { get; set; }

        public TimeSpan TimeLeft { 
            get => TimeSpan.FromHours(Hour + Minute / 30.0f);
            set
            {
                Hour = (byte)value.Hours;
                Minute = (byte)value.Minutes;
            }
        }
    }
    [WZContract]
    public class SCrywolfStatueAndAltarInfo : IEventMessage
    {
        [WZMember(0)]
        public int StatueHP { get; set; }
        [WZMember(1,5)]
        public byte[] AltarState { get; set; }
    }
    [WZContract]
    public class SCrywolfBossMonsterInfo : IEventMessage
    {
        [WZMember(0)]
        public int MonsterHP { get; set; }
        [WZMember(1)]
        public byte Monster { get; set; }
    }
    [WZContract]
    public class SCrywolfStageEffect : IEventMessage
    {
        [WZMember(0)]
        public byte Active { get; set; }
    }
    [WZContract]
    public class SCrywolfPersonalRank : IEventMessage
    {
        [WZMember(0)]
        public byte Rank { get; set; }
        [WZMember(1)]
        public int Exp { get; set; }
    }
    [WZContract]
    public class SCrywolfHeroList : IEventMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))]
        public CrywolfHeroDto[] Heros { get; set; }
    }
    [WZContract]
    public class CrywolfHeroDto : IEventMessage
    {
        [WZMember(0)]
        public byte Rank { get; set; }

        [WZMember(1, 10)]
        public byte[] btName { get; set; }

        [WZMember(2)]
        public int Score { get; set; }

        [WZMember(3)]
        public HeroClass Class { get; set; }
    }

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

    [WZContract]
    public class SKanturuStateChange : IEventMessage
    {
        [WZMember(0)] public KanturuState State { get; set; }
        [WZMember(1)] public byte btDetailState { get; set; }
    }

    [WZContract]
    public class SKanturuMonsterUserCount : IEventMessage
    {
        [WZMember(0)] public byte MonsterCount { get; set; }
        [WZMember(1)] public byte UserCount { get; set; }
    }

    [WZContract]
    public class SKanturuBattleTime : IEventMessage
    {
        //[WZMember(0)] public byte Padding02 { get; set; }
        [WZMember(1)] public int BattleTime { get; set; }
    }

    [WZContract]
    public class SKanturuBattleResult : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SKanturuWideAttack : IEventMessage
    {
        [WZMember(0)] public ushortle ObjClass{get; set; } // 4
        [WZMember(1)] public byte Type { get; set; }	// 6
    }

    // Imperial
    [WZContract]
    public class SImperialEnterResult : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte Day { get; set; } // 5
        [WZMember(2)] public byte State { get; set; }   // 6
        [WZMember(3)] public byte Unk { get; set; }   // 7
        [WZMember(4)] public ushort Index { get; set; }	// 8
        [WZMember(5)] public ushort EntryTime { get; set; }	// A
    }

    [WZContract]
    public class SImperialNotifyZoneTime : IEventMessage
    {
        [WZMember(0)] public byte MsgType { get; set; }
        [WZMember(1)] public byte DayOfWeek { get; set; }
        [WZMember(2)] public ushort ZoneIndex { get; set; }
        [WZMember(3)] public uint RemainTime { get; set; }
        [WZMember(4)] public uint RemainMonster { get; set; }
    }

    [WZContract]
    public class SImperialNotifyZoneClear : IEventMessage
    {
        [WZMember(0)] public uint Type { get; set; }
        [WZMember(1)] public uint RewardExp { get; set; }
    }

    // ArcaBattle
    [WZContract]
    public class SArcaBattleState : IEventMessage
    {
        [WZMember(0)] public byte State { get; set; }
    }

    [WZContract]
    public class SSendBanner : IEventMessage
    {
        [WZMember(0)] public BannerType Type { get; set; }
    }

    [WZContract]
    public class SEventInventoryOpenS16 : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public EventInventoryType Id { get; set; }
        [WZMember(2)] public int EventTime { get; set; }
        [WZMember(3)] public byte Data { get; set; }
    }

    [WZContract(Serialized = true)]
    public class SEventItemGet : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, 12)] public byte[] Item { get; set; }

        public SEventItemGet() { }
        public SEventItemGet(byte result, byte[] item, ushort index)
        {
            Result = result;
            Item = item;
        }
    }

    [WZContract(Serialized = true)]
    public class SEventItemGetS16 : IEventMessage
    {
        [WZMember(0)] public byte Junk1 { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte IndexH { get; set; }
        [WZMember(3)] public byte Junk2 { get; set; }
        [WZMember(4)] public byte IndexL { get; set; }
        [WZMember(5, 12)] public byte[] Item { get; set; }

        public SEventItemGetS16() { }
        public SEventItemGetS16(byte result, byte[] item, ushort index)
        {
            Result = result;
            Item = item;
            IndexH = (byte)(index >> 8);
            IndexL = (byte)(index & 0xFF);
        }
    }

    [WZContract]
    public class SEventItemThrow : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte Pos { get; set; }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SEventInventory : IInventory, IEventMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public InventoryDto[] Inventory { get; set; }

        public void LoadItems(IEnumerable<AInventoryDto> items)
        {
            Inventory = items.Select(x => x as InventoryDto).ToArray();
        }
    }

    [WZContract(LongMessage = true, Serialized = true)]
    public class SEventInventoryS17 : IInventory, IEventMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public InventoryS17Dto[] Inventory { get; set; }
        public void LoadItems(IEnumerable<AInventoryDto> items)
        {
            Inventory = items.Select(x => x as InventoryS17Dto).ToArray();
        }
    }

    [WZContract]
    public class SAcheronEventEnter : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class MuRummyCardInfo
    {
        [WZMember(0)] public byte Color { get; set; }
        [WZMember(1)] public byte Number { get; set; }
        [WZMember(2)] public byte Slot { get; set; }
    }

    [WZContract]
    public class SMuRummyStart : IEventMessage
    {
        [WZMember(0)] public ushortle Score { get; set; }
        [WZMember(1)] public byte CardCount { get; set; }
        [WZMember(2)] public byte SpecialCardCount { get; set; }
        [WZMember(3)] public byte Unk { get; set; }
        [WZMember(4)] public byte Type { get; set; }
        [WZMember(5, typeof(ArraySerializer))] public byte[] SlotStatus { get; set; } //10
        [WZMember(6, typeof(ArraySerializer))] public MuRummyCardInfo[] CardInfo { get; set; } //6
    }

    [WZContract]
    public class SMuRummyCardList : IEventMessage
    {
        [WZMember(0, typeof(ArraySerializer))] public MuRummyCardInfo[] CardInfo { get; set; } //6
    }

    [WZContract]
    public class SMuRummyReveal : IEventMessage
    {
        [WZMember(0, typeof(ArraySerializer))] public MuRummyCardInfo[] CardInfo { get; set; } //6
        [WZMember(1)] public byte CardCount { get; set; }
        [WZMember(2)] public byte SpecialCardCount { get; set; }
    }

    [WZContract]
    public class SMuRummyPlayCard : IEventMessage
    {
        [WZMember(0)] public byte From { get; set; }
        [WZMember(1)] public byte To { get; set; }
        [WZMember(2)] public byte Color { get; set; }
        [WZMember(3)] public byte Number { get; set; }
    }

    [WZContract]
    public class SMuRummyMatch : IEventMessage
    {
        [WZMember(0)] public ushortle Score { get; set; } = new ushortle(0);
        [WZMember(1)] public ushortle TotalScore { get; set; } = new ushortle(0);
        [WZMember(2)] public byte Result { get; set; }
    }
    [WZContract]
    public class SMuRummyExit : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        //C1 04 4D 15
    }

    [WZContract]
    public class SMuRummyMessage : IEventMessage
    {
        [WZMember(0)] public byte Index { get; set; }
        [WZMember(1)] public ushortle Value { get; set; } = new ushortle(0);
    }

    [WZContract]
    public class SMineSweeperOpen : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte RemainBombs { get; set; }
        [WZMember(2)] public ushort Count { get; set; }
        [WZMember(3)] public ushort CurrentScore { get; set; }
        [WZMember(4, typeof(ArraySerializer))] public ushort[] Cells { get; set; }
    }

    [WZContract]
    public class SMineSweeperStart : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SMineSweeperCreateCell : IEventMessage
    {
        [WZMember(0)] public uint Time { get; set; }
        [WZMember(1)] public byte X { get; set; }
        [WZMember(2)] public byte Y { get; set; }
        [WZMember(3)] public byte Effect { get; set; }
    }

    [WZContract]
    public class SMineSweeperReveal : IEventMessage
    {
        [WZMember(0)] public byte Cell { get; set; }
        [WZMember(1)] public ushortle Score { get; set; }
        [WZMember(4, typeof(ArrayWithScalarSerializer<byte>))] public ushort[] Cells { get; set; }
    }

    [WZContract]
    public class SMineSweeperMark : IEventMessage
    {
        [WZMember(0)] public byte Cell { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte RemainBombs { get; set; }
    }

    [WZContract]
    public class SMineSweeperEnd : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte Count { get; set; }
        [WZMember(2)] public ushort Score { get; set; }
        [WZMember(3)] public ushort BombsFound { get; set; }
        [WZMember(4)] public ushort BombsFailure { get; set; }
        [WZMember(5)] public ushort SteppedOnBomb { get; set; }
        [WZMember(6)] public ushort Clear { get; set; }
        [WZMember(7)] public ushort TotalScore { get; set; }
        [WZMember(8, typeof(ArraySerializer))] public ushort[] Cells { get; set; }
    }

    [WZContract]
    public class SMineSweeperGetReward : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SJewelBingoState : IEventMessage
    {
        [WZMember(0)] public JBState State { get; set; }
    }

    [WZContract]
    public class SJewelBingoInfo : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, typeof(ArraySerializer))] public JBType[] Grid { get; set; } // 25 (5*5)
        [WZMember(2, typeof(ArraySerializer))] public byte[] CurrentJewel { get; set; }// 6
    }

    [WZContract]
    public class SJewelBingoPlayInfo : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, typeof(ArraySerializer))] public JBType[] Grid { get; set; } // 25 (5*5)
        [WZMember(2, typeof(ArraySerializer))] public byte[] MatchingJewel { get; set; }// 12
        [WZMember(3)] public JBType CurrentJewel { get; set; }
        [WZMember(4)] public byte JewelCount { get; set; }
        [WZMember(5)] public byte CurrentBox { get; set; }
    }

    [WZContract]
    public class SJewelBingoPlayResult : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, typeof(ArraySerializer))] public JBType[] Grid { get; set; } // 25 (5*5)
        [WZMember(2, typeof(ArraySerializer))] public byte[] MatchingJewel { get; set; }// 12
        //[WZMember(3)] public byte unk { get; set; }
        [WZMember(4)] public ushort LuckyClear { get; set; }
        [WZMember(5)] public ushort NormalClear { get; set; }
        [WZMember(6)] public ushort JewelryClear { get; set; }
    }

    [WZContract]
    public class SJewelBingoBox : IEventMessage
    { }

    [WZContract]
    public class SBallsAndCowsOpen : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public byte Junk { get; set; }
        [WZMember(2)] public ushort Score { get; set; }
        [WZMember(3, typeof(ArraySerializer))] public byte[] Strikes { get; set; } //5
        [WZMember(4, typeof(ArraySerializer))] public byte[] Ball { get; set; }//5
        [WZMember(5, typeof(ArraySerializer))] public byte[] Numbers { get; set; }//5*3
    }

    [WZContract]
    public class SBallsAndCowsStart : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SBallsAndCowsResult : IEventMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, typeof(ArraySerializer))] public byte[] Strikes{ get; set; } //5
        [WZMember(2, typeof(ArraySerializer))] public byte[] Ball { get; set; } //5
        [WZMember(3, typeof(ArraySerializer))] public byte[] Numbers { get; set; } //5*3
        [WZMember(4)] public byte Data4 { get; set; }
        [WZMember(5)] public byte Junk { get; set; }
        [WZMember(6)] public ushort Score { get; set; }
    }
}
