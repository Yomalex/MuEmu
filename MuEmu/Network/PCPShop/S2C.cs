using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.PCPShop
{
    [WZContract]
    public class SPCPShopPoints : IPCPShopMessage
    {
        [WZMember(0)]
        public ushort Points { get; set; }
        [WZMember(1)]
        public ushort MaxPoints { get; set; }

        public SPCPShopPoints()
        { }

        public SPCPShopPoints(ushort points, ushort maxPoints)
        {
            Points = points;
            MaxPoints = maxPoints;
        }

    }

    [WZContract]
    public class SPCPShopInfo : IPCPShopMessage
    {
        [WZMember(0)]
        public byte Unk { get; set; }

    }

    [WZContract(LongMessage = true)]
    public class SPCPShopItems : IPCPShopMessage
    {
        [WZMember(0)]
        public ushort wzCount { get; set; }

        public ushort Count { get => wzCount.ShufleEnding(); set => wzCount = value.ShufleEnding(); }

        [WZMember(1, typeof(ArraySerializer))]
        public PCPShopItemDto[] itemDtos { get; set; }

        public SPCPShopItems()
        {
            itemDtos = Array.Empty<PCPShopItemDto>();
            Count = 0;
        }

        public SPCPShopItems(PCPShopItemDto[] items)
        {
            itemDtos = items;
            Count = (ushort)items.Count();
        }
    }

    [WZContract]
    public class PCPShopItemDto
    {
        [WZMember(0)]
        public byte Position { get; set; }
        [WZMember(1)]
        public byte Index { get; set; }
        [WZMember(2)]
        public byte Opts { get; set; }
        [WZMember(3)]
        public byte Dur { get; set; }
        [WZMember(4)]
        public byte unk1 { get; set; }
        [WZMember(5)]
        public byte Exc { get; set; }
        [WZMember(6)]
        public byte Type16 { get; set; }
        [WZMember(7)]
        public SocketOption NewOpt1 { get; set; }
        [WZMember(8)]
        public SocketOption NewOpt2 { get; set; }
        [WZMember(9)]
        public SocketOption NewOpt3 { get; set; }
        [WZMember(10)]
        public SocketOption NewOpt4 { get; set; }
        [WZMember(11)]
        public SocketOption NewOpt5 { get; set; }
        [WZMember(12)]
        public byte unk2 { get; set; }
    }
}
