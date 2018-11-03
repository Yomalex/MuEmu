using CSEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace CSEmu.Network.Services
{
    [WZContract]
    public class SConnectResult : IMainMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }

        public SConnectResult()
        { }

        public SConnectResult(byte result)
        {
            Result = result;
        }
    }

    [WZContract(LongMessage = true)]
    public class SServerList : IMainMessage
    {
        [WZMember(0)]
        public byte CountH { get; set; }

        [WZMember(1)]
        public byte CountL { get; set; }

        [WZMember(2, SerializerType = typeof(ArraySerializer))]
        public ServerDto[] List { get; set; }

        public SServerList()
        { }

        public SServerList(ServerDto[] list)
        {
            CountH = (byte)(list.Length >> 8);
            CountL = (byte)(list.Length & 0xff);
            List = list;
        }
    }

    [WZContract]
    public class SServerInfo : IMainMessage
    {
        [WZMember(0, 16)]
        public byte[] btAddress { get; set; }

        [WZMember(1)]
        public ushort Port { get; set; }

        public string Address
        {
            set => btAddress = value.GetBytes();
            get => btAddress.MakeString();
        }

        public SServerInfo()
        {
            btAddress = Array.Empty<byte>();
        }
    }
}
