using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
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
        None,
        Live,
        Die = 4,
    }

    public enum NPCAttributeType
    {
        Warehouse,
        Shop,
        Quest,
        Buff,
        JewelMix,
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
        EventMsg = 1,
        DefaultMsg = 4,
        ServerDivision = 6,
        ShadowPhantom = 0x0D,
        DevilSquarePK = 0x37,
        BloodCastlePK,
        ChaosCastlePK,
    }

    public enum EventMsg
    {
        YouNeedInvitationDS = 2,
        DevilStarted, //???
        DevilDisabled = 6, //???
        RunningBC = 0x14,
        YouNeedInvitationBC = 0x15,
        SucceedBC = 0x17,
        InvalidBC = 0x18,
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

    public enum DamageType
    {
        Miss,
        Regular,
        Excellent,
        Critical,
        Reflect,
        Beat,
        Double = 0x40,
        Combo = 0x80,
    }

    public enum SkillStates
    {
        ShadowPhantom = 0x03,
        None = 0xff,
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
}
