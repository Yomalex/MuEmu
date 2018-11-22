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
    }


    public enum GlobalOpCode : ushort
    {
        LiveClient = 0x000E,
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
    }

    public enum GameOpCode : ushort
    {
        ClientMessage = 0x03F1,
        KillCount = 0x01B8,
        Inventory = 0x10F3,
        Spells = 0x11F3,
        Equipament = 0x13F3,
        NewQuestInfo = 0x1AF6,
        GameSecurity = 0xFF03,
        EventState = 0xFF0B,
        Notice = 0xFF0D,
        Weather = 0xFF0F,
        ViewPortCreate = 0xFF12,
        ViewPortMCreate = 0xFF13,
        ViewPortMCall = 0xFF1F,
        CloseWindow = 0xFF31,
        ViewPortChange = 0xFF45,
        QuestInfo = 0xFFA0,
        FriendList = 0xFFC0,
        FriendRequest = 0xFFC2,
        AddLetter = 0xFFC6,
    }

    public enum CashOpCode : ushort
    {
        CashItems = 0x05D0,
        CashPoints = 0x04F5,
    }
}
