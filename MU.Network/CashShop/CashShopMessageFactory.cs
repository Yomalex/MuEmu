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
            Register<SCashPointsS9>(CashOpCode.CashPointsS9);
            Register<SCashPoints>(CashOpCode.CashPoints);
            VersionSelector.Register<SCashPoints>(ServerSeason.Season6Kor, CashOpCode.CashPoints);
            VersionSelector.Register<SCashPointsS9>(ServerSeason.Season9Eng, CashOpCode.CashPoints);
            //VersionSelector.Register<SCashPointsS9>(ServerSeason.Season12Eng, CashOpCode.CashPoints);
            Register<SCashInit>(CashOpCode.CashInit);
            Register<SCashVersion>(CashOpCode.CashVersion);
            Register<SCashBanner>(CashOpCode.CashBanner);
            Register<SCashOpen>(CashOpCode.CashOpen);
            Register<SCashInventoryItem>(CashOpCode.CashInventoryCount);
            Register<SCashItemBuy>(CashOpCode.CashItemBuy);
            Register<SCashItemList>(CashOpCode.CashItemList);

            switch(Season)
            {
                case ServerSeason.Season17Kor75:
                    ChangeOPCode<SCashInit>(Data.ProtocolXChangeS17K75(CashOpCode.CashInit, false));
                    ChangeOPCode<SCashVersion>(Data.ProtocolXChangeS17K75(CashOpCode.CashVersion, false));
                    ChangeOPCode<SCashBanner>(Data.ProtocolXChangeS17K75(CashOpCode.CashBanner, false));
                    ChangeOPCode<SCashPointsS9>(Data.ProtocolXChangeS17K75(CashOpCode.CashPoints, false));
                    break;
            }
        }
    }
}
