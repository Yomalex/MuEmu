using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Event
{
    [WZContract]
    public class SEventRemainTime : IEventMessage
    {
        [WZMember(0)]
        public EventEnterType EventType { get; set; }

        [WZMember(1)]
        public byte RemainTime { get; set; }

        [WZMember(2)]
        public byte EnteredUser { get; set; }

        [WZMember(3)]
        public byte RemainTime_LOW { get; set; }
    }
}
