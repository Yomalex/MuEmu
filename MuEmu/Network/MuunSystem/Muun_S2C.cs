using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.MuunSystem
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
}
