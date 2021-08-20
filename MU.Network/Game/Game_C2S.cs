using MU.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Game
{
    [WZContract]
    public class CCheckSum : IGameMessage
    {
        [WZMember(0)]
        public ushort Key { get; set; }
    }

    [WZContract]
    public class CClientMessage : IGameMessage
    {
        [WZMember(0)]
        public HackCheck Flag { get; set; }
    }

    [WZContract]
    public class CCloseWindow : IGameMessage
    { }

    [WZContract]
    public class CTeleport : IGameMessage
    {
        [WZMember(0)] public byte Unk { get; set; }
        [WZMember(1)] public ushort MoveNumber { get; set; }
        [WZMember(2)] public byte X { get; set; }
        [WZMember(3)] public byte Y { get; set; }
        [WZMember(4)] public byte Unk2 { get; set; }

        //public ushort MoveNumber => wzMoveNumber.ShufleEnding();
    }

    [WZContract]
    public class CTeleportS9 : IGameMessage
    {
        [WZMember(1)] public ushort MoveNumber { get; set; }
        [WZMember(2)] public byte X { get; set; }
        [WZMember(3)] public byte Y { get; set; }

        //public ushort MoveNumber => wzMoveNumber.ShufleEnding();
    }

    [WZContract]
    public class CAction : IGameMessage
    {
        [WZMember(0)]
        public byte Dir { get; set; }

        [WZMember(1)]
        public byte ActionNumber { get; set; }

        //[WZMember(2)]
        public ushort btTarget { get; set; }

        public ushort Target
        {
            get => btTarget.ShufleEnding();
            set
            {
                btTarget = value.ShufleEnding();
            }
        }
    }

    [WZContract]
    public class CMove : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; } // 3

        [WZMember(1)]
        public byte Y { get; set; } // 4

        [WZMember(2,8)]
        public byte[] Path{ get; set; }   // 5 - 8
    }

    [WZContract]
    public class CMoveEng : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; } // 3

        [WZMember(1)]
        public byte Y { get; set; } // 4

        [WZMember(2, 8)]
        public byte[] Path { get; set; }   // 5 - 8
    }

    [WZContract]
    public class CChatNickname : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] Character { get; set; } // 3

        [WZMember(1, 60)]
        public byte[] Message { get; set; } // 4
    }
    
    [WZContract]
    public class CChatNumber : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; } // 3

        [WZMember(1, 60)]
        public byte[] Message { get; set; } // 4

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CChatWhisper : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] btId { get; set; }    // 3

        [WZMember(1, 60)]
        public byte[] btMessage { get; set; }   // D  

        public string Id => btId.MakeString();

        public string Message => btMessage.MakeString();
    };

    [WZContract]
    public class CPositionSet : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; }

        [WZMember(1)]
        public byte Y { get; set; }

        public Point Position => new Point(X, Y);
    }

    [WZContract]
    public class CPointAdd : IGameMessage
    {
        [WZMember(0)]
        public PointAdd Type { get; set; }
    }

    [WZContract]
    public class CClientClose : IGameMessage
    {
        [WZMember(0)]
        public ClientCloseType Type { get; set; }
    }

    [WZContract]
    public class CMoveItem : IGameMessage
    {
        [WZMember(0)]
        public MoveItemFlags sFlag { get; set; }

        [WZMember(1)]
        public byte Source { get; set; }

        [WZMember(2,12)]
        public byte[] ItemInfo { get; set; }

        [WZMember(3)]
        public MoveItemFlags tFlag { get; set; }

        [WZMember(4)]
        public byte Dest { get; set; }
    }

    [WZContract]
    public class CUseItem : IGameMessage
    {
        [WZMember(0)]
        public byte Source { get; set; }

        [WZMember(1)]
        public byte Dest { get; set; }

        [WZMember(2)]
        public byte Type { get; set; }
    }

    [WZContract]
    public class CItemGet : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        public ushort Number => wzNumber.ShufleEnding();
    }

    [WZContract]
    public class CEventEnterCount : IGameMessage
    {
        [WZMember(0)]
        public EventEnterType Type { get; set; }
    }

    [WZContract]
    public class CTalk : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; }
    }

    [WZContract]
    public class CWarehouseUseEnd : IGameMessage
    { }

    [WZContract]
    public class CBuy:IGameMessage
    {
        [WZMember(0)]
        public byte Position { get; set; }
    }

    [WZContract]
    public class CSell : IGameMessage
    {
        [WZMember(0)]
        public byte Position { get; set; }
    }

    [WZContract]
    public class CAttack : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }   // 3,4

        [WZMember(1)]
        public byte AttackAction { get; set; }  // 5

        [WZMember(2)]
        public byte DirDis { get; set; }    // 6

        public ushort Number => wzNumber.ShufleEnding();
    }

    [WZContract]
    public class CAttackS5E2 : IGameMessage
    {
        [WZMember(0)]
        public byte AttackAction { get; set; }  // 5

        [WZMember(1)]
        public byte DirDis { get; set; }    // 6

        [WZMember(2)]
        public ushort Number { get; set; }   // 3,4
    }

    [WZContract]
    public class CMagicAttack : IGameMessage
    {
        [WZMember(0)]
        public ushort wzMagicNumber { get; set; }

        [WZMember(1)]
        public ushort wzTarget { get; set; }

        [WZMember(2)]
        public byte Dis { get; set; }

        public ushort Target { get => wzTarget.ShufleEnding(); set => wzTarget = value.ShufleEnding(); }
        public Spell MagicNumber { get => (Spell)wzMagicNumber.ShufleEnding(); set => wzMagicNumber = ((ushort)value).ShufleEnding(); }
    }

    [WZContract]
    public class CMagicAttackS9 : IGameMessage
    {
        [WZMember(0)]
        public byte TargetH { get; set; }

        [WZMember(1)]
        public byte MagicNumberH { get; set; }
        [WZMember(2)]
        public byte TargetL { get; set; }

        [WZMember(3)]
        public byte MagicNumberL { get; set; }

        public ushort Target => (ushort)(TargetH << 8 | TargetL);
        public Spell MagicNumber => (Spell)(MagicNumberH << 8 | MagicNumberL);
    }

    [WZContract]
    public class CMagicDuration : IGameMessage
    {
        [WZMember(0)]
        public ushort wzMagicNumber { get; set; }
        [WZMember(1)]
        public byte X { get; set; }
        [WZMember(2)]
        public byte Y { get; set; }
        [WZMember(3)]
        public byte Dir { get; set; }
        [WZMember(4)]
        public byte Dis { get; set; }
        [WZMember(5)]
        public byte TargetPos { get; set; }

        [WZMember(6)]
        public ushort wzTarget { get; set; }

        [WZMember(7)]
        public byte MagicKey { get; set; }

        [WZMember(8,5)]
        public byte[] Unk { get; set; }

        public ushort Target { get => wzTarget.ShufleEnding(); set => wzTarget = value.ShufleEnding(); }
        public Spell MagicNumber
        {
            get => (Spell)wzMagicNumber.ShufleEnding();
            set => wzMagicNumber = ((ushort)value).ShufleEnding();
        }
    }

    [WZContract]
    public class CMagicDurationS9 : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; }
        [WZMember(1)]
        public byte MagicNumberH { get; set; }
        [WZMember(2)]
        public byte Y { get; set; }
        [WZMember(3)]
        public byte MagicNumberL { get; set; }
        [WZMember(4)]
        public byte Dir { get; set; }

        [WZMember(5)]
        public byte TargetH { get; set; }
        [WZMember(6)]
        public byte Dis { get; set; }

        [WZMember(7)]
        public byte TargetL { get; set; }

        [WZMember(8)]
        public byte TargetPos { get; set; }

        [WZMember(9)]
        public byte MagicKey { get; set; }

        public ushort Target => (ushort)(TargetH << 8 | TargetL);
        public Spell MagicNumber => (Spell)(MagicNumberH<<8|MagicNumberL);
    }

    [WZContract]
    public class CBeattackDto
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1)] public byte MagicKey { get; set; }

        public ushort Number => wzNumber.ShufleEnding();
    }

    [WZContract]
    public class CBeattack : IGameMessage
    {
        [WZMember(0)] public ushort wzMagicNumber { get; set; }
        [WZMember(1)] public byte X { get; set; }
        [WZMember(2)] public byte Y { get; set; }
        [WZMember(3)] public byte Serial { get; set; }
        //[WZMember(1)] public byte Count { get; set; }
        [WZMember(4, typeof(ArrayWithScalarSerializer<byte>))] public CBeattackDto[] Beattack { get; set; }

        public Spell MagicNumber => (Spell)wzMagicNumber.ShufleEnding();
        public Point Position => new Point(X, Y);
    }

    [WZContract]
    public class CBeattackS9Dto
    {
        [WZMember(0)] public byte NumberH { get; set; }   // 0
        [WZMember(1)] public byte MagicKey { get; set; }  // 1
        [WZMember(2)] public byte NumberL { get; set; }	// 2

        public ushort Number => (ushort)(NumberH << 8 | NumberL);
    }

    [WZContract]
    public class CBeattackS9 : IGameMessage
    {
        [WZMember(0)] public byte MagicNumberH { get; set; }
        [WZMember(1)] public byte Count { get; set; }
        [WZMember(2)] public byte MagicNumberL { get; set; }
        [WZMember(3)] public byte X { get; set; }
        [WZMember(4)] public byte Serial { get; set; }
        [WZMember(5)] public byte Y { get; set; }
        //[WZMember(1)] public byte Count { get; set; }
        [WZMember(6, typeof(ArraySerializer))] public CBeattackS9Dto[] Beattack { get; set; }

        public Spell MagicNumber => (Spell)(MagicNumberH<<8|MagicNumberL);
        public Point Position => new Point(X, Y);
    }

    [WZContract]
    public class CWarp : IGameMessage
    {
        [WZMember(0)]
        public int iCheckVal { get; set; }

        [WZMember(1)]
        public ushort MoveNumber { get; set; }
    }

    [WZContract]
    public class CDataLoadOK :IGameMessage
    { }

    [WZContract]
    public class CJewelMix : IGameMessage
    {
        [WZMember(0)]
        public byte JewelType { get; set; }

        [WZMember(1)]
        public byte JewelMix { get; set; }
    }

    [WZContract]
    public class CJewelUnMix : IGameMessage
    {
        [WZMember(0)]
        public byte JewelType { get; set; }

        [WZMember(1)]
        public byte JewelLevel { get; set; }

        [WZMember(2)]
        public byte JewelPos { get; set; }
    }

    [WZContract]
    public class CChaosBoxItemMixButtonClick : IGameMessage
    { }

    [WZContract]
    public class CChaosBoxItemMixButtonClickS5 : IGameMessage // Season 5 reference
    {
        [WZMember(0)] public ChaosMixType Type { get; set; }
        [WZMember(1)] public byte Info { get; set; }
    }

    [WZContract]
    public class CChaosBoxUseEnd : IGameMessage
    { }

    [WZContract]
    public class CInventory : IGameMessage
    { }

    [WZContract]
    public class CSkillKey : IGameMessage
    {
        [WZMember(0)]
        public byte subcode { get; set; }   // 3

        [WZMember(1, 20)]
        public byte[] SkillKey { get; set; }  // 4

        [WZMember(2)]
        public byte GameOption { get; set; }    // E

        [WZMember(3)]
        public byte QkeyDefine { get; set; }    // F

        [WZMember(4)]
        public byte WkeyDefine { get; set; }    // 10

        [WZMember(5)]
        public byte EkeyDefine { get; set; }    // 11

        [WZMember(6)]
        public byte ChatWindow { get; set; }    // 13

        [WZMember(7)]
        public byte RkeyDefine { get; set; }

        [WZMember(8)]
        public uint QWERLevelDefine { get; set; }
    }

    [WZContract]
    public class CItemThrow : IGameMessage
    {
        [WZMember(0)]
        public byte MapX { get; set; }   // 3

        [WZMember(1)]
        public byte MapY { get; set; }  // 4

        [WZMember(2)]
        public byte Source { get; set; }    // 5
    }

    [WZContract]
    public class CItemModify : IGameMessage
    {
        [WZMember(0)] public byte Position { get; set; }

        [WZMember(1)] public byte ReqPosition { get; set; }
    }

    [WZContract]
    public class CPShopSetItemPrice : IGameMessage
    {
        [WZMember(0)] public byte Position { get; set; }
        [WZMember(1)] public uint Price { get; set; }
    }

    [WZContract]
    public class CPShopRequestOpen : IGameMessage
    {
        [WZMember(0, 36)] public byte[] btName { get; set; }

        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CPShopRequestClose : IGameMessage
    { }

    [WZContract]
    public class CPShopRequestList : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1,10)] public byte[] btName { get; set; }
        public ushort Number => wzNumber.ShufleEnding();
        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CPShopRequestBuy : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1, 10)] public byte[] btName { get; set; }
        [WZMember(2)] public byte Position { get; set; }

        public ushort Number => wzNumber.ShufleEnding();
        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CPartyList : IGameMessage
    {

    }

    [WZContract]
    public class CPartyRequest : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CPartyRequestResult : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public ushort wzNumber { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CPartyDelUser : IGameMessage
    {
        [WZMember(0)]
        public byte Index { get; set; }
    }

    [WZContract]
    public class CDuelRequest : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1, 10)] public byte[] btName { get; set; }

        public CDuelRequest() { }
        public CDuelRequest(ushort number, string name)
        {
            wzNumber = number.ShufleEnding();
            btName = name.GetBytes();
        }
    }

    [WZContract]
    public class CDuelAnswer : IGameMessage
    {
        [WZMember(0)] public byte DuelOK { get; set; }
        [WZMember(1)] public ushort wzNumber { get; set; }

        public ushort Number => wzNumber.ShufleEnding();
    }

    [WZContract]
    public class CDuelLeave : IGameMessage { }

    [WZContract]
    public class CDuelJoinRoom : IGameMessage 
    {
        [WZMember(0)] public byte Room { get; set; }
    }

    [WZContract]
    public class CDuelLeaveRoom : IGameMessage
    {
        [WZMember(0)] public byte Room { get; set; }
    }

    #region Friend
    [WZContract]
    public class CFriendList : IGameMessage { }

    [WZContract]
    public class CFriendAdd : IGameMessage
    {
        [WZMember(0,10)] public byte[] btName { get; set; }

        public string Name => btName.MakeString();
    }
    #endregion

    [WZContract]
    public class CMasterSkill : IGameMessage
    {
        [WZMember(0)]
        public Spell MasterSkill { get; set; }

        [WZMember(1)]
        public ushort MasterEmpty { get; set; }
    }

    [WZContract]
    public class CTradeRequest : IGameMessage
    {
        [WZMember(0)]
        public ushort wzNumber { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); }
    }

    [WZContract]
    public class CTradeResponce : IGameMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        /*[WZMember(1, 10)]
        public byte[] szId { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public int GuildNumber { get; set; }*/
    }

    [WZContract]
    public class CTradeMoney : IGameMessage
    {
        [WZMember(0)]
        public uint Money { get; set; }
    }

    [WZContract]
    public class CTradeButtonOk : IGameMessage
    {
        [WZMember(0)]
        public byte Flag { get; set; }
    }

    [WZContract]
    public class CTradeButtonCancel : IGameMessage
    { }

    [WZContract]
    public class CFriendAddReq : IGameMessage
    {
        [WZMember(0, 10)] public byte[] btName { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class CWaitFriendAddReq : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }

        [WZMember(1, 10)] public byte[] btName { get; set; }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class CMemberPosInfoStart : IGameMessage
    { }

    [WZContract]
    public class CMemberPosInfoStop : IGameMessage
    { }

    [WZContract]
    public class CNPCJulia : IGameMessage
    { }

    [Flags]
    public enum HuntingFlags19 : byte
    {
        AutoPotion=0x01,
        DrainLife = 0x04,
        LongDistanceC = 0x08,
        OriginalPosition = 0x10,
        UseSkillClosely = 0x20,
        Party = 0x40,
        PreferenceOfParty = 0x80,
    }

    [Flags]
    public enum HuntingFlags1A : byte
    {
        BuffTimeParty = 0x01,
        BuffDuration = 0x04,
        Delay = 0x08,
        Condition = 0x10,
        MonsterAttacking = 0x20,
        Cond1 = 0x40,
        Cond2 = 0x80,
        Cond3 = 0xC0,
    }

    [Flags]
    public enum HuntingFlags1B : byte
    {
        Delay = 0x01,
        Condition = 0x02,
        MonsterAttacking = 0x04,
        Cond1 = 0x08,
        Cond2 = 0x10,
        Cond3 = 0x18,
        Repair = 0x20,
        PickAllNearItems = 0x40,
        PickSelectedItems = 0x80,
    }

    [Flags]
    public enum HuntingFlags1C : byte
    {
        Delay = 0x01,
        Condition = 0x02,
        AutoAcceptFriend = 0x04,
        AutoAcceptGuild = 0x08,
        UseElitePotion = 0x10,
        UseSkillClosely = 0x20,
        UseRegularAttackArea = 0x40,
        PickSelectedItems = 0x80,
    }

    [Flags]
    public enum OptainingFlags:byte
    {
        Unk = 0x01,
        Jewels = 0x08,
        SetItem = 0x10,
        ExcellentItem = 0x20,
        Zen = 0x40,
        ExtraItem = 0x80,
    }

    [WZContract(LongMessage = true)]
    public class CMUBotData : IGameMessage
    {
        /*[WZMember(0, typeof(BinarySerializer), 257)]
        public byte[] Data { get; set; }*/
        [WZMember(0)] public byte Data0 { get; set; }
        [WZMember(1)] public OptainingFlags OptainingFlags { get; set; }
        [WZMember(2)] public byte Data2 { get; set; }
        [WZMember(3)] public byte OPDelayTime { get; set; }
        [WZMember(4)] public ushort BasicSkill { get; set; }
        [WZMember(6)] public ushort ActivationSkill { get; set; }
        [WZMember(8)] public ushort DelayTime { get; set; }
        [WZMember(0xA)] public ushort ActivationSkill2 { get; set; }
        [WZMember(0xC)] public ushort DelayTime2 { get; set; }
        [WZMember(0xE)] public ushort Unk0E { get; set; }
        [WZMember(0x10)] public ushort Buff1 { get; set; }
        [WZMember(0x12)] public ushort Buff2 { get; set; }
        [WZMember(0x14)] public ushort Buff3 { get; set; }
        [WZMember(0x16)] public byte Unk16 { get; set; }
        [WZMember(0x17)] public byte AutoPotion_Heal { get; set; }
        [WZMember(0x18)] public byte AutoDrainLife_Party { get; set; }
        [WZMember(0x19)] public HuntingFlags19 Flags19 { get; set; }
        [WZMember(0x1A)] public HuntingFlags1A Flags1A { get; set; }
        [WZMember(0x1B)] public HuntingFlags1B Flags1B { get; set; }
        [WZMember(0x1C)] public HuntingFlags1C Flags1C { get; set; }
        [WZMember(0x1D, typeof(BinarySerializer), 36)] public byte[] Data1D { get; set; }
        [WZMember(0x41, typeof(BinaryStringSerializer), 16)] public string ExtraItem1 { get; set; }
        [WZMember(0x51, typeof(BinaryStringSerializer), 16)] public string ExtraItem2 { get; set; }
        [WZMember(0x61, typeof(BinaryStringSerializer), 16)] public string ExtraItem3 { get; set; }
        [WZMember(0x71, typeof(BinaryStringSerializer), 16)] public string ExtraItem4 { get; set; }
        [WZMember(0x81, typeof(BinaryStringSerializer), 16)] public string ExtraItem5 { get; set; }
        [WZMember(0x91, typeof(BinaryStringSerializer), 16)] public string ExtraItem6 { get; set; }
        [WZMember(0xA1, typeof(BinaryStringSerializer), 16)] public string ExtraItem7 { get; set; }
        [WZMember(0xB1, typeof(BinaryStringSerializer), 16)] public string ExtraItem8 { get; set; }
        [WZMember(0xC1, typeof(BinaryStringSerializer), 16)] public string ExtraItem9 { get; set; }
        [WZMember(0xD1, typeof(BinaryStringSerializer), 16)] public string ExtraItem10 { get; set; }
        [WZMember(0xE1, typeof(BinaryStringSerializer), 16)] public string ExtraItem11 { get; set; }
        [WZMember(0xF1, typeof(BinaryStringSerializer), 16)] public string ExtraItem12 { get; set; }
        public int AutoDrainLife
        {
            get => AutoDrainLife_Party & 0x0F;
            set
            {
                AutoDrainLife_Party &= 0xF0;
                AutoDrainLife_Party |= (byte)value;
            }
        }
        public int AutoPartyHeal
        {
            get => (AutoDrainLife_Party >> 4) & 0x0F;
            set
            {
                AutoDrainLife_Party &= 0x0F;
                AutoDrainLife_Party |= (byte)(value << 4);
            }
        }
        public int AutoPotion
        {
            get => AutoPotion_Heal & 0x0F;
            set
            {
                AutoPotion_Heal &= 0xF0;
                AutoPotion_Heal |= (byte)value;
            }
        }
        public int AutoHeal
        {
            get => (AutoPotion_Heal >> 4) & 0x0F;
            set
            {
                AutoPotion_Heal &= 0x0F;
                AutoPotion_Heal |= (byte)(value << 4);
            }
        }
        public int HuntingRange
        {
            get => Data2 & 0x0F;
            set
            {
                Data2 &= 0xF0;
                Data2 |= (byte)value;
            }
        }
        public int OptainingRange
        {
            get => (Data2>>4) & 0x0F;
            set
            {
                Data2 &= 0x0F;
                Data2 |= (byte)(value<<4);
            }
        }
    }

    [WZContract]
    public class CMuHelperState : IGameMessage
    {
        [WZMember(0)]
        public byte State { get; set; }
    }

    [WZContract]
    public class CQuestExp : IGameMessage
    { }

    [WZContract]
    public class CShadowBuff : IGameMessage
    { }

    [WZContract]
    public class CGremoryCaseOpen : IGameMessage
    { }
}
