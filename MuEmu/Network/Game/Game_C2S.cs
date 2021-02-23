using MuEmu.Network.Serializers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Game
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
    public class CNewQuestInfo : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; } // 3,4

        [WZMember(1)]
        public ushort Quest { get; set; } // 5,6

        //[WZMember(2)]
        //public ushort Unk1 { get; set; } // 7

        //[WZMember(3)]
        //public ushort Unk2 { get; set; } // 8

        //[WZMember(4)]
        //public ushort Unk3 { get; set; } // 9

        //[WZMember(5)]
        //public ushort Unk4 { get; set; } // A
    }

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

        public ushort Target => wzTarget.ShufleEnding();
        public Spell MagicNumber => (Spell)wzMagicNumber.ShufleEnding();
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
}
