using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    [WZContract]
    public class VPMCreateDto
    {
        [WZMember(0)]
        public byte NumberH { get; set; }

        [WZMember(1)]
        public byte NumberL { get; set; }

        [WZMember(2)]
        public byte TypeH { get; set; }

        [WZMember(3)]
        public byte TypeL { get; set; }

        [WZMember(4)]
        public byte X { get; set; }

        [WZMember(5)]
        public byte Y { get; set; }

        [WZMember(6)]
        public byte TX { get; set; }

        [WZMember(7)]
        public byte TY { get; set; }

        [WZMember(8)]
        public byte Path { get; set; }

        [WZMember(9, typeof(ArrayWithScalarSerializer<byte>))]
        //public byte SkillStateCount { get; set; }
        public byte[] ViewSkillState { get; set; } //Num_ViewSkillState

        public VPMCreateDto()
        {
            ViewSkillState = Array.Empty<byte>();
        }

        public int Number
        {
            get => (NumberH << 8) | NumberL;
            set
            {
                NumberH = (byte)(value >> 8);
                NumberL = (byte)(value & 0xFF);
            }
        }

        public int Type
        {
            get => (TypeH << 8) | TypeL;
            set
            {
                TypeH = (byte)(value >> 8);
                TypeL = (byte)(value & 0xFF);
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
    }
}
