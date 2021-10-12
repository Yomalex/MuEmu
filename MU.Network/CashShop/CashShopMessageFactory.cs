using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.CashShop
{
    public interface ICashMessage
    { }

    public class CashShopMessageFactory : MessageFactory<CashOpCode, ICashMessage>
    {
        public CashShopMessageFactory(ServerSeason Season)
        {
            //C2S
            Register<CCashOpen>(CashOpCode.CashOpen);
            Register<CCashPoints>(CashOpCode.CashPointsS9);
            Register<CCashInventoryItem>(CashOpCode.CashInventoryCount);
            Register<CCashItemBuy>(CashOpCode.CashItemBuy);

            // S2C
            switch(Season)
            {
                case ServerSeason.Season9Eng:
                    Register<SCashPointsS9>(CashOpCode.CashPointsS9);
                    break;
                default:
                    Register<SCashPoints>(CashOpCode.CashPoints);
                    break;
            }
            VersionSelector.Register<SCashPoints>(ServerSeason.Season6Kor, CashOpCode.CashPoints);
            VersionSelector.Register<SCashPointsS9>(ServerSeason.Season9Eng, CashOpCode.CashPoints);
            Register<SCashInit>(CashOpCode.CashInit);
            Register<SCashVersion>(CashOpCode.CashVersion);
            Register<SCashBanner>(CashOpCode.CashBanner);
            Register<SCashOpen>(CashOpCode.CashOpen);
            Register<SCashInventoryItem>(CashOpCode.CashInventoryCount);
            Register<SCashItemBuy>(CashOpCode.CashItemBuy);
            Register<SCashItemList>(CashOpCode.CashItemList);
        }
    }
}
