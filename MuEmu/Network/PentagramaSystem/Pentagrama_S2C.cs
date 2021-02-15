using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Pentagrama
{
	[WZContract(LongMessage = true)]
    public class SPentagramaJewelInfo : IGameMessage
    {
        [WZMember(0)] public byte Result { get; set; }
		[WZMember(1)] public byte JewelCnt { get; set; }
		[WZMember(2)] public byte JewelPos { get; set; }

		[WZMember(3, typeof(ArraySerializer))] public PentagramaJewelDto[] List { get; set; }
	}

	[WZContract]
    public class PentagramaJewelDto
	{
        [WZMember(0)] public byte Result { get; set; }
		[WZMember(1)] public byte JewelPos { get; set; }
		[WZMember(2)] public byte JewelIndex { get; set; }
		[WZMember(3)] public byte MainAttribute { get; set; }
		[WZMember(4)] public byte ItemType { get; set; }
		[WZMember(5)] public ushort ItemIndex { get; set; }
		[WZMember(6)] public byte Level { get; set; }
		[WZMember(7)] public byte Rank1OptionNum { get; set; }
		[WZMember(8)] public byte Rank1Level { get; set; }
		[WZMember(9)] public byte Rank2OptionNum { get; set; }
		[WZMember(10)] public byte Rank2Level { get; set; }
		[WZMember(11)] public byte Rank3OptionNum { get; set; }
		[WZMember(12)] public byte Rank3Level { get; set; }
		[WZMember(13)] public byte Rank4OptionNum { get; set; }
		[WZMember(14)] public byte Rank4Level { get; set; }
		[WZMember(15)] public byte Rank5OptionNum { get; set; }
		[WZMember(16)] public byte Rank5Level { get; set; }
	}
}
