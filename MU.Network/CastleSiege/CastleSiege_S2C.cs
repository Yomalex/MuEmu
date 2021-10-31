using MU.Network.Event;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.CastleSiege
{
    [WZContract]
    public class SLeftTimeAlarm : IEventMessage
    {
        public byte Hour { get; set; }    // 4
        public byte Minute { get; set; }	// 5
    }

    [WZContract]
    public class SSiegeState : IEventMessage
    {
		[WZMember(0)] public byte Result { get; set; }
		[WZMember(1)] public byte CastleSiegeState { get; set; }
		[WZMember(2)] public ushort StartYear { get; set; }
		[WZMember(3)] public byte StartMonth { get; set; }
		[WZMember(4)] public byte StartDay { get; set; }
		[WZMember(5)] public byte StartHour { get; set; }
		[WZMember(6)] public byte StartMinute { get; set; }
		[WZMember(7)] public ushort EndYear { get; set; }
		[WZMember(8)] public byte EndMonth { get; set; }
		[WZMember(9)] public byte EndDay { get; set; }
		[WZMember(10)] public byte EndHour { get; set; }
		[WZMember(11)] public byte EndMinute { get; set; }
		[WZMember(12)] public ushort SiegeStartYear { get; set; }
		[WZMember(13)] public byte SiegeStartMonth { get; set; }
		[WZMember(14)] public byte SiegeStartDay { get; set; }
		[WZMember(15)] public byte SiegeStartHour { get; set; }
		[WZMember(16)] public byte SiegeStartMinute { get; set; }
		[WZMember(17, typeof(BinaryStringSerializer), 8)] public string OwnerGuild { get; set; }
		[WZMember(18, typeof(BinaryStringSerializer), 10)] public string OwnerGuildMaster { get; set; }
		[WZMember(19)] public int StateLeftSec { get; set; }
	}

	[WZContract]
	public class SGuildRegisteInfo : IEventMessage
    {
		[WZMember(1)] public byte Result { get; set; }
		[WZMember(2, typeof(BinaryStringSerializer), 8)] public string GuildName { get; set; }
		[WZMember(3)] public int wzGuildMark { get; set; }
		[WZMember(4)] public byte IsGiveUp { get; set; }
		[WZMember(5)] public byte RegRank { get; set; }

		public int GuildMark { get => wzGuildMark.ShufleEnding(); set => wzGuildMark = value.ShufleEnding(); }
	}
	[WZContract]
	public class SGuildMarkOfCastleOwner : IEventMessage
    {
		[WZMember(0, 32)] public byte[] Mark { get; set; }
	}
	[WZContract]
	public class SGuildRegiste : IEventMessage
	{
		[WZMember(1)] public byte Result { get; set; }
		[WZMember(2, typeof(BinaryStringSerializer), 8)] public string GuildName { get; set; }
	}

	[WZContract]
	public class SiegueGuildDto
    {
		/*<thisrel this+0x0>*/ /*|0x8|*/
		[WZMember(0, typeof(BinaryStringSerializer), 8)] public string GuildName { get; set; }
		/*<thisrel this+0x8>*/ /*|0x4|*/
		[WZMember(1)] public int wzRegMarks { get; set; }
		/*<thisrel this+0xc>*/ /*|0x1|*/
		[WZMember(2)] public byte IsGiveUp { get; set; }
		/*<thisrel this+0xd>*/ /*|0x1|*/
		[WZMember(3)] public byte SeqNum { get; set; }

		public int RegMarks { get => wzRegMarks.ShufleEnding(); set => wzRegMarks = value.ShufleEnding(); }
	}
	[WZContract(LongMessage = true)]
	public class SSiegeGuildList : IEventMessage
	{
		[WZMember(1)] public byte Result { get; set; }
		[WZMember(2)] public byte Padding1 { get; set; }
		[WZMember(3)] public ushort Padding2 { get; set; }
		[WZMember(4, typeof(ArrayWithScalarSerializer<int>))] public SiegueGuildDto[] List { get; set; }
	}

	[WZContract]
	public class SSiegeRegisteMark : IEventMessage
	{
		[WZMember(1)] public byte Result { get; set; }
		[WZMember(2, typeof(BinaryStringSerializer), 8)] public string GuildName { get; set; }
		[WZMember(3)] public int wzGuildMark { get; set; }

		public int GuildMark { get => wzGuildMark.ShufleEnding(); set => wzGuildMark = value.ShufleEnding(); }
	}

	[WZContract]
	public class SJoinSideNotify : IEventMessage
	{
		[WZMember(1)] public byte Side { get; set; }
	}

	[WZContract]
	public class SCastleSiegeNotifyStart : IEventMessage
	{
		[WZMember(1)] public byte State { get; set; }
	}

	[WZContract]
	public class SCastleSiegeNotifySwitch : IEventMessage
	{
		[WZMember(0)] public ushort wzIndex { get; set; }
		[WZMember(1)] public ushort wzUserIndex { get; set; }
		[WZMember(2)] public byte State { get; set; }

		public ushort Index { get => wzIndex.ShufleEnding(); set => wzIndex = value.ShufleEnding(); }
		public ushort UserIndex { get => wzUserIndex.ShufleEnding(); set => wzUserIndex = value.ShufleEnding(); }
	}


	[WZContract]
	public class CastleSiegeMinimapDta : IEventMessage
	{
		[WZMember(0)] public byte X { get; set; }
		[WZMember(1)] public byte Y { get; set; }
	}

	[WZContract(LongMessage = true)]
	public class SCastleSiegeMinimapData : IEventMessage
	{
		[WZMember(0, typeof(ArrayWithScalarSerializer<int>))] public CastleSiegeMinimapDta[] List { get; set; }
	}


	[WZContract]
	public class CastleSiegeMinimapNPCDta : IEventMessage
	{
		[WZMember(0)] public byte Type { get; set; }
		[WZMember(1)] public byte X { get; set; }
		[WZMember(2)] public byte Y { get; set; }
	}

	[WZContract(LongMessage = true)]
	public class SCastleSiegeMinimapNPCData : IEventMessage
	{
		[WZMember(0, typeof(ArrayWithScalarSerializer<int>))] public CastleSiegeMinimapNPCDta[] List { get; set; }
	}

	[WZContract]
	public class SCastleSiegeNotifySwitchInfo : IEventMessage
	{
		[WZMember(0)] public ushort wzIndex { get; set; }
		[WZMember(1)] public byte State { get; set; }
		[WZMember(2)] public byte Side { get; set; }
		[WZMember(3, typeof(BinaryStringSerializer), 8)] public string GuildName { get; set; }
		[WZMember(4, typeof(BinaryStringSerializer), 10)] public string UserName { get; set; }
		public ushort Index { get => wzIndex.ShufleEnding(); set => wzIndex = value.ShufleEnding(); }
	}

	[WZContract]
	public class SCastleSiegeNotifyCrownState : IEventMessage
	{
		[WZMember(1)] public byte State { get; set; }
	}
}
