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
        Davias,
        Noria,
        Dugeon
    }

    public enum MapEvents : byte
    {
        GoldenInvasion = 3
    }

    public enum HackCheck : ushort
    {
        PacketProblem = 0x0006,
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
        LeftHand,
        RightHand,
        Helm,
        Armor,
        Pants,
        Gloves,
        Boots,
        Wings,
        Pet,

    }

    public enum HarmonyOption : byte
    {
        None,
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
}
