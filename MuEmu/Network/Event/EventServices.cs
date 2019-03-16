using MuEmu.Events.BloodCastle;
using MuEmu.Events.LuckyCoins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace MuEmu.Network.Event
{
    public class EventServices : MessageHandler
    {
        [MessageHandler(typeof(CEventRemainTime))]
        public void CEventRemainTime(GSSession session, CEventRemainTime message)
        {
            var res = new SEventRemainTime { EventType = message.EventType };
            switch (message.EventType)
            {
                case EventEnterType.DevilSquare:
                    res.RemainTime = 0;
                    break;
                case EventEnterType.BloodCastle:
                    res.RemainTime = BloodCastles.RemainTime();
                    break;
                case EventEnterType.ChaosCastle:
                    res.RemainTime = 0;
                    break;
                case EventEnterType.IllusionTemple:
                    res.RemainTime = 0;
                    break;
            }

            session.SendAsync(res);
        }

        [MessageHandler(typeof(CLuckyCoinsCount))]
        public async Task CLuckyCoinsCount(GSSession session/*, CLuckyCoinsCount message*/)
        {
            var coins = await LuckyCoins.Registered(session.Player);
            await session.SendAsync(new SLuckyCoinsCount(coins));
        }

        [MessageHandler(typeof(CLuckyCoinsRegistre))]
        public async Task CLuckyCoinsRegistre(GSSession session/*, CLuckyCoinsRegistre message*/)
        {
            var coins = await LuckyCoins.Registre(session.Player);
            await session.SendAsync(new SLuckyCoinsCount(coins));
        }

        [MessageHandler(typeof(CBloodCastleMove))]
        public async Task CBloodCastleMove(GSSession session, CBloodCastleMove message)
        {
            var plr = session.Player;
            var @char = session.Player.Character;

            var invisibleCloack = @char.Inventory.Get(message.ItemPos);
            var itemLevel = BloodCastles.GetNeededLevel(plr);

            if(invisibleCloack.Plus != message.Bridge && invisibleCloack.Number != ItemNumber.FromTypeIndex(13,47))
            {
                await session.SendAsync(new SBloodCastleMove(1));
                return;
            }

            if(itemLevel != invisibleCloack.Plus)
            {
                await session.SendAsync(new SBloodCastleMove((byte)(itemLevel > invisibleCloack.Plus ? 4 : 3)));
                return;
            }

            if (!BloodCastles.TryAdd(message.Bridge, plr))
            {
                await session.SendAsync(new SBloodCastleMove(5));
                return;
            }

            @char.Inventory.Delete(message.ItemPos);
        }

        [MessageHandler(typeof(CCrywolfBenefit))]
        public void CCrywolfBenefit(GSSession session)
        {
            session.SendAsync(new SCrywolfBenefit());
        }
    }
}
