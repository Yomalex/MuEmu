using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Network
{
    public enum ConOpCode : ushort
    {
        CSWelcome = 0x0100,
        GSJoin = 0xFF10,
        GSKeep = 0xFF11,
        GSClientAdd = 0xFF12,
        GSClientChat = 0xFF13,
        GSClientRem = 0xFF14,
    }


    public enum GlobalOpCode : ushort
    {
        LiveClient = 0xFF0E,
    }

    public enum AHOpCode : ushort
    {
        AHCheck = 0x11FA,
    }

    public enum CSOpCode : ushort
    {
        JoinResult = 0x00F1,
        Login = 0x01F1,
        CharacterList = 0x00F3,
        CharacterCreate = 0x01F3,
        CharacterDelete = 0x02F3,
        JoinMap2 = 0x03F3,
        JoinMap = 0x15F3,
        ServerMove = 0x00B1,
        ServerMoveAuth = 0x01B1,


        Resets = 0x0BFA,
    }

    public enum GameOpCode : ushort
    {
        MapMoveCheckSum = 0x018E,
        Warp = 0x028E,

        DuelRequest = 0x01AA,
        DuelAnswer = 0x02AA,
        DuelLeave = 0x03AA,
        DuelScoreBroadcast = 0x04AA,
        DuelHPBroadcast = 0x05AA,
        DuelChannelList = 0x06AA,
        DuelRoomJoin = 0x07AA,
        DuelRoomJoinBroadcast = 0x08AA,
        DuelRoomLeave = 0x09AA,
        DuelRoomLeaveBroadcast = 0x0AAA,
        DuelRoomObserversBroadcast = 0x0BAA,
        DuelResultBroadcast = 0x0CAA,
        DuelRoundBroadcast = 0x0DAA,

        Tax = 0x1AB2,
        KillCount = 0x01B8,
        ClientClose = 0x02F1,
        ClientMessage = 0x03F1,
        CharRegen = 0x04F3,
        LevelUp = 0x5F3,
        PointAdd = 0x06F3,
        Damage = 0x07F3,
        Inventory = 0x10F3,
        Spells = 0x11F3,
        DataLoadOK = 0x12F3,
        Equipament = 0x13F3,
        OneItemSend = 0x14F3,
        SkillKey = 0x30F3,
        Command = 0x40F3,

        MasterLevelInfo = 0x50F3,
        MasterLevelUp = 0x51E0,
        MasterLevelSkill = 0x52F3,

        NewQuestInfo = 0x1AF6,
        QuestDetails = 0x1BF6,
        QuestWindow = 0x01F9,
        JewelMix = 0x00BC,
        JewelUnMix = 0x01BC,
        GeneralChat0 = 0xFF00,
        GeneralChat1 = 0xFF01,
        WhisperChat = 0xFF02,
        GameSecurity = 0xFF03,
        ViewSkillState = 0xFF07,
        EventState = 0xFF0B,
        Notice = 0xFF0D,
        Weather = 0xFF0F,
        Beattack = 0xFF10,
        ViewPortCreate = 0xFF12,
        ViewPortMCreate = 0xFF13,
        ViewPortDestroy = 0xFF14,
        AttackResult = 0xFF15,
        KillPlayer = 0xFF16,
        DiePlayer = 0xFF17,
        Rotation = 0xFF18,
        MagicAttack = 0xFF19,
        Teleport = 0xFF1C,
        MagicDuration = 0xFF1E,
        ViewPortMCall = 0xFF1F,
        ViewPortItemCreate = 0xFF20,
        ViewPortItemDestroy = 0xFF21,
        ItemGet = 0xFF22,
        ItemThrow = 0xFF23,
        MoveItem = 0xFF24,
        HealthUpdate = 0xFF26,
        ManaUpdate = 0xFF27,
        InventoryItemDelete = 0xFF28,
        ItemUseSpecialTime = 0xFF29,
        InventoryItemDurUpdate = 0xFF2A,
        Talk = 0xFF30,
        CloseWindow = 0xFF31,
        Buy = 0xFF32,
        Sell = 0xFF33,
        ItemModify = 0xFF34,
        TradeRequest = 0xFF36,
        TradeResponce = 0xFF37,
        TradeOtherAdd = 0xFF39,
        TradeMoney = 0xFF3A,
        TradeOtherMoney = 0xFF3B,
        TradeButtonOk = 0xFF3C,
        TradeButtonCancel = 0xFF3D,
        ViewPortPShop = 0x003F,
        PShopSetItemPrice = 0x013F,
        PShopRequestOpen = 0x023F,
        PShopRequestClose = 0x033F,
        PShopRequestList = 0x053F,
        PShopRequestBuy = 0x063F,
        PShopRequestSold = 0x083F,
        PartyRequest = 0xFF40,
        PartyResult = 0xFF41,
        PartyList = 0xFF42,
        PartyDelUser = 0x0FF43,
        PartyLifeUpdate = 0x0FF44,
        ViewPortChange = 0xFF45,
        SetMapAtt = 0xFF46,
        Effect = 0xFF48,
        ViewPortGuildCreate = 0xFF65,
        WarehouseMoney = 0xFF81,
        WarehouseUseEnd = 0xFF82,
        ChaosBoxItemMixButtonClick = 0xFF86,
        ChaosBoxUseEnd = 0xFF87,
        EventEnterCount = 0xFF9F,
        QuestInfo = 0xFFA0,

        FriendList = 0xFFC0,
        FriendAdd = 0xFFC1,
        FriendAddWait = 0xFFC2,
        FriendDel = 0xC3,
        FriendState = 0xC4,
        LetterSend = 0xC5,
        AddLetter = 0xFFC6,
        LetterRead = 0xC7,
        LetterDel = 0xC8,
        LetterList = 0xC9,

        Move = 0xFFD3,
        MoveEng = 0xFFD4,
        Attack = 0xFFD7,
        Position = 0xFFDF,
        MuunInventory = 0x024E,
        MuunRideRequest = 0x114E,
        MuunRideViewPort = 0x144E,

        MiniMapNPC = 0x03E7,
        PeriodItemCount = 0x11D2,

        PentagramaJInfo = 0x01EE,
        UBFInfo = 0x01CD,
        PopUpType = 0x26F3,

        MemberPosInfoStart = 0x01E7,
        MemberPosInfoStop = 0x02E7,
    }

    public enum GensOpCode
    {
        SendGensInfo = 0x07F8,
    }

    public enum GuildOpCode
    {
        GuildRequest = 0xFF50,
        GuildResult = 0xFF51,
        GuildListAll = 0xFF52,
        RemoveUser = 0xFF53,
        MasterQuestion = 0xFF54,
        GuildSaveInfo = 0xFF55,
        GuildViewPort = 0xFF65,
        GuildReqViewport = 0xFF66,
        GuildSetStatus = 0xFFE1,
    }

    public enum EventOpCode
    {
        RemainTime = 0xFF91,
        LuckyCoinsCount = 0x0BBF,
        LuckyCoinsRegistre = 0x0CBF,

        DevilSquareMove = 0xFF90,
        DevilSquareSet = 0xFF92,

        BloodCastleReward = 0xFF93,

        EventChipInfo = 0xFF94,

        BloodCastleEnter = 0xFF9A,
        BloodCastleState = 0xFF9B,

        ChaosCastleMove = 0x01AF,

        CrywolfState = 0x00BD,
        CrywolfStatueAndAltarInfo = 0x02BD,
        CrywolfContract = 0x03BD,
        CrywolfLeftTime = 0x04BD,
        CrywolfBossMonsterInfo = 0x05BD,
        CrywolfStageEffect = 0x06BD,
        CrywolfPersonalRank = 0x07BD,
        CrywolfHeroList = 0x08BD,
        CrywolfBenefit = 0x09BD,

        KanturuState = 0x00D1,

        ImperialGuardianEnter = 0x01F7,
        ImperialGuardianEnterResult = 0x02F7,

        ArcaBattleState = 0x38F8,
        Banner = 0x184D,
    }
    public enum CashOpCode : ushort
    {
        CashItems = 0x05D0,
        CashPoints = 0x04F5,
    }

    public enum PCPShopOpCode : ushort
    {
        PCPShopPoints = 0x04D0,
        PCPShopBuy = 0x05D0,
        PCPShopInfo = 0x06D0,
        PCPShopItems = 0xFF31,
    }

    public enum QuestOpCode : ushort
    {
        SetQuest = 0xFFA1,
        SetQuestState = 0xFFA2,
        QuestPrize = 0xFFA3,
    }
}
