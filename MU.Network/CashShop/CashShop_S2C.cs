using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.CashShop
{
    [WZContract]
    public class SCashPoints : ICashMessage
    {
        [WZMember(0)]
        public int CashPoints { get; set; }

        public SCashPoints()
        {

        }
        public SCashPoints(int wzCoinC, int wzCoinP, int goblinP)
        {
            CashPoints = wzCoinP;
        }
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

        public SCashPointsS9()
        {

        }

        public SCashPointsS9(int wzCoinC, int wzCoinP, int goblinP)
        {
            ViewType = 1;
            TotalCash = wzCoinC;
            TotalPoint = goblinP + wzCoinP;
            GoblinPoint = goblinP;
            Cash_C = wzCoinC;
            Cash_P = wzCoinP;
        }
    }
    [WZContract]
    public class SCashPointsS16 : ICashMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(2)] public double Cash_C { get; set; }
        [WZMember(3)] public double Cash_P { get; set; }
        [WZMember(5)] public double GoblinPoint { get; set; }

        public SCashPointsS16()
        {

        }

        public SCashPointsS16(int wzCoinC, int wzCoinP, int goblinP)
        {
            Result = 1;
            GoblinPoint = goblinP;
            Cash_C = wzCoinC;
            Cash_P = wzCoinP;
        }
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
        public CSResult Result { get; set; }
        [WZMember(2)]
        public int Unknown { get; set; }
    }
    [WZContract]
    public class SCashItemList : ICashMessage
    {
        [WZMember(0)] public ushort aIndex { get; set; }
        [WZMember(1)] public CSInventory InvType { get; set; }
        [WZMember(2)] public byte InvNum { get; set; }
        [WZMember(3, typeof(BinaryStringSerializer), 11)] public string AccountID { get; set; }
        [WZMember(4)] public int Result { get; set; }
        [WZMember(5, typeof(ArrayWithScalarSerializer<int>))] public SCashItemDto[] Items { get; set; }
    }

    [WZContract]
    public class SCashItemDto
    {
        [WZMember(0)] public int Serial { get; set; }
        [WZMember(1)] public int AuthCode { get; set; }
        [WZMember(2)] public int SerialCashShop { get; set; }
        [WZMember(3)] public int Product { get; set; }
        [WZMember(4)] public int Otion { get; set; }
        [WZMember(5)] public CSInventory InventoryType { get; set; }
        [WZMember(6, typeof(BinaryStringSerializer), 10)] public string GiftName { get; set; }
        [WZMember(7, typeof(BinaryStringSerializer), 200)] public string Message { get; set; }
    }

    [WZContract]
    public class SCashItemList2 : ICashMessage
    {
        [WZMember(0)] public int Serial { get; set; }
        [WZMember(1)] public int AuthCode { get; set; }
        [WZMember(2)] public int Package { get; set; }
        [WZMember(3)] public int Category { get; set; }
        [WZMember(4)] public int NodeId { get; set; }
        [WZMember(5)] public double Value { get; set; }
        [WZMember(6)] public byte Unknow2 { get; set; }
    }

    [WZContract]
    public class SCashItemPreUse : ICashMessage
    {
        public enum CashItemUseResults : byte
        {
            Ok,
            DoesNotExists,
        }
        [WZMember(0)] public ushort Item { get; set; }
        [WZMember(1)] public CashItemUseResults result { get; set; }
    }

    [WZContract]
    public class SCashItemUse : ICashMessage
    {
        public enum CashItemUseResults : byte
        {
            Ok,
            DoesNotExists,
        }
        [WZMember(1)] public CashItemUseResults result { get; set; }
    }
}
