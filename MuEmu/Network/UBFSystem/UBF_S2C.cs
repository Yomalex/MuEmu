using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.UBFSystem
{
    [WZContract]
    public class SUBFInfo : IGameMessage
    {
        public byte Result { get; set; }
    }
}
