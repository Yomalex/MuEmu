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
    public abstract class VPChangeAbs
    {
        [WZMember(0)]
        public byte NumberH { get; set; }

        [WZMember(1)]
        public byte NumberL { get; set; }

        [WZMember(2)]
        public byte X { get; set; }

        [WZMember(3)]
        public byte Y { get; set; }

        [WZMember(4)]
        public ushortle Skin { get; set; }

        [WZMember(6, 10)]
        public byte[] Id { get; set; } //10

        [WZMember(7)]
        public byte TX { get; set; }

        [WZMember(8)]
        public byte TY { get; set; }

        [WZMember(9)]
        public byte DirAndPkLevel { get; set; }
        //public ulong ViewSkillState;

        [WZMember(10, 18)]
        public byte[] CharSet { get; set; } //18

        public int Number
        {
            get => (NumberH << 8) | NumberL;
            set
            {
                NumberH = (byte)(value >> 8);
                NumberL = (byte)(value & 0xFF);
            }
        }

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

        public string Name
        {
            get => Id.MakeString();
            set => Id = value.GetBytes();
        }

    }

    [WZContract]
    public class VPChangeDto : VPChangeAbs
    {
        [WZMember(11, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        //public byte SkillStateCount { get; set; }
        public byte[] ViewSkillState { get; set; } //Num_ViewSkillState

        public VPChangeDto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<byte>();
        }
    }

    [WZContract]
    public class VPChangeS9Dto : VPChangeAbs
    {
        [WZMember(11)] public byte PentagramMainAttribute { get; set; }
        [WZMember(12)] public ushort wzMuunItem { get; set; }
        [WZMember(13)] public ushort wzMuunSubItem { get; set; }
        [WZMember(14)] public ushort wzMuunRideItem { get; set; }
        [WZMember(15)] public ushort wzLevel { get; set; }
        [WZMember(16)] public uint wzMaxLife { get; set; }
        [WZMember(17)] public uint wzCurLife { get; set; }
        [WZMember(18)] public ushort ServerCodeOfHomeWorld { get; set; }

        [WZMember(19, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public byte[] ViewSkillState { get; set; }

        public VPChangeS9Dto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
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
    public class VPChangeS12Dto : VPChangeAbs
    {
        [WZMember(11)] public byte PentagramMainAttribute { get; set; }
        [WZMember(12)] public ushortle MuunItem { get; set; }
        [WZMember(13)] public ushortle MuunSubItem { get; set; }
        [WZMember(14)] public ushortle MuunRideItem { get; set; }
        [WZMember(15)] public ushortle Level { get; set; }
        [WZMember(16)] public uint wzMaxLife { get; set; }
        [WZMember(17)] public uint wzCurLife { get; set; }
        [WZMember(18)] public ushort ServerCodeOfHomeWorld { get; set; }

        [WZMember(19, SerializerType = typeof(ArrayWithScalarSerializer<ushort>))]
        public SkillStates[] ViewSkillState { get; set; }

        public VPChangeS12Dto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<SkillStates>();
        }
        public uint MaxLife { get => wzMaxLife.ShufleEnding(); set => wzMaxLife = value.ShufleEnding(); }
        public uint CurLife { get => wzCurLife.ShufleEnding(); set => wzCurLife = value.ShufleEnding(); }
    }
}
