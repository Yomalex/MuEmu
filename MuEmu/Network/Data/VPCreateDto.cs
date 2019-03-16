using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class VPCreateDto
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
        //public ulong ViewSkillState;

        [WZMember(9, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        //public byte SkillStateCount { get; set; }
        public SkillStates[] ViewSkillState { get; set; } //Num_ViewSkillState

        public VPCreateDto()
        {
            CharSet = Array.Empty<byte>();
            Id = Array.Empty<byte>();
            ViewSkillState = Array.Empty<SkillStates>();
        }

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

        public Player Player { get; set; }
    }
}
