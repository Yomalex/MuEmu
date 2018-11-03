using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
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

    [WZContract]
    public class SCharacterList : IAuthMessage
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

        public SCharacterList(byte maxClas, byte moveCnt, CharacterPreviewDto[] chars)
        {
            MaxClass = maxClas;
            MoveCount = moveCnt;
            CharacterList = chars;
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
        public HeroClass Class { get; set; }

        [WZMember(5, typeof(ArraySerializer))]
        public byte[] Equipament { get; set; }

        public SCharacterCreate()
        {
            btName = Array.Empty<byte>();
            Equipament = new byte[24];
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
        public byte[] Name { get; set; }

        [WZMember(2)]
        public ushort Level { get; set; }

        [WZMember(3)]
        public ControlCode ControlCode { get; set; }

        [WZMember(4)]
        public CharsetDto CharSet { get; set; }

        [WZMember(5)]
        public GuildStatus GuildStatus { get; set; }
    }

    [WZContract]
    public class SCharacterMapJoin : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] Name { get; set; }

        [WZMember(1)]
        public byte Valid { get; set; }
    }

    [WZContract(Serialized = true)] // 0xC3
    public class SCharacterMapJoin2 : IAuthMessage
    {
        [WZMember(0)]
        public byte MapX { get; set; }

        [WZMember(1)]
        public byte MapY { get; set; }

        [WZMember(2)]
        public Maps Map { get; set; }

        [WZMember(3)]
        public byte Direccion { get; set; }

        [WZMember(4)]
        public ulong Experience { get; set; }

        [WZMember(5)]
        public ulong NextExperience { get; set; }

        [WZMember(6)]
        public ushort LevelUpPoints { get; set; }

        [WZMember(7)]
        public ushort Str { get; set; }

        [WZMember(8)]
        public ushort Agi { get; set; }

        [WZMember(9)]
        public ushort Vit { get; set; }

        [WZMember(10)]
        public ushort Ene { get; set; }

        [WZMember(11)]
        public ushort Life { get; set; }

        [WZMember(12)]
        public ushort MaxLife { get; set; }

        [WZMember(13)]
        public ushort Mana { get; set; }

        [WZMember(14)]
        public ushort MaxMana { get; set; }

        [WZMember(15)]
        public ushort Shield { get; set; }

        [WZMember(16)]
        public ushort MaxShield { get; set; }

        [WZMember(17)]
        public ushort Stamina { get; set; }

        [WZMember(18)]
        public ushort MaxStamina { get; set; }

        [WZMember(19)]
        public ushort unk { get; set; }

        [WZMember(20)]
        public int Zen { get; set; }

        [WZMember(21)]
        public byte PKLevel { get; set; }

        [WZMember(22)]
        public byte ControlCode { get; set; }

        [WZMember(23)]
        public short AddPoints { get; set; }

        [WZMember(24)]
        public short MaxAddPoints { get; set; }

        [WZMember(25)]
        public ushort Cmd { get; set; }

        [WZMember(26)]
        public short MinusPoints { get; set; }

        [WZMember(27)]
        public short MaxMinusPoints { get; set; }

        [WZMember(28)]
        public byte ExpandedInv { get; set; }

        [WZMember(29)]
        public ushort Unk { get; set; }

        [WZMember(30)]
        public byte ExpandedVault { get; set; }

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
}
