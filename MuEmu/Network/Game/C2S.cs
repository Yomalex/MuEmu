using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Game
{
    [WZContract]
    public class CCheckSum : IGameMessage
    {
        [WZMember(0)]
        public ushort Key { get; set; }
    }

    [WZContract]
    public class CClientMessage : IGameMessage
    {
        [WZMember(0)]
        public HackCheck Flag { get; set; }
    }

    [WZContract]
    public class CCloseWindow : IGameMessage
    { }

    [WZContract]
    public class CAction : IGameMessage
    {
        [WZMember(0)]
        public byte Dir { get; set; }

        [WZMember(1)]
        public byte ActionNumber { get; set; }

        //[WZMember(2)]
        public ushort btTarget { get; set; }

        public ushort Target
        {
            get => btTarget.ShufleEnding();
            set
            {
                btTarget = value.ShufleEnding();
            }
        }
    }

    [WZContract]
    public class CMove : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; } // 3

        [WZMember(1)]
        public byte Y { get; set; } // 4

        [WZMember(2,8)]
        public byte[] Path{ get; set; }   // 5 - 8
    }

    [WZContract]
    public class CChatNickname : IGameMessage
    {
        [WZMember(0, 10)]
        public byte[] Character { get; set; } // 3

        [WZMember(1, 60)]
        public byte[] Message { get; set; } // 4
    }

    [WZContract]
    public class CChatNumber : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; } // 3

        [WZMember(1, 60)]
        public byte[] Message { get; set; } // 4
    }

    [WZContract]
    public class CNewQuestInfo : IGameMessage
    {
        [WZMember(0)]
        public ushort Number { get; set; } // 3,4

        [WZMember(1)]
        public ushort Quest { get; set; } // 5,6

        //[WZMember(2)]
        //public ushort Unk1 { get; set; } // 7

        //[WZMember(3)]
        //public ushort Unk2 { get; set; } // 8

        //[WZMember(4)]
        //public ushort Unk3 { get; set; } // 9

        //[WZMember(5)]
        //public ushort Unk4 { get; set; } // A
    }

    [WZContract]
    public class CPositionSet : IGameMessage
    {
        [WZMember(0)]
        public byte X { get; set; }

        [WZMember(1)]
        public byte Y { get; set; }
    }

    [WZContract]
    public class CPointAdd : IGameMessage
    {
        [WZMember(0)]
        public PointAdd Type { get; set; }
    }

    [WZContract]
    public class CClinetClose : IGameMessage
    {
        [WZMember(0)]
        public ClientCloseType Type { get; set; }
    }

    [WZContract]
    public class CMoveItem : IGameMessage
    {
        [WZMember(0)]
        public MoveItemFlags sFlag { get; set; }

        [WZMember(1)]
        public byte Source { get; set; }

        [WZMember(2,12)]
        public byte[] ItemInfo { get; set; }

        [WZMember(3)]
        public MoveItemFlags tFlag { get; set; }

        [WZMember(4)]
        public byte Dest { get; set; }
    }

    [WZContract]
    public class CUseItem : IGameMessage
    {
        [WZMember(0)]
        public byte Source { get; set; }

        [WZMember(1)]
        public byte Dest { get; set; }

        [WZMember(2)]
        public byte Type { get; set; }
    }

    [WZContract]
    public class CEventEnterCount : IGameMessage
    {
        [WZMember(0)]
        public EventEnterType Type { get; set; }
    }
}
