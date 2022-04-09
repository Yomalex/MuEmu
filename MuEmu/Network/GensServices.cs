using MU.Network.GensSystem;
using MU.Resources;
using MuEmu.Entity;
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
        [MessageHandler(typeof(CRequestLeave))]
        public async Task RequestLeave(GSSession session)
        {
            var @char = session.Player.Character;
            if (@char.Guild != null && @char.Guild.Master.Player == session.Player)
            {
                await session.SendAsync(new SGensLeaveResult(2, (ushort)session.ID));
                return;
            }
            if (@char.Gens.Influence == GensType.None)
            {
                await session.SendAsync(new SGensLeaveResult(1, (ushort)session.ID));
                return;
            }
            if (@char.Party != null)
            {
                await session.SendAsync(new SGensLeaveResult(4, (ushort)session.ID));
                return;
            }

            @char.Gens.Leave();
            await session.SendAsync(new SGensLeaveResult(1, (ushort)session.ID));
        }

        [MessageHandler(typeof(CRequestReward))]
        public async Task RequestReward(GSSession session, CRequestReward message)
        {
            var result = new SGensReward { ItemType = 3 };
            var @char = session.Player.Character;

            
            if(!@char.Inventory.TryAdd())
            {
                await session.SendAsync(result);
                return;
            }

            result.ItemType = 0;

            await session.SendAsync(result);
        }

        [MessageHandler(typeof(CRequestMemberInfo))]
        public void RequestMemberInfo(GSSession session)
        {
            var gens = session.Player.Gens;
            using(var db = new GameContext())
            {
                var info = db.Gens.Find(session.Player.Character.Id);
                gens?.UpdateInfo(info);
                gens?.SendMemberInfo();
            }
        }
    }
}
