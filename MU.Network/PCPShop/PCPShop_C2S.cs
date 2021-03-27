using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.PCPShop
{
    [WZContract]
    public class CPCPShopItems : IPCPShopMessage
    { }

    [WZContract]
    public class CPCPShopBuy : IPCPShopMessage
    {
        [WZMember(0)]
        public byte Position { get; set; }
    }
}
