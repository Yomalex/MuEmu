using System;
using System.Collections.Generic;
using System.Text;

namespace CSEmu.Network
{
    enum MainOpCode : ushort
    {
        Join = 0xFF00,
        GSJoin = 0xFF10,
        GSKeep = 0xFF11,
        ServerInfo = 0x03F4,
        ServerList = 0x06F4,
        Unk = 0xFFA9,
    }
}
