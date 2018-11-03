using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class SpellDto
    {
        [WZMember(0)]
        public byte Index { get; set; }

        [WZMember(1)]
        public ushort Spell { get; set; }

        [WZMember(2)]
        public byte Padding { get; set; }
    }
}
