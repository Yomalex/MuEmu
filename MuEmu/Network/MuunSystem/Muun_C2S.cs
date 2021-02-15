using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.MuunSystem
{
    [WZContract]
    public class CMuunRideReq : IGameMessage
    {
        [WZMember(0)] public ushort wzItemNumber { get; set; }
    }
}
