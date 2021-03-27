using MU.Network.GensSystem;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace MuEmu.Network
{
    public class GensServices : MessageHandler
    {
        [MessageHandler(typeof(CRequestJoin))]
        public async Task RequestJoin(GSSession session, CRequestJoin message)
        {
            var @char = session.Player.Character;
            if(@char.Gens.Influence != GensType.None)
            {
                await session.SendAsync(new SRegMember(1, message.Influence));
                return;
            }
            if(@char.Level < 50)
            {
                await session.SendAsync(new SRegMember(3, message.Influence));
                return;
            }
            if(@char.Party != null)
            {
                await session.SendAsync(new SRegMember(6, message.Influence));
                return;
            }

            @char.Gens.Join(message.Influence);
            await session.SendAsync(new SRegMember(0, message.Influence));
        }
    }
}
