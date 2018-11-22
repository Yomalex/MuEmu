using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.CashShop
{
    [WZContract]
    public class SCashPoints : ICashMessage
    {
        [WZMember(0)]
        public int CashPoints { get; set; }
    }
}
