using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class GuildViewPortDto
    {
        [WZMember(0)]
        public int ID { get; set; }

        [WZMember(1)]
        public byte Status { get; set; }

        [WZMember(2)]
        public byte Type { get; set; }

        [WZMember(3)]
        public byte RelationShip { get; set; }

        [WZMember(4)]
        public byte NumberH { get; private set; }

        [WZMember(5)]
        public byte NumberL { get; private set; }

        public int Number { get => (NumberH << 8 | NumberL); set { NumberH = (byte)(value >> 8); NumberL = (byte)(value & 0xFF); } }
    }
}
