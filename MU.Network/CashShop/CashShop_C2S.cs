using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.CashShop
{
    [WZContract]
    public class CCashOpen : ICashMessage
    { }
    [WZContract]
    public class CCashPoints : ICashMessage
    { }
    [WZContract]
    public class CCashInventoryItem : ICashMessage
    {
        [WZMember(0)]
        public int Page { get; set; }

        [WZMember(1)]
        public CSInventory InventoryType { get; set; }
    }
    [WZContract]
    public class CCashItemBuy : ICashMessage
    {
        [WZMember(0)]
        public int ItemIndex { get; set; }
        [WZMember(1)]
        public int Category { get; set; }
        [WZMember(2)]
        public int ItemOpt { get; set; }
        [WZMember(3)]
        public short ItemID { get; set; }
        [WZMember(4)]
        public CoinType Coin { get; set; }
    }
}
