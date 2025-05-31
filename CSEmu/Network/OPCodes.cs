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
        GSClientAdd = 0xFF12,
        GSClientChat = 0xFF13,
        GSClientRem = 0xFF14,
        ServerInfo = 0x03F4,
        ServerList = 0x06F4,
        ServerListS0 = 0x02F4,
        Unk = 0xFFA9,
    }

    enum ChatOpCode : ushort
    {
        Auth = 0xFF00,
        Unk1,
        Unk2,
        Unk3,
        Message,
        Keep,
    }
}
