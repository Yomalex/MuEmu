using BlubLib.Serialization;
using MuEmu.Network.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Auth
{
    [WZContract]
    public class CIDAndPass : IAuthMessage
    {
        [WZMember(0, 10)]
        public byte[] Account { get; set; }

        [WZMember(1, 10)]
        public byte[] Password { get; set; }

        [WZMember(2)]
        public uint TickCount { get; set; }

        [WZMember(3, 7)]
        public byte[] ClientVersion { get; set; }
    }
}
