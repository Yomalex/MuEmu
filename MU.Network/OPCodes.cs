using System;
using System.Collections.Generic;
using System.Text;

namespace MU.Network
{
    public enum ConOpCode : ushort
    {
        CSWelcome = 0x014A,
        GSJoin = 0xFF10, //S17
        GSKeep = 0xFF11, //S17
        GSClientAdd = 0xFF12, //S17
        GSClientChat = 0xFF13, //S17
        GSClientRem = 0xFF14, //S17
        ServerList = 0x06F4, //S17
    }


    public enum GlobalOpCode : ushort
    {
        LiveClient = 0xFF24, //S17
    }

    public enum AHOpCode : ushort
    {
        AHCheck = 0x11FA, //S17
        AHEncKey = 0x00FA, //S17
    }

    public enum CSOpCode : ushort
    {
        JoinResult = 0x003A, //S17 

        Login = 0xFD3A, //S17
        LoginRes = 0x02F3, //S17

        CharacterList = 0x0482, //S17
        CharacterListRes = 0x6052, //S17

        CharacterCreate = 0x0082, //S17
        CharacterCreateRes = 0x2652, //S17

        CharacterDelete = 0x0782, //S17
        CharacterDeleteRes = 0x0652, //S17

        JoinMap2 = 0x1482, //S17
        JoinMap2Res = 0x0052, //S17

        JoinMap = 0x0582, //S17
        JoinMapRes = 0x0152, //S17

        ServerMove = 0x0073, //S17
        ServerMoveAuth = 0x014B, //S17

        ResetList = 0x0AFA, //S17
        Resets = 0x0BFA, //S17
        EnableCreate = 0x00DE, //S17

        ChannelList = 0x3151, //S17
        ChannelListRes = 0x58F7, //S17
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

        ItemSplit = 0x00E8, //S17

        Tax = 0x1ECA, //S17
        KillCount = 0x01B8, //S17

        ClientClose = 0xFE3A, //S17
        ClientCloseRes = 0x01F3, //S17

        ClientMessage = 0x03F3, //S17
        Eventnotification = 0x013A, //S17
        CharRegen = 0x2082, //S17
        LevelUp = 0x6082, //S17
        PointAdd = 0x2582, //S17
        PointAddRes = 0x1552, //S17
        Damage = 0x0682, //S17
        Inventory = 0x2682, //S17
        InventoryRes = 0x1052, //S17

        Spells = 0x5382, //S17
        DataLoadOK = 0x1652, //S17
        Equipament = 0x5282, //S17
        OneItemSend = 0x5182, //S17
        SkillKey = 0x1082, //S17
        Command = 0x3582, //S17

        MasterLevelInfo = 0x1182, //S17
        MasterLevelUp = 0x0182, //S17
        MasterLevelSkill = 0x1682, //S17
        MasterLevelSkillRes = 0x0352, //S17

        NewQuestInfo = 0x1AF6,
        QuestDetails = 0x1BF6,
        QuestWindow = 0x01F9,
        JewelMix = 0x00BC,
        JewelUnMix = 0x01BC,
        GeneralChat0 = 0xFF00, //S17
        GeneralChat0Res = 0xFF4A, //S17
        GeneralChat1 = 0xFF01, //S17
        WhisperChat = 0xFF02, //S17

        GameSecurity = 0xFF00, //S17
        GameSecurityRes = 0xFF31, //S17

        ViewSkillState = 0xFF07,
        EventState = 0xFF0B,
        Notice = 0xFF0D,
        Weather = 0xFF0F,
        Beattack = 0xFF10,
        BeattackS16 = 0xFFD3,
        ViewPortCreate = 0xFF12, //S17
        ViewPortMCreate = 0xFF13, //S17
        ViewPortDestroy = 0xFF14, //S17
        AttackResult = 0xFF15, //S17
        KillPlayer = 0xFF16, //S17
        DiePlayer = 0xFF17, //S17
        Rotation = 0xFF53, //S17
        RotationRes = 0xFF83, //S17
        MagicAttack = 0xFF19, //S17
        MagicAttackRes = 0xFF26, //S17
        Teleport = 0xFF1C, //S17
        MagicDuration = 0xFF1E, //S17
        ViewPortMCall = 0xFF1F, //S17
        ViewPortItemCreate = 0xFF20, //S17
        ViewPortItemDestroy = 0xFF21, //S17
        ItemGet = 0xFF32, //S17
        ItemGetRes = 0xFFC1, //S17

        ItemThrow = 0xFFCB, //S17
        ItemThrowRes = 0xFFB2, //S17

        MoveItem = 0xFF41, //S17
        MoveItemRes = 0xFF3A, //S17

        HealthUpdate = 0xFF55, //S17
        UseItem = 0xFFC2, //S17
        ManaUpdate = 0xFF27, //S17
        InventoryItemDelete = 0xFF28, //S17
        ItemUseSpecialTime = 0xFF29, //S17
        InventoryItemDurUpdate = 0xFF2A, //S17
        PeriodicEffect = 0xFF2D, //S17
        Talk = 0xFF4A, //S17
        TalkRes = 0xFF90, //S17
        CloseWindow = 0xFFC0, //S17
        ShopItemList = 0xFF31, //S17
        CancelItemSale = 0x006F,
        CancelItemSaleClose = 0x016F,
        CancelItemSaleItem = 0x026F,

        Buy = 0xFF9F, //S17
        BuyRes = 0xFFB7, //S17

        Sell = 0xFF33, //S17
        SellRes = 0xFF97, //S17

        ItemModify = 0xFF18, //S17
        ItemModifyRes = 0xFF57, //S17
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
        PShopAlterVault = 0xA8FA,
        PShopCloseDeal = 0x073F,
        PShopRequestSold = 0x083F,
        PShopSearchItem = 0x31EC,
        PartyRequest = 0xFF40,
        PartyResult = 0xFF41,
        PartyList = 0xFF42,
        PartyDelUser = 0x0FF43,
        PartyLifeUpdate = 0x0FF44, //S17
        ViewPortChange = 0xFF45, //S17
        SetMapAtt = 0xFF46, //S17
        Effect = 0xFF48, //S17
        ViewPortGuildCreate = 0xFF65, //S17
        WarehouseMoney = 0xFF91, //S17
        WarehouseMoneyRes = 0xFF73, //S17
        WarehouseUseEnd = 0xFFEC, //S17
        ChaosBoxItemMixButtonClick = 0xFF52, //S17
        ChaosBoxItemMixButtonClickRes = 0xFF55, //S17
        ChaosBoxUseEnd = 0xFF87,
        EventEnterCount = 0xFF9F, //S17
        QuestInfo = 0xFF81, //S17

        FriendList = 0xFFC0,
        FriendAdd = 0xFFC1,
        FriendAddWait = 0xFFC2,
        FriendDel = 0xC3, //NO USE
        FriendState = 0xC4, //NO USE
        LetterSend = 0xC5, //NO USE
        AddLetter = 0xFFC6, //NO USE
        LetterRead = 0xC7, //NO USE
        LetterDel = 0xC8, //NO USE
        LetterList = 0xC9, //NO USE

        OpenBox = 0xF2D0, //S17


        Move12Eng = 0xFFD7, //S17

        Attack = 0xFFD7, //S17
        AttackEng = 0xFF11, //S17
        Position = 0xFFDF, //S17
        Position9Eng = 0xFF15, //S17

        MiniMapNPC = 0x03E7, //S17
        PeriodItemCount = 0x11D2, //S17

        PentagramaJInfo = 0x01EE, //S17
        UBFInfo = 0x01CD, //S17

        PopUpType = 0x2682, //S17
        PopUpTypeRes = 0x3252, //S17

        MemberPosInfoStart = 0x01E7,
        MemberPosInfoStop = 0x02E7,
        LifeInfo = 0x2551, //S17
        NPCJulia = 0x17BF, //S17
        MuHelperSwitch = 0x51BF, //S17
        MuHelper = 0xFFAE, //S17
        AttackSpeed = 0x5451, //S17
        KillPlayerEXT = 0xFF9C, //S17
        NPCDialog = 0x01F9, //S17
        QuestExp = 0x30F6, //S17
        ShadowBuff = 0x31F6, //S17
        ChainMagic = 0x0ABF, //S17

        GremoryCaseList = 0x004F,
        GremoryCaseReceive = 0x014F,
        GremoryCaseUse = 0x024F,
        GremoryCaseDelete = 0x034F,
        GremoryCaseNotice = 0x044F,
        GremoryCaseOpen = 0x054F,
        AcheronEnter = 0x20F8,
        RefineJewel = 0x02EC,
        PentagramaJewelIn = 0x00EC,
        PentagramaJewelInOut = 0x04EC,
        ElementDamage = 0xFFD8, //S17
        NeedSpiritMap = 0x21F8,
        PetInfo = 0xFFA9,
        MasterLevelSkills = 0x53F3,
        ExpEventInfo = 0x52BF,

        MuunItemGet = 0x004E,
        MuunInventory = 0x024E,
        MuunItemUse = 0x084E,
        MuunItemSell = 0x094E,
        MuunItemRideSelect = 0x114E,
        MuunItemExchange = 0x134E,
        MuunRideViewPort = 0x144E,
        PetCommand = 0xFFA7,
        PetAttack = 0xFFA8,
        InventoryEquipament = 0x20BF, //S17
        EquipamentChange = 0xFF25,
        Attack12Eng = 0xFFDF, //S17
        SXUpPront = 0x7551, //S17
        SXElementalData = 0x0051, //S17
        SXInfo = 0x71F7, //S17
        SXCharacterInfo = 0x0151, //S17
        NewQuestWorldLoad = 0x20F6,
        NewQuestWorldList = 0x50F6,
        PKLevel = 0x1582, //S17
        MonsterSkill = 0x0069,
        FavoritesList = 0x016D,
        FavoritesListS16Kor = 0x0459,

        PartyMatchingRegister = 0x00EF,
        PartyMatchingSearch = 0x01EF,
        PartyMatchingJoin = 0x02EF,
        PartyMatchingJoinData = 0x03EF,
        PartyMatchingJoinList = 0x04EF,
        PartyMatchingAccept = 0x05EF,
        PartyMatchingCancel = 0x06EF,
        PartyLeaderChange = 0x07EF,
        PartyJoinNotify = 0x08EF,
        HuntingRecordRequest = 0x7151, //S17
        HuntingRecordRequestRes = 0x08F7, //S17
        HuntingRecordClose = 0x50F7, //S17
        HuntingRecordVisibility = 0x27F7, //S17
        HuntingRecordDay = 0x0651, //S17
        HuntingRecordCurrent = 0x7251, //S17
        MossMerchant = 0x1170,
        MossMerchantOpenBox = 0x1070,
        MossMerchantOpenBoxReward = 0x1270,
        GremoryCaseOpenS16 = 0x06CD,

        MiningSystemUnk = 0x204C,
        MajesticInfo = 0x027E,
        MajesticStatsInfo = 0x067E,
        Position16Kor = 0xFF10, //S17

        PShopSearchS16Kor = 0x007C,
        PShopItemSearchS16Kor = 0x017C,
        PShopItemSearch2S16Kor = 0x037C,
        PShopRequestList2S16Kor = 0x067C,
        PShopItemViewS16Kor = 0x077C,
        PShopSetItemPriceS16Kor = 0x087C,
        PShopCancelItemSaleS16Kor = 0x097C,
        PShopChangeState = 0x0A7C,
        ChangeSkin = 0x1252, //S17
        MonsterSoulShop = 0x424D,
        MonsterSoulAvailableShop = 0x464D,
        Ruudbuy = 0xF0D0,
        RuudOpenBox = 0xF1D0,
        RuudSend = 0xF1D0,
    }

    public enum GensOpCode
    {
        RequestJoin = 0x01F8,
        RegMember = 0x02F8,
        RequestLeave = 0x03F8,
        ViewPortGens = 0x05F8,
        SendGensInfo = 0x07F8,
        RequestReward = 0x09F8,
        RemoveMember = 0x7F9,
        RewardSend = 0x0AF8,
        RequestMemberInfo = 0x0BF8,
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
        GuildRelationShip = 0xFFE5,
        GuildRelationShipAns = 0xFFE6,
        GuildUnionList = 0xFFE9,

        GuildMatchingList = 0x00ED,
        GuildMatchingListSearch = 0x01ED,
        GuildMatchingRegister = 0x02ED,
        GuildMatchingRegisterCancel = 0x03ED,
        GuildMatchingJoin = 0x04ED,
        GuildMatchingJoinAccept = 0x06ED,
        GuildMatchingJoinCancel = 0x05ED,
        GuildMatchingJoinList = 0x07ED,
        GuildMatchingJoinInfo = 0x08ED,
        GUildMatchingNotify = 0x09ED,
        GUildMatchingNotifyMaster = 0x10ED,
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

        ChaosCastleMove = 0x01AF, //S17

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
        KanturuEnter = 0x01D1,
        KanturuStateChange = 0x03D1,
        KanturuBattleResult = 0x04D1,
        KanturuBattleTime = 0x05D1,
        KanturuWideAttack = 0x06D1,
        KanturuMonsterUserCount = 0x07D1,

        ImperialGuardianEnter = 0x01F7,
        ImperialGuardianEnterResult = 0x02F7,
        ImperialGuardianNotifyZoneTime = 0x04F7,
        ImperialGuardianNotifyZoneAllClear = 0x06F7,

        ArcaBattleState = 0x38F8,
        Banner = 0x2987, //S17

        CastleSiegeLeftTimeAlarm = 0x1EB2,
        CastleSiegeState = 0x00B2,
        CastleSiegeRegiste = 0x01B2,
        CastleSiegeGuildInfo = 0x03B2,
        CastleSiegeRegisteMark = 0x04B2,
        CastleSiegeSwitchNotify = 0x14B2,
        CastleSiegeCrownState = 0x16B2,
        CastleSiegeNotifyStart = 0x17B2,
        CastleSiegejoinSide = 0x19B2,
        CastleSiegeNotifySwitchInfo = 0x20B2,
        CastleSiegeGuildMarkOfOwner = 0x02B9,
        CastleSiegeGuildList = 0xFFB4,
        CastleSiegeMinimap = 0xFFB6,

        AcheronEnterReq = 0x4BF8,
        AcheronEnter = 0x4CF8,

        EventInventoryOpen = 0x1487, //S17
        EventInventoryOpenRes = 0x3103, //S17
        EventItemGet = 0x004D,
        EventItemThrow = 0x014D,
        EventInventory = 0x3B87, //S17

        // MiniGames
        MuRummyStart = 0x104D,
        MuRummyReveal = 0x114D,
        MuRummyPlayCard = 0x124D,
        MuRummyThrow = 0x134D,
        MuRummyMatch = 0x144D,
        MuRummySpecialMatch = 0x194D,
        MuRummyExit = 0x154D,
        MuRummyCardList = 0x164D,
        MuRummyMessage = 0x174D,

        MineSweeperStart = 0x234D,
        MineSweeperReveal = 0x244D,
        MineSweeperMark = 0x254D,
        MineSweeperEnd = 0x264D,
        MineSweeper = 0x274D,
        MineSweeperGetReward = 0x284D,
        MineSweeperCreateCell = 0x294D,

        BallsAndCowsStart = 0x304D,
        BallsAndCowsPick = 0x314D,
        BallsAndCowsOpen = 0x334D,
        BallsAndCowsResult = 0x344D,

        JeweldryBingoState = 0x2A4D,
        JeweldryBingoInfo = 0x2B4D,
        JeweldryBingoBox = 0x2C4D,
        JeweldryBingoPlayInfo = 0x2D4D,
        JeweldryBingoPlayResult = 0x2E4D,
    }
    public enum CashOpCode : ushort
    {
        CashItems = 0x05D0,
        CashPoints = 0x04F5,

        CashInit = 0x00D2,
        CashPointsS9 = 0x01D2,
        CashOpen = 0x02D2,
        CashItemBuy = 0x03D2,
        CashItemGif = 0x04D2,
        CashInventoryCount = 0x05D2,
        CashItemList = 0x06D2,
        CashVersion = 0x0CD2,
        CashBanner = 0x15D2,
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
        SetQuestState = 0xFF83, //S17
        SetQuestStateRes = 0xFF51, //S17
        QuestPrize = 0xFFA3,
        QuestKillCount = 0xFFA4,
        QuestSurvivalTime = 0x01A5,
        QuestExp = 0x00F6,
        QuestSwitchListNPC = 0x0AF6,
        QuestExpInfo = 0x0BF6,
        QuestExpInfoAsk = 0x0CF6,
        QuestExpComplete = 0x0DF6,
        QuestEXPProgress = 0x1BF6,
        QuestExpProgressList = 0x1AF6,
        QuestEXPEventItemEPList = 0x21F6,
        QuestSwitchListEvent = 0x03F6,
        QuestSwitchListItem = 0x04F6,
        QuestMUTalk = 0x71F6,
        QuestMUAccept = 0x72F6,
        CentQuestTest = 0x203E,
        CentQuestSummon = 0x213E,
        CentQuestMove = 0x223E,
    }
}
