using System;
using System.Collections.Generic;
using System.Text;

namespace MU.Resources
{
    public enum GremoryStorage : byte
    {
        Server = 1,
        Character,
    }
    public enum GremorySource : byte
    {
        ChaosCastle = 1,
        BloodCastle = 2,
        IllusionTemple = 3,
        DoppleGanger = 4,
        ChaosCastleSurvival = 5,
        TormentedSquare = 6,
        IllusionTempleLeague = 7,
        ChaosCastleSurvival2 = 8,
        TormentedSquareLeague = 9,
        ChaosCastleUBF = 10,
        GMReward = 100
    }
    public enum GremoryNotice : byte
    {
        ItemAboutToExpire = 1,
        InventoryToBeFilled = 5,
        InventoryFull = 6,
    }
    public enum Element : byte
    {
        None,
        Fire,
        Water,
        Earth,
        Wind,
        Dark,
    }
    public enum GensType : byte
    {
        None,
        Duprian,
        Vanert,
    }
    public enum Messages
    {
        Server_Cfg,
        Server_Title,
        Server_MySQL_Error,
        Server_Error,
        Server_CSServer_Error,
        Server_Disconnecting_Accounts,
        Server_Ready,
        Server_GlobalAnnouncement,
        Server_MapAnnouncement,
        Server_NoEventMapAnnouncement,
        Server_Close,
        RCache_Initialized,
        RCache_Loading_Items,
        RCache_Loading_Spells,
        RCache_Loading_Maps,
        RCache_Loading_DefClass,
        RCache_Loading_Shops,
        RCache_Loading_NPCs,
        RCache_Loading_JoHs,
        RCache_Loading_Gates,
        RCache_Loading_Quests,
        RCache_Loading_ChaosMixs,
        MonsterMng_Loading,
        MonsterMng_Loading2,
        MonsterMng_Types,
        MonsterMng_Loaded,
        BC_Closed,
        BC_Open,
        BC_DoorKiller,
        BC_StatueKiller,
        DS_Closed,
        Chat_Player_Offline,
        Game_Close,
        Game_Close_Message,
        Game_Vault_active,
        Game_NoQuestAvailable,
        Game_DefaultNPCMessage,
        Game_Warp,
        Game_Warp2,
        Game_ChaosBoxMixError,
        Game_MonsterOutOfRange,
        BC_MonstersKilled,
        BC_BossKilled,
        BC_Open2,
        BC_WeaponOwner,
        BC_Winner,
        BC_Time,
        BC_WeaponError,
        Kanturu_NextBattle,
        Kanturu_CoreGateOpens,
        Kanturu_NightmareNotify,
        Kanturu_Successfull,
        Kanturu_Fail,
        Server_EventStateChange,
        IV_IndexAlreadyUsed,
        IVEX_AlreadyEquiped,
        IVEX_NeedMoreAgility,
        IVEX_NeedMoreEnergy,
        IVEX_NeedMoreCommand,
        IVEX_CharNotLogged,
        IVEX_NeedMoreVitality,
        IVEX_UnequipNoEquiped,
        IV_MoveItem,
        IVEX_NeedMoreStrength,
        IV_CantMove,
        IVEX_PentagramTradeLimit,
        IV_DBSaveDeletingItem,
    }
    public enum Spell : ushort
    {
        None,
        Poison,
        Meteorite,
        Lighting,
        FireBall,
        Flame,
        Teleport,
        Ice,
        Twister,
        EvilSpirit,
        Hellfire,
        PowerWave,
        AquaBeam,
        Cometfall,
        Inferno,
        TeleportAlly,
        SoulBarrier,
        EnergyBall,
        Defense,
        Falling_Slash,
        Lunge,
        Uppercut,
        Cyclone,
        Slash,
        Triple_Shot,
        Heal = 26,
        GreaterDefense,
        GreaterDamage,
        Summon_Goblin = 30,
        Summon_StoneGolem,
        Summon_Assassin,
        Summon_EliteYeti,
        Summon_DarkKnight,
        Summon_Bali,
        Summon_Soldier,
        Decay = 38,
        IceStorm,
        Nova,
        TwistingSlash,
        RagefulBlow,
        DeathStab,
        CrescentMoonSlash,
        ManaGlaive,
        Starfall,
        Impale,
        GreaterFortitude,
        FireBreath,
        FlameofEvilMonster,
        IceArrow,
        Penetration,
        FireSlash = 55,
        PowerSlash,
        SpiralSlash,
        Force = 60,
        FireBurst,
        Earthshake,
        Summon,
        IncreaseCriticalDmg,
        ElectricSpike,
        ForceWave,
        Stern,
        CancelStern,
        SwellMana,
        Transparency,
        CancelTransparency,
        CancelMagic,
        ManaRays,
        FireBlast,
        PlasmaStorm = 76,
        InfinityArrow,
        FireScream,
        DrainLife = 214,
        ChainLighting,
        ElectricSurge,
        Reflex,
        Sleep = 219,
        Night,
        MagicSpeedUp,
        MagicDefenseUp,
        Sahamutt,
        Neil,
        GhostPhantom,

        RedStorm = 230,
        MagicCircle = 233,
        Recovery = 234,
        MultiShot = 235,
        LightingStorm = 237,

        //RF Skills
        KillingBlow = 260,
        BeastUppercut,
        ChainDrive,
        DarkSide,
        DragonRoar,
        DragonSlasher,
        IgnoreDefense,
        IncreaseHealth,
        IncreaseBlock,
        Charge,
        PhoenixShot,

        // Master Level
        SoulBarrier1 = 435,
        SoulBarrier2,
        SoulBarrier3,
        SoulBarrier4,
        SoulBarrier5,
        Hellfire1 = 440,
        Hellfire2,
        Hellfire3,
        Hellfire4,
        Hellfire5,
        EvilSpirit1 = 445,
        EvilSpirit2,
        EvilSpirit3,
        EvilSpirit4,
        EvilSpirit5,
        IceStorm1 = 450,
        IceStorm2,
        IceStorm3,
        IceStorm4,
        IceStorm5,
        TwistingSlash1 = 455,
        TwistingSlash2,
        TwistingSlash3,
        TwistingSlash4,
        TwistingSlash5,
        DeathStab1 = 460,
        DeathStab2,
        DeathStab3,
        DeathStab4,
        DeathStab5,
        RagefulBlow1 = 465,
        RagefulBlow2,
        RagefulBlow3,
        RagefulBlow4,
        RagefulBlow5,
        GreatFortitude1 = 470,
        GreatFortitude2,
        GreatFortitude3,
        GreatFortitude4,
        GreatFortitude5,
        Heal1 = 475,
        Heal2,
        Heal3,
        Heal4,
        Heal5,
        GreaterDefense1 = 480,
        GreaterDefense2,
        GreaterDefense3,
        GreaterDefense4,
        GreaterDefense5,
        GreaterDamage1 = 485,
        GreaterDamage2,
        GreaterDamage3,
        GreaterDamage4,
        GreaterDamage5,
    }
    public enum MiniMapTag : byte
    {
        Shield = 1,
        Conversation,
        Hammer,
        Elixir,
        Storage,
    }

    [Flags]
    public enum MapAttributes : byte
    {
        Safe = 1,
        Stand = 2,
        NoWalk = 4,
        Hide = 8,
        Unknow = 16,
    }
    public enum RewardType : uint
    {
        None = 0x00,
        Exp = 0x01,
        Zen = 0x02,
        Item = 0x04,
        Point = 0x10
    }
    public enum AskType : uint
    {
        None,
        Monster,
        Skill,
        Item = 4,
        LevelUp = 8,
        Tutorial = 16,
        Buff = 32,
        ChaosCastleUserKill = 0x40,
        ChaosCastleMonsterKill,
        BloodCastleDoorKill,
        BloodCastleClear = 0x100,
        ChaosCastleClear,
        DevilSquareClear,
        IllusionTempleClear,
    }
    [Flags]
    public enum EnableClassCreation : byte
    {
        Summoner = 1,
        DarkLord = 2,
        MagicGladiator = 4,
        RageFighter = 8,
        GrowLancer = 16,
    }
    public enum BannerType : int
    {
        MuRummy,
        EvenInven,
        Evomon,
        UnityBattleField,
        UnityBattleField2,
        Unk,
    }

    public enum TradeResult : byte
    {
        Error = 0,
        Ok = 1,
        InventoryFull = 2,
        CantTradeHarmonized = 4,
    }
    public enum StorageID : int
    {
        Equipament,
        Inventory = 12,// Size 64
        ExpandedInventory1 = 76, // Size 32
        ExpandedInventory2 = 108, // Size 32
        PersonalShop = 140, // Size 32
        ChaosBox,
        TradeBox,
        Pentagram,
        Warehouse
    }

    public enum LoginStatus
    {
        NotLogged,
        Logged,
        Playing
    }

    public enum LoginResult : byte
    {
        Ok = 1,
        Fail,
        IsConnected,
        ServerFull,
        IsBanned,
        OldVersion,
        ConnectionError,
        ConnectionClosed3Fail,
        OnlyVIP,
        VipEnding,
        VipEnded,
        VipEnded2,

        OkWithItemBlock = 15,
    }

    public enum ControlCode : byte
    {
        Normal,
        CharacterBlock,
        ItemBlock,
        GameMaster = 0x20,
    }

    public enum Maps : byte
    {
        Lorencia,
        Dugeon,
        Davias,
        Noria,
        LostTower,
        Exile,
        Stadium,
        Atlans,
        Tarkan,
        DevilSquare,
        Icarus,
        BloodCastle1,
        BloodCastle2,
        BloodCastle3,
        BloodCastle4,
        BloodCastle5,
        BloodCastle6,
        BloodCastle7,
        ChaosCastle1,
        ChaosCastle2,
        ChaosCastle3,
        ChaosCastle4,
        ChaosCastle5,
        ChaosCastle6,
        Kalima1,
        Kalima2,
        Kalima3,
        Kalima4,
        Kalima5,
        Kalima6,
        ValleyofLoren,
        LandofTrial,
        DevilSquare2,
        Aida,
        Crywolf,
        // null
        Kalima7 = 36,
        Kantru1,
        Kantru2,
        Kantru3,
        SilentSwamp,
        BarracksofBalgass,
        BalgassRefuge,
        T43,
        T44,
        IllusionTemple1,
        IllusionTemple2,
        IllusionTemple3,
        IllusionTemple4,
        IllusionTemple5,
        IllusionTemple6,
        Elbeland,
        BloodCastle8,
        ChaosCastle7,
        // null
        // null
        SwampOfCalmness = 56,
        Raklion,
        Selupan,
        // null
        // null
        Buhwajang = 61,
        SantaTown,
        Vulcan,
        DuelArena,
        DoppelgangerSnow,
        DoppelgangerVulcan,
        DoppelgangerSea,
        DoppelgangerCrystals,
        ImperialGuardian1,
        ImperialGuardian2,
        ImperialGuardian3,
        ImperialGuardian4,
        LorenMarket = 79,
        Karutan1,
        Karutan2,
        DoppelgangerRenewal,
        Acheron = 91,
        ArkaWar,
        Debenter = 95,
        ArcaBattle,
        ChaosCastleSurvival,
        IllussionTempleLeague1,
        IllussionTempleLeague2,
        UrkMontain1,
        UrkMontain2,
        TormentedSquareoftheFittest,
        TormentedSquare1,
        TormentedSquare2,
        TormentedSquare3,
        TormentedSquare4,
        Nars = 110,
        Ferea = 112,
        NixieLake,
        LabyrinthEntrance,
        Labyrinth,
        DeepDungeon1,
        DeepDungeon2,
        DeepDungeon3,
        DeepDungeon4,
        DeepDungeon5,
        NewQuest,
        SwampOfDarkness,
        KuberaMineArea1,
        KuberaMineArea2,
        KuberaMineArea3,
        KuberaMineArea4,
        KuberaMineArea5,
        AbyssOfAtlas1,
        AbyssOfAtlas2,
        AbyssOfAtlas3,
        ScorchedCanyon,
        InvalidMap = 255,
    }

    public enum MapEvents : byte
    {
        GoldenInvasion = 3
    }

    public enum HackCheck : ushort
    {
        InvalidPacket = 0x0006,
        StrangePacket = 0xF300,
    }

    public enum HeroClass : byte
    {
        DarkWizard,
        SoulMaster,
        GranMaster,
        SoulWizard, //4TH Class
        DarkKnight = 0x10,
        BladeKnight,
        BladeMaster,
        DragonKnight, //4TH Class
        FaryElf = 0x20,
        MuseElf,
        HighElf,
        NobleElf, //4TH
        MagicGladiator = 0x30,
        DuelMaster,
        MagicKnight, //4TH
        DarkLord = 0x40,
        LordEmperator,
        EmpireLord,
        Summoner = 0x50,
        BlodySummoner,
        DimensionMaster,
        DimensionSummoner, //4TH
        RageFighter = 0x60,
        FistMaster,
        FistBlazer,//4TH
        GrowLancer = 0x70,
        MirageLancer,
        ShinningLancer, //4TH
        RuneWizard = 0x80,
        RuneSpellMaster,
        GrandRuneMaster, //4TH
        End = 0xff,
    }

    public enum Equipament
    {
        RightHand,
        LeftHand,
        Helm,
        Armor,
        Pants,
        Gloves,
        Boots,
        Wings,
        Pet,
        Pendant,
        RightRing,
        LeftRing,
        End,
        Pentagrama = 236,
    }


    [Flags]
    public enum ExcellentOptionArmor
    {
        None = 0,
        IncreaseZen = 1,//Increase Zen +40%
        DefenseSuccessRate = 2,//Defense Success rate +10%
        ReflectDamage = 4,//Reflect Damage +5%
        DamageDecrease = 8, //Damage Decrease +4%
        IncreaseMana = 16,//Increase Mana +4%
        IncreaseHP = 32,//Increase HP +4%
        FullItem = IncreaseZen | DefenseSuccessRate | ReflectDamage | DamageDecrease | IncreaseMana | IncreaseHP,
    }


    [Flags]
    public enum ExcellentOptionWeapons
    {
        None = 0,
        IncreaseManaRate = 1,//Increase Adquisition rate of Mana after hunting monsters Mana/8
        IncreaseLifeRate = 2,//Increase Adquisition rate of Mana after hunting monsters life/8
        IncreaseAttacking = 4,//Increase Attacking (Wizardry) speed + 7
        IncreaseWizardryRate = 8, //Increase Wizardry Dmg 2%
        IncreaseWizardry = 16,//Increase Wizardry Level/20
        ExcellentDmgRate = 32,//Excellent Dmg Rate 10%
        FullItem = IncreaseManaRate | IncreaseLifeRate | IncreaseAttacking | IncreaseWizardryRate | IncreaseWizardry | ExcellentDmgRate,
    }

    // Level*50+Type
    public enum SocketOption : byte
    {
        SocketFire = 0, //Attack/Wizardy Increase +57
        SocketWater = 1, //Block rating increase +14%
        SocketIce = 2, //Monster destruction for the Life increase +16250
        SocketWind = 3, //Automatic Life recovery increase +20
        SocketLightning = 4, //Exelen damage increase +40
        SocketGround = 5,

        SocketFire1 = 50, //Attack Speed Increase +11
        SocketWater1 = 51, //Defense Increase +42
        SocketIce1 = 52, //Monster destruction for the Mana increase +16250
        SocketWind1 = 53, //Maximum Life increase +8%
        SocketLightning1 = 54, //Exelen damage rate increase +14%
        SocketGround1 = 55,

        SocketFire2 = 100, //Maximum attack/Wizardy Increase +50
        SocketWater2 = 101, //Shield protection increase +30%
        SocketIce2 = 102, //Skill attack increase +50
        SocketWind2 = 103, //Maximim Mana increase +8%
        SocketLightning2 = 104, //Critical damage increase +50
        SocketGround2 = 105, //Hearth increase +38

        SocketFire3 = 150, //Minimum attack/Wizardy Increase +35
        SocketWater3 = 151, //Damage reduction +8%
        SocketIce3 = 152, //Attack rating increase +40
        SocketWind3 = 153, //Automatic Mana recovery increase +35
        SocketLightning3 = 154, //Critical damage rate increase +12%
        SocketGround3 = 155,

        SocketFire4 = 200, //Attack/Wizardy Increase +35
        SocketWater4 = 201, //Damage reflection +9%
        SocketIce4 = 202, //Item durability increase +38%
        SocketWind4 = 203, //Maximum AG increase +50
        SocketLightning4 = 204,
        SocketGround4 = 205,

        EmptySocket = 0xfe,
        None = 0xff
    }

    public enum GuildStatus : byte
    {
        Member,
        BattleMaster = 0x20,
        Assistant = 0x40,
        GuildMaster = 0x80,
        NoMember = 0xff
    }

    public enum GuildResult : byte
    {
        Fail = 0x00,
        Success = 0x01,
        CannotAcceptMoreMembers = 0x02,
        PlayerOffline = 0x03,
        NotGuildMaster = 0x04,
        HaveGuild = 0x05,
        InTransaction = 0x06,
        InsuficientLevel = 0x07,
        NotExist = 0x10,
        //UnionFail,
        NotExistPermission,
        NotExistExtraStatus,
        NotExistExtraType,
        ExistRelationshipUnion = 0x15,
        ExistRealtionshipRival,
        ExistUnion,
        ExistRival,
        NotExistUnion,
        NotExistRival,
        NotUnionMaster,
        NotRival,
        CannotBeUnionMaster,
        ExceedMaxUnionMembers,
        CancelRequest = 0x20,
        AllyMasterNoGems = 0xA1,
        DifferentGens = 0xA3,
    }

    public enum GuildRelation : byte
    {
        None,
        Union,
        Rival,
        UnionMaster = 4,
        RivalMaster = 8,
    }

    public enum LetterStatus : byte
    {
        Readed,
        UnReaded,
        New,
    }

    public enum RefillInfo : byte
    {
        MaxChanged = 0xFE,
        Update = 0xFD,
        Drink = 0xFF,
    }

    public enum PointAdd : byte
    {
        Strength,
        Agility,
        Vitality,
        Energy,
        Command,
    }

    public enum ClientCloseType : byte
    {
        CloseGame,
        SelectChar,
        ServerList,
    }

    public enum MoveItemFlags : byte
    {
        Inventory,
        Trade,
        Warehouse,
        ChaosBox,
        PersonalShop,
        DarkTrainer = 5,
        ElpisBox,
        PentagramBox = 17,
    }

    public enum NoticeType : byte
    {
        Gold,
        Blue,
        Guild
    }

    public enum EventEnterType : byte
    {
        DevilSquare = 1,
        BloodCastle,
        ChaosCastle = 4,
        IllusionTemple,
    }

    public enum ObjectType
    {
        NPC,
        Monster,
        Gate,
    }

    public enum ObjectState
    {
        Regen,
        Live,
        Dying,
        Dying2,
        Die,
        WaitRegen,
    }

    public enum NPCAttributeType
    {
        Warehouse,
        Shop,
        Quest,
        Buff,
        Window,
        EventChips,
        MessengerAngel,
        KingAngel,
        GuildMaster,
        Kanturu,
        DevilSquare,
        ServerDivision,
        Talk,
        Gens,
    }

    public enum NPCWindow : byte
    {
        Shop = 0,
        Warehouse = 2,
        ChaosMachine = 3,
        DevilSquared = 4,
        MessengerAngel = 6,
        JewelMixer = 9,
        ChaosCard = 21,
        BlossomGovernor = 22,
        SokcetMaster = 23,
        SokcetResearcher = 24,
        LuckyCoins = 32,
        GateKeeper = 33,
        Sartigan = 35,
        LeoTheHelper = 38,
    }

    public enum AttributeType
    {
        Ice,
        Poison,
        Light,
        Fire,
        Earth,
        Wind,
        Water,
    }

    public enum TaxType : byte
    {
        Warehouse = 1,
        Shop,
    }

    public enum ServerCommandType : byte
    {
        Fireworks,
        EventMsg,
        DefaultMsg = 4,
        ServerDivision = 6,
        ShadowPhantom = 0x0D,
        DevilSquarePK = 0x37,
        BloodCastlePK,
        ChaosCastlePK,
    }

    public enum EventMsg : byte
    {
        YouNeedInvitationDS = 2,
        DevilStarted, //???
        DevilDisabled = 6, //???
        RunningBC = 0x14,
        YouNeedInvitationBC = 0x15,
        SucceedBC = 0x17,
        InvalidBC = 0x18,
        GoAheadBC = 0x2D,
        CompletedBC = 0x2E,
    }

    public enum DefaultMsg
    {
        Guard,
        DeviasMadam,
        Lala,
    }

    public enum TalkResult : byte
    {
        Shop,
        WareHouse = 2,
        DevilSquare = 4,
    }

    public enum DamageType : ushort
    {
        Regular,
        Excellent = 2,
        Critical,
        Beat,
        Poison,
        Reflect,
        White,
        Miss = 9,
        Double = 0x40,
        Combo = 0x80,
    }

    public enum SkillStates : byte
    {
        Attack = 1,
        Defense,
        ShadowPhantom,
        SoulBarrier,
        CriticalDamage,
        InfinityArrow,
        RecoverySpeed,
        SwellLife,
        SwellMana,
        PotionBless,
        PotionSoul,
        ss12,
        SS13,
        SS14,
        SS15,
        SS16,
        SS17,
        Transparency,
        SS19,
        SS20,
        AltarCanContract,
        AltarCantContract,
        AltarValidContract,
        AltarOfWolfCA,
        Admin,
        AltarOfWolfHCS,
        Invisible,
        GameMaster,
        SealAscencion,
        SealWealth,
        SealSustance,
        SS32,
        SS33,
        IllusionProtection,
        HAttackSpeed,
        HAttackPower,
        HDefensePower,
        HMaxLife,
        HMaxMana,
        SealAscencion2,
        SealWealth2,
        SealSustance2,
        SealMovement,
        ScrollQuick,
        ScrollDefense,
        ScrollDamage,
        ScrollPower,
        ScrollStamina,
        ScrollMana,
        ElixirPower,
        ElixirDefense,
        ElixirStamina,
        ElixirEnergy,
        ElixirCommand,
        Poison,
        Ice,
        IceArrow,
        DefenseDown,
        SS59,
        SS60,
        Stun,
        SS62,
        SS63,
        SS64,
        IllusionOorderOfBondage,
        SS66,
        SS67,
        SS68,
        SS69,
        SS70,
        SkillDamageDeflection,
        SkillSleep,
        SkillBlindness,
        SkillNeil,
        SkillSahamut,
        SkillNiceweek,
        SkillINNERBEIYSEYON,
        CherryManabuff,
        CherryLifebuff,
        CherryDamagebuff,
        SkillSwordPower,
        SkillMagicCircle,
        SkillSwordSlashEffect,
        SkillLightningStorm,
        SkillRedStorm,
        SkillCold,
        SealRed,
        SealYellow,
        USword,
        UTriangle,
        ImproveDefenseOfense,
        improveMaxLife,
        ImproveMana,
        ImproveDamage,
        ImrpoveDefense,
        improveAttackSpeed,
        ImproveBPRecovery,
        DuelInterface,
        SS99,
        SS100,
        SS101,
        SS102,
        DuelMedal,
    }

    public enum GateType : byte
    {
        Warp,
        Entrance,
        Exit,
    }

    public enum QuestState : byte
    {
        Clear,
        Reg,
        Complete,
        Unreg,
    }

    public enum QuestCompensation : byte
    {
        Statup = 200,
        Changeup,
        Plusstat,
        Comboskill,
        Master,
    }

    public enum DevilSquareState : byte
    {
        Close = 0,
        Open = 1,
        Playing = 2,
        None = 3,
        BeforeStart = 4,
        BeforeEnd = 5,
        Quit = 6,
        CCBeforeEnter = 10,
        CCBeforePlay = 11,
        CCBeforeEnd = 12
    }

    public enum ChaosBoxMixResult : byte
    {
        Fail,
        Success,
        InsufficientMoney,
        TooManyItems,
        LowLevelUser,
        LackingItems = 6,
        IncorrectItems,
        InvalidItemLevel,
        UserClassLow,
        NoBcCorrectItems,
        BcInsufficientMoney,

        PentagramaUpgradeFail = 0xE1,
        PentagramaRefineFail,
        PentagramaFailWithTalisman,
        PentagramaSuccessNotFound = 0xF8,
        PentagramaInsufficientMoney,
        PentagramaLackingItems,
        PentagramaAttributeMissMatch,
        PentagramaRefineNotFound,
    }

    public enum ChaosMixType
    {
        Default = 0x1,
        DevilSquared = 0x2,
        Plus10 = 0x3,
        Plus11 = 0x4,
        Dinorant = 0x5,
        Fruit = 0x6,
        WingLv2 = 0x7,
        BloodCastle = 0x8,
        WingLv1 = 0xb,
        SetItem = 0xc,
        DrakHorse = 0xd,
        DarkSpirit = 0xe,
        BlessPotion = 0xf,
        SoulPotion = 0x10,
        LifeStone = 0x11,
        HTBox = 0x14,
        Plus12 = 0x16,
        Plus13 = 0x17,
        Cloak = 0x18,
        Fenrir1 = 0x19,
        Fenrir2 = 0x1a,
        Fenrir3 = 0x1b,
        Fenrir4 = 0x1c,
        CounpundPotionL1 = 0x1e,
        CounpundPotionL2 = 0x1f,
        CounpundPotionL3 = 0x20,
        JOHPurity = 0x21,
        JOHSmeltingItem = 0x22,
        JOHRestore = 0x23,
        Item380 = 0x24,
        LotteryMix = 0x25,
        OldPaper = 0x25,
        CondorFeather = 0x26,
        WingLv3 = 0x27,
        SeedExtract = 0x2a,
        SeedSphere = 0x2b,
        SeedCalc = 0x2c,
        Mix2 = 0x2d,
        Secromicom = 0x2e,
        Plus14 = 49,
        Plus15 = 50,
    };

    public enum ClientEffect : byte
    {
        RecoverShield = 3,
        LevelUp = 16,
        ShieldDamage = 17,
    }

    public enum PartyResults : byte
    {
        Fail,
        Success,
        PlayerOffline = 0x03,
        InAnotherParty = 0x04,
        RestrictedLevel = 0x05
    }

    public enum KanturuState : byte
    {
        None,
        BattleStandBy,
        BattleOfMaya,
        BattleOfNightmare,
        TowerOfRefinery,
    }

    public enum DuelResults : byte
    {
        NoError,
        Failed,
        InvalidUser,
        NonPKServer,
        NonDuelCSServer,
        ConnectionClosing,
        NotDuelMurderer,
        AlreadyDuelled,
        InvalidMap,
        LimitPacketTime,
        InvitedMyself,
        InvalidIndex,
        Disconnected,
        SelfDefense,
        GuildWar,
        RefuseInvitated,
        DuelMax,
        InvalidStatus,
        AlreadyDuelRequested,
        AlreadyDuelReserved,
        AlreadyDuelling,
        AlreadyDuelRequested1,
        AlreadyDuelReserved1,
        AlreadyDuelling1,
        InvalidChannelId,
        FailedEnter,
        NotExistUser,
        ObserverMax,
        LimitLevel,
        NotFoundMoveReqData,
        NotEnoughMoney
    }

    public enum PShopResult : byte
    {
        Disabled,
        Success,
        InvalidPosition,
        InvalidItem,
        InvalidPrice,
        LevelTooLow,
        ItemBlocked,
        LackOfZen,
        ExceedingZen,
    }
}

namespace MU.Resources
{
    public enum PIGrade
    {
        None,
        Common,
        Unique,
        Rare,
    }
}