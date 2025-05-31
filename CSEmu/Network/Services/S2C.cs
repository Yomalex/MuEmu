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

    [WZContract(LongMessage = true)]
    public class SServerListS0 : IMainMessage
    {
        [WZMember(0)]
        public byte CountH { get; set; }

        [WZMember(1)]
        public byte CountL { get; set; }

        [WZMember(2, SerializerType = typeof(ArraySerializer))]
        public ServerDto[] List { get; set; }

        public SServerListS0()
        { }

        public SServerListS0(ServerDto[] list)
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

    [WZContract]
    public class SCAdd : IMainMessage
    {
        [WZMember(0)]
        public byte Server { get; set; }

        [WZMember(1, 10)]
        public byte[] btName { get; set; }
    }

    [WZContract]
    public class SCChat : IMainMessage
    {
        [WZMember(0)]
        public byte Server { get; set; }

        [WZMember(1, 10)]
        public byte[] btName { get; set; }

        [WZMember(2, 100)]
        public byte[] btChat { get; set; }
    }

    [WZContract]
    public class SCRem : IMainMessage
    {
        [WZMember(0)]
        public byte Server { get; set; }

        [WZMember(1, typeof(ArrayWithScalarSerializer<short>))]
        public CliRemDto[] List { get; set; }
    }

    [WZContract]
    public class CliRemDto : IMainMessage
    {
        [WZMember(0, 10)]
        public byte[] btName { get; set; }
    }
}
