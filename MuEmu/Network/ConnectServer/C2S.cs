using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.ConnectServer
{
    [WZContract]
    public class CWelcome : ICSMessage
    { }

    [WZContract]
    public class CRegistryReq : ICSMessage
    {
        [WZMember(0)]
        public ushort Index { get; set; }

        [WZMember(1, 16)]
        public byte[] btAddress { get; set; }

        [WZMember(2)]
        public ushort Port { get; set; }

        [WZMember(3)]
        public byte Show { get; set; }

        [WZMember(4, 16)]
        public byte[] btToken { get; set; }

        public string Address { get => btAddress.MakeString(); set => btAddress = value.GetBytes(); }
        public string Token { get => btToken.MakeString(); set => btToken = value.GetBytes(); }
    }

    [WZContract]
    public class CKeepAlive : ICSMessage
    {
        [WZMember(0)]
        public ushort Index { get; set; }

        [WZMember(1)]
        public byte Load { get; set; }

        [WZMember(2, 16)]
        public byte[] btToken { get; set; }
        public string Token { get => btToken.MakeString(); set => btToken = value.GetBytes(); }
    }
}
