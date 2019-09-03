using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class PartyDto
    {
        [WZMember(0, 10)]
        public byte[] btId { get; set; }

        [WZMember(1)]
        public byte Number { get; set; }

        [WZMember(2)]
        public Maps Map { get; set; }

        [WZMember(3)]
        public byte X { get; set; }

        [WZMember(4)]
        public byte Y { get; set; }

        [WZMember(5)]
        public ushort Padding1 { get; set; }

        [WZMember(6)]
        public int Life { get; set; }

        [WZMember(7)]
        public int MaxLife { get; set; }

        public string Id { get => btId.MakeString(); set => btId = value.GetBytes(); }
    }
}
