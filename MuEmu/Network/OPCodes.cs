using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Network
{
    public enum GlobalOpCode : ushort
    {
        LiveClient = 0x000E,
    }

    public enum AuthOpCode : ushort
    {
        JoinResult = 0x00F1,
        Login = 0x01F1,
    }
}
