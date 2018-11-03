using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Guild
{
    [WZContract(LongMessage = true)]
    public class SGuildViewPort
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public GuildViewPortDto[] Guilds { get; set; }

        public SGuildViewPort()
        {
            Guilds = Array.Empty<GuildViewPortDto>();
        }
    }
}
