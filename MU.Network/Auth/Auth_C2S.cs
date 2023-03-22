using BlubLib.Serialization;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.Auth
{
    [WZContract]
    public class CIDAndPass : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] btAccount { get; set; }

        [WZMember(1, 10)]
        public byte[] btPassword { get; set; }

        [WZMember(2)]
        public uint TickCount { get; set; }

        [WZMember(3)]
        public ushort Padding { get; set; }

        [WZMember(4, typeof(BinaryStringSerializer), 5)]
        public string ClientVersion { get; set; }

        [WZMember(5, typeof(BinaryStringSerializer), 16)]
        public string ClientSerial { get; set; }

        public string Account => btAccount.MakeString();
        public string Password => btPassword.MakeString();
    }

    [WZContract]
    public class CIDAndPassS12 : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] btAccount { get; set; }

        [WZMember(1, 20)]
        public byte[] btPassword { get; set; }

        [WZMember(3)]
        public uint TickCount { get; set; }

        [WZMember(4, typeof(BinaryStringSerializer), 5)]
        public string ClientVersion { get; set; }

        [WZMember(5, typeof(BinaryStringSerializer), 16)]
        public string ClientSerial { get; set; }

        public string Account => btAccount.MakeString();
        public string Password => btPassword.MakeString();
    }

    [WZContract]
    public class CCharacterList : IAuthMessage
    { }

    [WZContract]
    public class CServerList : IAuthMessage
    { }

    [WZContract]
    public class CCharacterCreate : IAuthMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; }

        [WZMember(1)]
        public HeroClass Class { get; set; }
    }

    [WZContract]
    public class CCharacterDelete : IAuthMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; }

        [WZMember(1, typeof(BinaryStringSerializer), 10)]
        public string JoominNumber { get; set; }
    }

    [WZContract]
    public class CCharacterMapJoin : IAuthMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; }
    }

    [WZContract]
    public class CCharacterMapJoin2 : IAuthMessage
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; }
    }

    [WZContract]
    public class CServerMove : IAuthMessage
    {
        [WZMember(0, 12)] public byte[] btAccount { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 12)] public string Character { get; set; }
        [WZMember(2)] public uint AuthCode1 { get; set; }
        [WZMember(3)] public uint AuthCode2 { get; set; }
        [WZMember(4)] public uint AuthCode3 { get; set; }
        [WZMember(5)] public uint AuthCode4 { get; set; }
        [WZMember(6)] public uint TickCount { get; set; }
        [WZMember(7, typeof(BinaryStringSerializer), 5)] public string ClientVersion { get; set; }
        [WZMember(8, typeof(BinaryStringSerializer), 16)] public string ClientSerial { get; set; }

        public string Account => btAccount.MakeString();
    }
}
