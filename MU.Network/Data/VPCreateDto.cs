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
        public ushort wzNumber { get; set; }

        [WZMember(2)]
        public byte X { get; set; }

        [WZMember(3)]
        public byte Y { get; set; }

        [WZMember(4, 18)]
        public byte[] CharSet { get; set; } //18
                                            //public ulong ViewSkillState;

        [WZMember(5, 10)]
        public byte[] Id { get; set; } //10

        [WZMember(6)]
        public byte TX { get; set; }

        [WZMember(7)]
        public byte TY { get; set; }

        [WZMember(8)]
        public byte DirAndPkLevel { get; set; }


        public ushort Number
        {
            get
            {
                return wzNumber.ShufleEnding();
            }

            set
            {
                wzNumber = value.ShufleEnding();
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
        public object Player { get; set; }
    }

    [WZContract]
    public class VPCreateDto : VPCreateAbs
    {
        [WZMember(9, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public SkillStates[] ViewSkillState { get; set; }

        public VPCreateDto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<SkillStates>();
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
        [WZMember(17, SerializerType = typeof(ArrayWithScalarSerializer<byte>))] public SkillStates[] ViewSkillState { get; set; }

        public VPCreateS9Dto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<SkillStates>();
        }

        public ushort MuunItem { get => wzMuunItem.ShufleEnding(); set => wzMuunItem = value.ShufleEnding(); }
        public ushort MuunSubItem { get => wzMuunSubItem.ShufleEnding(); set => wzMuunSubItem = value.ShufleEnding(); }
        public ushort MuunRideItem { get => wzMuunRideItem.ShufleEnding(); set => wzMuunRideItem = value.ShufleEnding(); }
        public ushort Level { get => wzLevel.ShufleEnding(); set => wzLevel = value.ShufleEnding(); }
        public uint MaxLife { get => wzMaxLife.ShufleEnding(); set => wzMaxLife = value.ShufleEnding(); }
        public uint CurLife { get => wzCurLife.ShufleEnding(); set => wzCurLife = value.ShufleEnding(); }
    }
}
