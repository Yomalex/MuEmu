using MU.Resources;
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
        public PCPShopMessageFactory(ServerSeason Season)
        {
            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, true);
            Register<CPCPShopItems>(PCPShopOpCode.PCPShopInfo);
            Register<CPCPShopBuy>(PCPShopOpCode.PCPShopBuy);

            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, false);
            Register<SPCPShopInfo>(PCPShopOpCode.PCPShopInfo);
            Register<SPCPShopItems>(PCPShopOpCode.PCPShopItems);
            Register<SPCPShopPoints>(PCPShopOpCode.PCPShopPoints);
        }     
    }
}
