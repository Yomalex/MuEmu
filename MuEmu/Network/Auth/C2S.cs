using BlubLib.Serialization;
using MuEmu.Network.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Auth
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

        [WZMember(4, 5)]
        public byte[] btClientVersion { get; set; }

        [WZMember(5, 16)]
        public byte[] btClientSerial { get; set; }

        public string Account => btAccount.MakeString();
        public string Password => btPassword.MakeString();
        public string ClientVersion => btClientVersion.MakeString();
        public string ClientSerial => btClientSerial.MakeString();
    }

    [WZContract]
    public class CCharacterList : IAuthMessage
    { }

    [WZContract]
    public class CCharacterCreate : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] btName { get; set; }

        [WZMember(1)]
        public HeroClass Class { get; set; }

        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CCharacterDelete : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] btName { get; set; }
        public string Name => btName.MakeString();

        [WZMember(0, 10)]
        public byte[] JoominNumber { get; set; }
    }

    [WZContract]
    public class CCharacterMapJoin : IAuthMessage
    {
        [WZMember(0,10)]
        public byte[] btName { get; set; }

        public string Name => btName.MakeString();
    }

    [WZContract]
    public class CCharacterMapJoin2 : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] Name { get; set; }
    }
}
