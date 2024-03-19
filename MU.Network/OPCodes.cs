using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MU.Network
{
    internal class Data
    {
        internal byte oldOPCode;
        internal byte newC2S;
        internal byte newS2C;
        internal IEnumerable<byte> oldSub;
        internal IEnumerable<byte> c2sSub;
        internal IEnumerable<byte> s2cSub;

        internal Dictionary<byte, byte> toClient = new Dictionary<byte, byte>();
        internal Dictionary<byte, byte> toServer = new Dictionary<byte, byte>();

        internal static List<Data> datas = new List<Data>
        {
            new Data(0xF1,0xF3,0x3A,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x12,0xFD,0xFE},
            new byte[]{0x00,0x02,0x01,0x03,0x04,0x05,0x06,0x12,0xFD,0xFE},
            new byte[]{0x00,0xFD,0xFE,0x03,0x04,0x05,0x06,0x12,0x02,0x01}
            ),
            new Data(0xF3,0x52,0x82,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x30,0x31,0x34,0x35,0x40,0x50,0x51,0x52,0x53,0x60},
            new byte[]{0x60,0x26,0x06,0x00,0x04,0x05,0x15,0x07,0x08,0x10,0x11,0x16,0x13,0x14,0x01,0x21,0x20,0x12,0x22,0x23,0x24,0x25,0x26,0x31,0x02,0x34,0x35,0x40,0x50,0x51,0x03,0x03,0x30},
            new byte[]{0x04,0x00,0x07,0x14,0x20,0x60,0x25,0x06,0x15,0x26,0x53,0x12,0x52,0x51,0x05,0x30,0x23,0x21,0x50,0x34,0x08,0x13,0x32,0x10,0x31,0x03,0x22,0x35,0x11,0x01,0x16,0x24,0x40}
            ),
            new Data(0x00,0x4A,0x00),
            new Data(0x03,0x31,0x00),
            new Data(0x0E,0x24,0x0E),
            new Data(0x18,0x83,0x53),
            new Data(0x19,0x26,0x19),
            new Data(0x22,0xC1,0x32),
            new Data(0x23,0xB2,0xCB),
            new Data(0x24,0x3A,0x41),
            new Data(0x26,0xC2,0x55),
            new Data(0x30,0x90,0x4A),
            new Data(0x31,0xC0,0x31),
            new Data(0x32,0xB7,0x9F),
            new Data(0x33,0x97,0x33),
            new Data(0x34,0x57,0x18),
            new Data(0x36,0x43,0xF1),
            new Data(0x37,0x61,0x22),
            new Data(0x3A,0x19,0x54),
            new Data(0x3C,0xCA,0xC6),
            new Data(0x3D,0xE5,0x3D),
            new Data(0x3E,0x33,0x71,
            new byte[]{0x01,0x03,0x07,0x08,0x09,0x10,0x11,0x20,0x21,0x22},
            Array.Empty<byte>(),
            new byte[]{0x01,0x30,0x39,0x32,0x09,0x03,0x11,0x20,0x21,0x22}
            ),
            new Data(0x3F,0x9D,0x36,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x10},
            Array.Empty<byte>(),
            new byte[]{0x06,0x01,0x02,0x12,0x04,0x05,0x06,0x07,0x00}
            ),
            new Data(0x40,0xE2,0x40),
            new Data(0x41,0x41,0xEC),
            new Data(0x43,0x34,0x43),
            new Data(0x4A,0x42,0x86),
            new Data(0x4B,0x4B,0xA0),
            new Data(0x4C,0xC4,0xB1,
            new byte[]{0x00,0x01,0x02,0x03,0x10,0x11},
            Array.Empty<byte>(),
            new byte[]{0x10,0x01,0x02,0x03,0x00,0x11}
            ),
            new Data(0x4D,0x03,0x87,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x0F,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2A,0x2B,0x2C,0x2D,0x2E,0x30,0x31,0x32,0x33,0x34,0x40,0x41,0x42,0x43,0x44,0x45,0x46,0x47},
            new byte[]{0x2B,0x0F,0x02,0x03,0x04,0x05,0x06,0x31,0x19,0x3B,0x33,0x27,0x00,0x2C,0x16,0x17,0x18,0x30,0x28,0x2D,0x38,0x26,0x3A,0x28,0x29,0x28,0x28,0x28,0x28,0x12,0x01,0x15,0x32,0x33,0x34,0x40,0x41,0x42,0x43,0x44,0x45,0x46,0x47},
            new byte[]{ 0x36, 0x11, 0x3B, 0x42, 0x45, 0x15, 0x03, 0x14, 0x3C, 0x33, 0x27, 0x0F, 0x2B, 0x16, 0x12, 0x2E, 0x29, 0x19, 0x06, 0x25, 0x2C, 0x01, 0x46, 0x44, 0x31, 0x2D, 0x00, 0x28, 0x24, 0x40, 0x05, 0x30, 0x48, 0x10, 0x26, 0x43, 0x02, 0x23, 0x38, 0x18, 0x17, 0x34, 0x39 }
            ),
            new Data(0x50,0xC3,0x96),
            new Data(0x51,0x0E,0xC3),
            new Data(0x52,0x4C,0xB7),
            new Data(0x53,0x50,0x9A),
            new Data(0x54,0xA7,0x54), // FIX THIS
            new Data(0x55,0x40,0x55), // FIX THIS
            new Data(0x61,0xC8,0x61),
            new Data(0x66,0x72,0x66),
            new Data(0x71,0x22,0x4C), // FIX THIS
            new Data(0x73,0x73,0xC0), // FIX THIS
            new Data(0x81,0x73,0x91),
            new Data(0x82,0xEC,0x95),
            new Data(0x83,0x86,0xB3),
            new Data(0x86,0x55,0x52),
            new Data(0x87,0x01,0x95),
            new Data(0x90,0x90,0x3E),
            new Data(0x91,0x66,0x30),
            new Data(0x95,0x95,0xA2),
            new Data(0x96,0x96,0xBD),
            new Data(0x97,0x81,0x97),
            new Data(0x9A,0x18,0xF3),
            new Data(0xA0,0xB1,0x81),
            new Data(0xA2,0x51,0x83),
            new Data(0xA7,0x9F,0x3F),
            new Data(0xA9,0xCB,0xE5),
            new Data(0xB1,0x4B,0x73,
            new byte[]{0x00,0x01},
            new byte[]{0x00,0x01},
            new byte[]{0x00,0x01}
            ),
            new Data(0xB2,0x30,0xCA,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,0x20},
            new byte[]{},
            new byte[]{0x01,0x07,0x02,0x06,0x1B,0x13,0x17,0x1D,0x03,0x05,0x20,0x04,0x18,0x1A,0x11,0x15,0x1F,0x1C,0x10,0x08,0x1E,0x00,0x1C,0x16,0x19,0x12,0x09}
            ),
            new Data(0xB3,0xA0,0x4B),
            new Data(0xB7,0x3E,0xC2,
            new byte[]{0x00,0x01,0x02,0x04},
            new byte[]{0x00,0x01,0x02,0x04},
            new byte[]{0x03,0x00,0x01,0x04}
            ),
            new Data(0xBD,0x3C,0xC8,
            new byte[]{0x00,0x02,0x03,0x04,0x05,0x07,0x08,0x09,0x0C},
            new byte[]{0x00,0x02,0x09,0x04,0x05,0x07,0x08,0x03,0x0C},
            new byte[]{0x04,0x02,0x09,0x05,0x0C,0x00,0x08,0x07,0x03}
            ),
            new Data(0xC0,0x3D,0xC7),
            new Data(0xC1,0x96,0x4D),
            new Data(0xC2,0x32,0x24),
            new Data(0xC3,0x53,0x34),
            new Data(0xC4,0xC9,0x5A),
            new Data(0xC5,0x23,0xF7),
            new Data(0xC6,0xC6,0xC4),
            new Data(0xC7,0xC5,0x90),
            new Data(0xC8,0x71,0x23),
            new Data(0xC9,0xA9,0xC9),
            new Data(0xCA,0xB3,0x3C),
            new Data(0xCB,0xC7,0xE2),
            new Data(0xE5,0x91,0xA7),
            new Data(0xEC,0xF7,0x51,
            new byte[]{0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x10,0x25,0x26,0x27,0x28,0x29,0x30,0x31,0x33,0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x57,0x58,0x60,0x61,0x62},
            new byte[]{0x57,0x70,0x54,0x07,0x04,0x75,0x33,0x31,0x01,0x10,0x25,0x26,0x71,0x28,0x29,0x30,0x31,0x33,0x08,0x50,0x27,0x53,0x51,0x56,0x56,0x58,0x59,0x60,0x73,0x62},
            new byte[]{ 0x60, 0x04, 0x07, 0x08, 0x30, 0x63, 0x73, 0x50, 0x53, 0x25, 0x75, 0x00, 0x27, 0x62, 0x01, 0x54, 0x31, 0x33, 0x71, 0x51, 0x52, 0x06, 0x55, 0x72, 0x28, 0x31, 0x10, 0x05, 0x61, 0x74 }
            ),
        };

        public Data(
            byte old, 
            byte C2S, 
            byte S2C, 
            IEnumerable<byte> _oldSub, 
            IEnumerable<byte> _c2sSub, 
            IEnumerable<byte> _s2cSub)
        {
            oldOPCode = old;
            newC2S = C2S;
            newS2C = S2C;

            oldSub = _oldSub;
            c2sSub = _c2sSub;
            s2cSub = _s2cSub;

            if (_c2sSub.Count() == _oldSub.Count())
                for (var i = 0; i < _oldSub.Count(); i++)
                {
                    toClient.Add(_oldSub.ElementAt(i), _c2sSub.ElementAt(i));
                }

            if (_s2cSub.Count() == _oldSub.Count())
                for (var i = 0; i < _oldSub.Count(); i++)
                {
                    toServer.Add(_oldSub.ElementAt(i), _s2cSub.ElementAt(i));
                }
        }

        public Data(
            byte old,
            byte C2S,
            byte S2C)
        {
            oldOPCode = old;
            newC2S = C2S;
            newS2C = S2C;

            oldSub = Array.Empty<byte>();
            c2sSub = Array.Empty<byte>();
            s2cSub = Array.Empty<byte>();
        }

        public ushort ToClient(ushort opcode)
        {
            var subHead = (byte)((opcode & 0xFF00) >> 8);
            if (toClient.Any())
            {
                if (toClient.TryGetValue(subHead, out var client))
                    return (ushort)(newC2S | (client << 8));
            }
            return (ushort)(newC2S | (subHead << 8));
        }

        public ushort ToServer(ushort opcode)
        {
            var subHead = (byte)((opcode & 0xFF00) >> 8);
            if (toServer.Any())
            {
                if (toServer.TryGetValue(subHead, out var client))
                    return (ushort)(newS2C | (client << 8));
            }
            return (ushort)(newS2C | (subHead << 8));
        }


        public static T ProtocolXChangeS17K75<T>(T opcodeOld, bool fromClient) where T : Enum
        {
            var tmp = (ushort)(object)opcodeOld;
            var head = (byte)(tmp & 0xff);

            var data = datas.FirstOrDefault(x => x.oldOPCode == head);
            if (data == null) return opcodeOld;

            return (T)(object)(fromClient ? data.ToClient(tmp) : data.ToServer(tmp));
        }
    }
    public enum ConOpCode : ushort
    {
        CSWelcome = 0x0100,
        GSJoin = 0xFF10,
        GSKeep = 0xFF11,
        GSClientAdd = 0xFF12,
        GSClientChat = 0xFF13,
        GSClientRem = 0xFF14,
        ServerList = 0x06F4,
    }


    public enum GlobalOpCode : ushort
    {
        LiveClient = 0xFF0E,
        LiveClientS17K75 = 0xFF24,
    }

    public enum AHOpCode : ushort
    {
        AHCheck = 0x11FA,
        AHEncKey = 0x00FA,
    }

    public enum CSOpCode : ushort
    {
        JoinResult = 0x00F1,
        Login = 0x01F1,
        LoginS17Kor = 0x020E,
        LoginS17KorResp = 0xFEF1,
        CharacterList = 0x00F3,
        CharacterListS17Kor = 0x154A,
        CharacterListS17KorResp = 0x1452,
        CharacterCreate = 0x01F3,
        CharacterDelete = 0x02F3,
        JoinMap2 = 0x03F3,
        JoinMap = 0x15F3,
        JoinMapS17Kor = 0x604A,
        JoinMapS17KorResp = 0x1652,
        JoinMap2S17Kor = 0x024A,
        JoinMap2S17KorResp = 0x2452,
        ServerMove = 0x00B1,
        ServerMoveAuth = 0x01B1,

        ResetList = 0x0AFA,
        Resets = 0x0BFA,
        EnableCreate = 0x00DE,
        ChannelList = 0x57EC,
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

        ItemSplit = 0x00E8,

        Tax = 0x1AB2,
        KillCount = 0x01B8,
        ClientClose = 0x02F1,
        ClientMessage = 0x03F1,
        ClientMessageS17K75 = 0x03F3,
        Eventnotification = 0xFEF1,
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
        MasterLevelUp = 0x51F3,
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
        BeattackS16 = 0xFFD3,
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
        PeriodicEffect = 0xFF2D,
        Talk = 0xFF30,
        CloseWindow = 0xFF31,
        CancelItemSale = 0x006F,
        CancelItemSaleClose = 0x016F,
        CancelItemSaleItem = 0x026F,
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
        PShopAlterVault = 0xA8FA,
        PShopCloseDeal = 0x073F,
        PShopRequestSold = 0x083F,
        PShopSearchItem = 0x31EC,
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

        OpenBox = 0xF2D0,

        Move = 0xFFD3,
        MoveEng = 0xFFD4,
        Move12Eng = 0xFFD7,
        Attack = 0xFFD7,
        AttackEng = 0xFF11,
        Position = 0xFFDF,
        Position9Eng = 0xFF15,

        MiniMapNPC = 0x03E7,
        PeriodItemCount = 0x11D2,

        PentagramaJInfo = 0x01EE,
        UBFInfo = 0x01CD,
        PopUpType = 0x26F3,

        MemberPosInfoStart = 0x01E7,
        MemberPosInfoStop = 0x02E7,
        LifeInfo = 0x10EC,
        NPCJulia = 0x17BF,
        MuHelperSwitch = 0x51BF,
        MuHelper = 0xFFAE,
        AttackSpeed = 0x30EC,
        KillPlayerEXT = 0xFF9C,
        NPCDialog = 0x01F9,
        QuestExp = 0x30F6,
        ShadowBuff = 0x31F6,
        ChainMagic = 0x0ABF,

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
        ElementDamage = 0xFFD8,
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
        InventoryEquipament = 0x20BF,
        EquipamentChange = 0xFF25,
        Attack12Eng = 0xFFDF,
        SXUpPront = 0x25EC,
        SXElementalData = 0x26EC,
        SXInfo = 0x27EC,
        SXCharacterInfo = 0x29EC,
        NewQuestWorldLoad = 0x20F6,
        NewQuestWorldList = 0x50F6,
        PKLevel = 0x08F3,
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
        HuntingRecordRequest = 0x50EC,
        HuntingRecordClose = 0x51EC,
        HuntingRecordVisibility = 0x52EC,
        HuntingRecordDay = 0x53EC,
        HuntingRecordCurrent = 0x55EC,
        MossMerchant = 0x1170,
        MossMerchantOpenBox = 0x1070,
        MossMerchantOpenBoxReward = 0x1270,
        GremoryCaseOpenS16 = 0x06CD,

        MiningSystemUnk = 0x204C,
        MajesticInfo = 0x027E,
        MajesticStatsInfo = 0x067E,
        Position16Kor = 0xFF10,

        PShopSearchS16Kor = 0x007C,
        PShopItemSearchS16Kor = 0x017C,
        PShopItemSearch2S16Kor = 0x037C,
        PShopRequestList2S16Kor = 0x067C,
        PShopItemViewS16Kor = 0x077C,
        PShopSetItemPriceS16Kor = 0x087C,
        PShopCancelItemSaleS16Kor = 0x097C,
        PShopChangeState = 0x0A7C,
        ChangeSkin = 0x21F3,
        MonsterSoulShop = 0x424D,
        MonsterSoulAvailableShop = 0x464D,
        Ruudbuy = 0xF0D0,
        RuudOpenBox = 0xF1D0,
        RuudSend = 0xF1D0,
    }

    public enum GensOpCode : ushort
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

    public enum GuildOpCode : ushort
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

    public enum EventOpCode : ushort
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
        Banner = 0x184D,

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

        EventInventoryOpen = 0x0F4D,
        EventItemGet = 0x004D,
        EventItemThrow = 0x014D,
        EventInventory = 0x024D,

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
        SetQuestState = 0xFFA2,
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
