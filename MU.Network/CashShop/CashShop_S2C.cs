using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.CashShop
{
    [WZContract]
    public class SCashPoints : ICashMessage
    {
        [WZMember(0)]
        public int CashPoints { get; set; }
    }
    [WZContract]
    public class SCashPointsS9 : ICashMessage
    {
        [WZMember(0)]
        public byte ViewType { get; set; }
        [WZMember(1)]
        public long TotalCash { get; set; }
        [WZMember(2)]
        public long Cash_C { get; set; }
        [WZMember(3)]
        public long Cash_P { get; set; }
        [WZMember(4)]
        public long TotalPoint { get; set; }
        [WZMember(5)]
        public long GoblinPoint { get; set; }
    }
    [WZContract]
    public class SCashInit : ICashMessage
    { }

    [WZContract]
    public class SCashVersion : ICashMessage
    {
        [WZMember(0)]
        public ushort Ver1 { get; set; }
        [WZMember(1)]
        public ushort Ver2 { get; set; }
        [WZMember(2)]
        public ushort Ver3 { get; set; }
    }

    [WZContract]
    public class SCashBanner : ICashMessage
    {
        [WZMember(0)]
        public ushort Ver1 { get; set; }
        [WZMember(1)]
        public ushort Ver2 { get; set; }
        [WZMember(2)]
        public ushort Ver3 { get; set; }
    }
    [WZContract]
    public class SCashOpen : ICashMessage
    {
        [WZMember(0)]
        public byte Result { get; set; }
    }

    [WZContract]
    public class SCashInventoryItem : ICashMessage
    {
        [WZMember(0)]
        public ushort TotalItemCount { get; set; }
        [WZMember(1)]
        public ushort CurrentItemCount { get; set; }
        [WZMember(2)]
        public ushort PageIndex { get; set; }
        [WZMember(3)]
        public ushort TotalPage { get; set; }
    }
    [WZContract]
    public class SCashItemBuy : ICashMessage
    {
        [WZMember(1)]
        public byte Result { get; set; }
        [WZMember(2)]
        public int Unknown { get; set; }
    }
}
