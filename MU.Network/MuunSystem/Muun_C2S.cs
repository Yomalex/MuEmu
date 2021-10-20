using MU.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.MuunSystem
{
    [WZContract(Serialized = true)]
    public class CMuunItemGet : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CMuunItemUse : IGameMessage
    {
        [WZMember(0)] public byte inventoryPos { get; set; }
        [WZMember(1)] public byte invenrotyTarget { get; set; }
        [WZMember(2)] public byte btItemUseType { get; set; }
    }

    [WZContract(Serialized = true)]
    public class CMuunItemSell : IGameMessage
    {
        [WZMember(0)] public byte inventoryPos { get; set; }
    }

    [WZContract]
    public class CMuunItemRideSelect : IGameMessage
    {
        [WZMember(0)] public ushort wzNumber { get; set; }
        public ushort Number { get => wzNumber.ShufleEnding(); set => wzNumber = value.ShufleEnding(); }
    }

    [WZContract]
    public class CMuunItemExchange : IGameMessage
    {
        [WZMember(0)] public byte select { get; set; }
    }
}
