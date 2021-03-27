using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.PCPShop
{
    public interface IPCPShopMessage
    { }
    public class PCPShopMessageFactory : MessageFactory<PCPShopOpCode, IPCPShopMessage>
    {
        public PCPShopMessageFactory()
        {
            Register<CPCPShopItems>(PCPShopOpCode.PCPShopInfo);
            Register<SPCPShopInfo>(PCPShopOpCode.PCPShopInfo);

            Register<SPCPShopItems>(PCPShopOpCode.PCPShopItems);

            Register<CPCPShopBuy>(PCPShopOpCode.PCPShopBuy);

            Register<SPCPShopPoints>(PCPShopOpCode.PCPShopPoints);
        }     
    }
}
