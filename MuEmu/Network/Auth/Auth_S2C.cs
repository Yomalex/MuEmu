using MU.DataBase;
using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Auth
{

    [WZContract] // 0xC1
    public class SJoinResult : IAuthMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        [WZMember(1)]
        public byte NumberH { get; set; }

        [WZMember(2)]
        public byte NumberL { get; set; }

        [WZMember(3, 5)]
        public byte[] ClientVersion { get; set; }

        public SJoinResult()
        {
            ClientVersion = Array.Empty<byte>();
        }

        public SJoinResult(byte result, int number, string clientVersion)
        {
            Result = result;
            NumberH = (byte)((number >> 8) & 0xff);
            NumberL = (byte)(number & 0xff);
            ClientVersion = clientVersion.GetBytes();
        }
    }

    [WZContract]
    public class SLoginResult : IAuthMessage
    {
        [WZMember(1)]
        public LoginResult Result { get; set; }

        public SLoginResult()
        { }

        public SLoginResult(LoginResult result)
        {
            Result = result;
        }
    }
    public abstract class CharList
    {
        public CharList()
        {

        }

        public CharList(byte maxClas, byte moveCnt, Dictionary<byte, MU.DataBase.CharacterDto> chars, byte CharSlotCount, byte WhSlotCount)
        {

        }
    }

    [WZContract]
    public class SCharacterList : CharList, IAuthMessage
    {
        [WZMember(0)]
        public byte MaxClass { get; set; }

        [WZMember(1)]
        public byte MoveCount { get; set; }

        //public byte Count { get; set; }

        [WZMember(2, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public CharacterPreviewDto[] CharacterList { get; set; }

        public SCharacterList()
        {
            CharacterList = Array.Empty<CharacterPreviewDto>();
        }

        public SCharacterList(byte maxClas, byte moveCnt, Dictionary<byte, MU.DataBase.CharacterDto> chars, byte CharSlotCount, byte WhSlotCount) : base(maxClas, moveCnt, chars, CharSlotCount, WhSlotCount)
        {
            MaxClass = maxClas;
            MoveCount = moveCnt;

            CharacterList = chars.Select(x => new CharacterPreviewDto(
                            x.Key,
                            x.Value.Name,
                            x.Value.Level,
                            (ControlCode)x.Value.CtlCode,
                            Inventory.GetCharset((HeroClass)x.Value.Class, new Inventory(null, x.Value), 0),
                            GuildManager.Instance.FindCharacter(x.Value.Name)?.Rank ?? GuildStatus.NoMember))
                        .ToArray();
        }
    }

    [WZContract]
    public class SCharacterListS9 : CharList, IAuthMessage
    {
        [WZMember(0)]
        public byte MaxClass { get; set; }

        [WZMember(1)]
        public byte MoveCount { get; set; }

        [WZMember(2)]
        public byte Count { get; set; }

        [WZMember(3)]
        public byte WhExpansion { get; set; }

        [WZMember(5, SerializerType = typeof(ArraySerializer))]
        public CharacterPreviewS9Dto[] CharacterList { get; set; }

        public SCharacterListS9()
        {
            CharacterList = Array.Empty<CharacterPreviewS9Dto>();
        }

        public SCharacterListS9(byte maxClas, byte moveCnt, Dictionary<byte, CharacterDto> chars, byte CharSlotCount, byte WhSlotCount) : base(maxClas, moveCnt, chars, CharSlotCount, WhSlotCount)
        {
            MaxClass = maxClas;
            MoveCount = moveCnt;
            CharacterList = chars.Select(x => new CharacterPreviewS9Dto(
                            x.Key,
                            x.Value.Name,
                            x.Value.Level,
                            (ControlCode)x.Value.CtlCode,
                            Inventory.GetCharset((HeroClass)x.Value.Class, new Inventory(null, x.Value), 0),
                            GuildManager.Instance.FindCharacter(x.Value.Name)?.Rank ?? GuildStatus.NoMember,
                            3))                            
                        .ToArray();

            Count = (byte)CharacterList.Length;
            WhExpansion = WhSlotCount;
        }
    }

    [WZContract]
    public class SCharacterListS12 : CharList, IAuthMessage
    {
        [WZMember(0)]
        public byte MaxClass { get; set; }

        [WZMember(1)]
        public byte MoveCount { get; set; }

        [WZMember(2)]
        public byte Count { get; set; }

        [WZMember(3)]
        public byte CharacterSlotCount { get; set; }

        [WZMember(4)]
        public byte WhExpansion { get; set; }

        [WZMember(5, SerializerType = typeof(ArraySerializer))]
        public CharacterPreviewS12Dto[] CharacterList { get; set; }

        public SCharacterListS12()
        {
            CharacterList = Array.Empty<CharacterPreviewS12Dto>();
        }

        public SCharacterListS12(byte maxClas, byte moveCnt, Dictionary<byte, CharacterDto> chars, byte CharSlotCount, byte WhSlotCount) : base(maxClas, moveCnt, chars, CharSlotCount, WhSlotCount)
        {
            MaxClass = maxClas;
            MoveCount = moveCnt;
            CharacterList = chars.Select(x => new CharacterPreviewS12Dto(
                            x.Key,
                            x.Value.Name,
                            x.Value.Level,
                            (ControlCode)x.Value.CtlCode,
                            Inventory.GetCharset((HeroClass)x.Value.Class, new Inventory(null, x.Value), 0),
                            GuildManager.Instance.FindCharacter(x.Value.Name)?.Rank ?? GuildStatus.NoMember,
                            3))
                        .ToArray();
            Count = (byte)CharacterList.Length;
            CharacterSlotCount = CharSlotCount;
            WhExpansion = WhSlotCount;
        }
    }

    [WZContract]
    public class SCharacterCreate : IAuthMessage
    {
        [WZMember(0)]
        public byte Result { get; set; } // 0: Error

        [WZMember(1, 10)]
        public byte[] btName { get; set; }

        [WZMember(2)]
        public byte Position { get; set; }

        [WZMember(3)]
        public ushort Level { get; set; }

        [WZMember(4)]
        public byte Class { get; set; }

        [WZMember(5, typeof(ArraySerializer))]
        public byte[] Equipament { get; set; }

        public SCharacterCreate()
        {
            btName = Array.Empty<byte>();
            Equipament = new byte[24];
        }

        public SCharacterCreate(byte result)
        {
            btName = Array.Empty<byte>();
            Equipament = new byte[24];

            Result = result;
        }

        public SCharacterCreate(byte result, string name, byte pos, ushort level, byte[] equip, byte @class)
        {
            btName = Array.Empty<byte>();
            Equipament = new byte[24];

            Result = result;
            Name = name;
            Position = pos;
            Level = level;
            Equipament = equip;
            Class = @class;
        }

        public string Name { get => btName.MakeString(); set => btName = value.GetBytes(); }
    }

    [WZContract]
    public class SCharacterDelete : IAuthMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }
    }

    [WZContract]
    public class CharacterPreviewDto
    {
        [WZMember(0)]
        public byte ID { get; set; }

        [WZMember(1, 11)]
        public byte[] btName { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public ControlCode ControlCode { get; set; }

        [WZMember(4, 18)]
        public byte[] CharSet { get; set; }

        [WZMember(5)]
        public GuildStatus GuildStatus { get; set; }

        [WZMember(6)]
        public byte PKlevel { get; set; }

        [WZMember(7)]
        public byte unk { get; set; }

        public CharacterPreviewDto() { }

        public CharacterPreviewDto(byte Id, string name, ushort level, ControlCode cc, byte[] charSet, GuildStatus gStatus)
        {
            ID = Id;
            btName = name.GetBytes();
            Level = level;
            ControlCode = cc;
            CharSet = charSet;
            GuildStatus = gStatus;
        }
    }

    [WZContract]
    public class CharacterPreviewS9Dto
    {
        [WZMember(0)]
        public byte ID { get; set; }

        [WZMember(1, 11)]
        public byte[] btName { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public ControlCode ControlCode { get; set; }

        [WZMember(4, 18)]
        public byte[] CharSet { get; set; }

        [WZMember(5)]
        public GuildStatus GuildStatus { get; set; }

        [WZMember(6)]
        public byte PKLevel { get; set; }

        public CharacterPreviewS9Dto() { }

        public CharacterPreviewS9Dto(byte Id, string name, ushort level, ControlCode cc, byte[] charSet, GuildStatus gStatus, byte pkLevel)
        {
            ID = Id;
            btName = name.GetBytes();
            Level = level;
            ControlCode = cc;
            CharSet = charSet;
            GuildStatus = gStatus;
            PKLevel = pkLevel;
        }
    }

    [WZContract]
    public class CharacterPreviewS12Dto
    {
        [WZMember(0)]
        public byte ID { get; set; }

        [WZMember(1, 11)]
        public byte[] btName { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public ControlCode ControlCode { get; set; }

        [WZMember(4, 18)]
        public byte[] CharSet { get; set; }

        [WZMember(5)]
        public GuildStatus GuildStatus { get; set; }

        [WZMember(6)]
        public byte PKLevel { get; set; }
        
        //[WZMember(7)]
        public byte unk { get; set; }

        public CharacterPreviewS12Dto() { }

        public CharacterPreviewS12Dto(byte Id, string name, ushort level, ControlCode cc, byte[] charSet, GuildStatus gStatus, byte pkLevel)
        {
            ID = Id;
            btName = name.GetBytes();
            Level = level;
            ControlCode = cc;
            CharSet = charSet;
            GuildStatus = gStatus;
            PKLevel = pkLevel;
        }
    }

    [WZContract]
    public class SCharacterMapJoin : IAuthMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; }

        [WZMember(1)]
        public byte Valid { get; set; }
    }

    [WZContract(Serialized = true)] // 0xC3
    public class SCharacterMapJoin2 : IAuthMessage
    {
        [WZMember(0)] public byte MapX { get; set; }//0
        [WZMember(1)] public byte MapY { get; set; }//1
        [WZMember(2)] public Maps Map { get; set; }//2
        [WZMember(3)] public byte Direccion { get; set; }//3
        [WZMember(4)] public ulong Experience { get; set; }//4
        [WZMember(5)] public ulong NextExperience { get; set; }//c
        [WZMember(6)] public ushort LevelUpPoints { get; set; }//14
        [WZMember(7)] public ushort Str { get; set; }//16
        [WZMember(8)] public ushort Agi { get; set; }//18
        [WZMember(9)] public ushort Vit { get; set; }//1a
        [WZMember(10)] public ushort Ene { get; set; }//1c
        [WZMember(11)] public ushort Life { get; set; }//1e
        [WZMember(12)] public ushort MaxLife { get; set; }//20
        [WZMember(13)] public ushort Mana { get; set; }//22
        [WZMember(14)] public ushort MaxMana { get; set; }//24
        [WZMember(15)] public ushort Shield { get; set; }//26
        [WZMember(16)] public ushort MaxShield { get; set; }//28
        [WZMember(17)] public ushort Stamina { get; set; }//2a
        [WZMember(18)] public ushort MaxStamina { get; set; }//2c
        [WZMember(19)] public ushort unk { get; set; }
        [WZMember(20)] public uint Zen { get; set; }//2e
        [WZMember(21)] public byte PKLevel { get; set; }//36
        [WZMember(22)] public byte ControlCode { get; set; }//37
        [WZMember(23)] public short AddPoints { get; set; }//38
        [WZMember(24)] public short MaxAddPoints { get; set; }//3a 
        [WZMember(25)] public ushort Cmd { get; set; }//3c
        [WZMember(26)] public short MinusPoints { get; set; }//3e
        [WZMember(27)] public short MaxMinusPoints { get; set; }//40
        [WZMember(28)] public byte ExpandedInv { get; set; }//41
        [WZMember(29)] public int Ruud { get; set; }//42
        //[WZMember(30)]
        //public byte ExpandedVault { get; set; }//44

        //45
        public Point Position
        {
            get => new Point(MapX, MapY);
            set
            {
                MapX = (byte)value.X;
                MapY = (byte)value.Y;
            }
        }
    }

    [WZContract]
    public class SServerMove : IAuthMessage
    {
        [WZMember(0, 16)] public byte[] IpAddress { get; set; }
        [WZMember(1)] public ushort ServerPort { get; set; }
        [WZMember(2)] public ushort ServerCode { get; set; }
        [WZMember(3)] public uint AuthCode1 { get; set; }
        [WZMember(4)] public uint AuthCode2 { get; set; }
        [WZMember(5)] public uint AuthCode3 { get; set; }
        [WZMember(6)] public uint AuthCode4 { get; set; }
    }

    [WZContract]
    public class SResets : IAuthMessage
    {
        [WZMember(0)] public ushort Resets { get; set; }
    }

    [WZContract]
    public class SEnableCreation : IAuthMessage
    {
        [WZMember(0)]
        public EnableClassCreation EnableCreation { get; set; }
    }
}
