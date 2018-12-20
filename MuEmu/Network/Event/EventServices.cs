using System;
using System.Collections.Generic;
using System.Text;
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
                    res.RemainTime = 0;
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
    }
}
