using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.Global
{
    [WZContract]
    public class SResult : IGlobalMessage
    {
        [WZMember(1)]
        public byte Result { get; set; }

        public SResult()
        { }

        public SResult(byte result)
        {
            Result = result;
        }
    }
}
