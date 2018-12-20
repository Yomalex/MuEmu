using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Event
{
    public interface IEventMessage
    { }

    public class EventMessageFactory : MessageFactory<EventOpCode, IEventMessage>
    {
        public EventMessageFactory()
        {
            Register<CEventRemainTime>(EventOpCode.RemainTime);


            Register<SEventRemainTime>(EventOpCode.RemainTime);
        }
    }
}
