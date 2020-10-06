using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.PCPShop
{
    public interface IPCPShopMessage
    { }
    class PCPShopMessageFactory : MessageFactory<PCPShopOpCode, IPCPShopMessage>
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
