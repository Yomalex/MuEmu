using MU.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.MuunSystem
{
    [WZContract]
    public class SMuunRideVP : IGameMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public MuunRideVPDto[] ViewPort { get; set; }
    }

    [WZContract]
    public class MuunRideVPDto
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        [WZMember(1)] public ushort wzMuunRideItem { get; set; }

        public MuunRideVPDto()
        {

        }
        public MuunRideVPDto(ushort Number, ushort MuunItem)
        {
            wzNumber = Number.ShufleEnding();
            wzMuunRideItem = MuunItem.ShufleEnding();
        }
    }

    [WZContract(Serialized = true)]
    public class SMuunItemGet : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1, 12)] public byte[] Item { get; set; }
    }
}
