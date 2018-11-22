using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.CashShop
{
    public interface ICashMessage
    { }

    public class CashShopMessageFactory : MessageFactory<CashOpCode, ICashMessage>
    {
        public CashShopMessageFactory()
        {
            // S2C
            Register<SCashPoints>(CashOpCode.CashPoints);

            //C2S
        }
    }
}
