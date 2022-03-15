using MU.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public abstract class VPCreateAbs
    {
        [WZMember(0)]
        public ushortle Number { get; set; }

        [WZMember(2)]
        public byte X { get; set; }

        [WZMember(3)]
        public byte Y { get; set; }

        [WZMember(4, 18)]
        public byte[] CharSet { get; set; } //18
                                            //public ulong ViewSkillState;

        [WZMember(5, typeof(BinaryStringSerializer), 10)]
        public string Name { get; set; } //10

        [WZMember(6)]
        public byte TX { get; set; }

        [WZMember(7)]
        public byte TY { get; set; }

        [WZMember(8)]
        public byte DirAndPkLevel { get; set; }
       
        public Point Position
        {
            get => new Point(X, Y);
            set
            {
                X = (byte)value.X;
                Y = (byte)value.Y;
            }
        }

        public Point TPosition
        {
            get => new Point(TX, TY);
            set
            {
                TX = (byte)value.X;
                TY = (byte)value.Y;
            }
        }

        public object Player { get; set; }
    }

    [WZContract]
    public class VPCreateDto : VPCreateAbs
    {
        [WZMember(9, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public byte[] ViewSkillState { get; set; }

        public VPCreateDto()
        {
            CharSet = Array.Empty<byte>();
            //Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<byte>();
        }

    }

    [WZContract]
    public class VPCreateS9Dto : VPCreateAbs
    {
        [WZMember(9)] public byte PentagramMainAttribute { get; set; }
        [WZMember(10)] public ushort wzMuunItem { get; set; }
        [WZMember(11)] public ushort wzMuunSubItem { get; set; }
        [WZMember(12)] public ushort wzMuunRideItem { get; set; }
        [WZMember(13)] public ushort wzLevel { get; set; }
        [WZMember(14)] public uint wzMaxLife { get; set; }
        [WZMember(15)] public uint wzCurLife { get; set; }
        [WZMember(16)] public ushort ServerCodeOfHomeWorld { get; set; }
        [WZMember(17, SerializerType = typeof(ArrayWithScalarSerializer<byte>))] public byte[] ViewSkillState { get; set; }

        public VPCreateS9Dto()
        {
            CharSet = Array.Empty<byte>();
            //Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<byte>();
        }

        public ushort MuunItem { get => wzMuunItem.ShufleEnding(); set => wzMuunItem = value.ShufleEnding(); }
        public ushort MuunSubItem { get => wzMuunSubItem.ShufleEnding(); set => wzMuunSubItem = value.ShufleEnding(); }
        public ushort MuunRideItem { get => wzMuunRideItem.ShufleEnding(); set => wzMuunRideItem = value.ShufleEnding(); }
        public ushort Level { get => wzLevel.ShufleEnding(); set => wzLevel = value.ShufleEnding(); }
        public uint MaxLife { get => wzMaxLife.ShufleEnding(); set => wzMaxLife = value.ShufleEnding(); }
        public uint CurLife { get => wzCurLife.ShufleEnding(); set => wzCurLife = value.ShufleEnding(); }
    }

    [WZContract]
    public class VPCreateS12Dto : VPCreateAbs
    {
        [WZMember(9)] public byte PentagramMainAttribute { get; set; }
        [WZMember(10)] public ushortle MuunItem { get; set; }
        [WZMember(11)] public byte fill1 { get; set; }
        [WZMember(12)] public ushortle MuunSubItem { get; set; }
        [WZMember(13)] public byte fill2 { get; set; }
        [WZMember(14)] public ushortle MuunRideItem { get; set; }
        [WZMember(15)] public byte DarkSpirit { get; set; }
        [WZMember(16)] public ushortle Level { get; set; }
        [WZMember(17)] public uint wzMaxLife { get; set; }
        [WZMember(18)] public uint wzCurLife { get; set; }
        [WZMember(19)] public ushort ServerCodeOfHomeWorld { get; set; }
        [WZMember(20, SerializerType = typeof(ArrayWithScalarSerializer<ushort>))] public SkillStates[] ViewSkillState { get; set; }

        public VPCreateS12Dto()
        {
            CharSet = Array.Empty<byte>();
            //Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<SkillStates>();
        }

        public uint MaxLife { get => wzMaxLife.ShufleEnding(); set => wzMaxLife = value.ShufleEnding(); }
        public uint CurLife { get => wzCurLife.ShufleEnding(); set => wzCurLife = value.ShufleEnding(); }
    }
}
