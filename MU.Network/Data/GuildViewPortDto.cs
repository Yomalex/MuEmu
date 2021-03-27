using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class GuildViewPortDto
    {
        [WZMember(0)]
        public int ID { get; set; }

        [WZMember(1)]
        public GuildStatus Status { get; set; }

        [WZMember(2)]
        public byte Type { get; set; }

        [WZMember(3)]
        public GuildRelation RelationShip { get; set; }

        [WZMember(4)]
        public ushort wzNumber { get; set; }

        [WZMember(5)]
        public byte CastleState { get; set; }

        public ushort Number { get => wzNumber.ShufleEnding(); set { wzNumber = value.ShufleEnding(); } }
    }
}
