using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public enum StorageID
    {
        Equipament,
        Inventory = 12,
        PersonalShop = 76,
        ChaosBox,
        TradeBox,
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
        T42,
        T43,
        // null
        // null
        IllusionTemple1 = 45,
        IllusionTemple2,
        IllusionTemple3,
        IllusionTemple4,
        IllusionTemple5,
        IllusionTemple6,
        Elbeland = 51,
        BloodCastle8,
        ChaosCastle7,
        // null
        // null
        // null
        Raklion = 57,
        // null
        // null
        // null
        Buhwajang,
        SantaTown = 62,
        Vulcan,
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
        DarkKnight = 0x10,
        BladeKnight,
        BladeMaster,
        FaryElf = 0x20,
        MuseElf,
        HighElf,
        MagicGladiator = 0x30,
        DuelMaster,
        DarkLord = 0x40,
        LordEmperator,
        Summoner = 0x50,
        BlodySummoner,
        DimensionMaster,
        RageFighter = 0x60,
        FistMaster,
        FistBlazer,
        GrowLancer = 0x70,
        MirageLancer,
        ShinningLancer,
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
        End
    }

    public enum SocketOption : byte
    {
        None = 0xff
    }

    public enum GuildStatus : byte
    {
        Member,
        NoMember = 0xff
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

    public enum DamageType : byte
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
        SS21,
        SS22,
        SS23,
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

    public enum GateType
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
}
