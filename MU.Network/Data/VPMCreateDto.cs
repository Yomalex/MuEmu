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
    public abstract class VPMCreateAbs
    {
        [WZMember(0)]
        public ushortle Number { get; set; }

        [WZMember(2)]
        public ushortle Type { get; set; }

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

    [WZContract]
    public class VPMCreateDto : VPMCreateAbs
    {
        [WZMember(9, typeof(ArrayWithScalarSerializer<byte>))]
        public byte[] ViewSkillState { get; set; }

        public VPMCreateDto()
        {
            ViewSkillState = Array.Empty<byte>();
        }
    }

    [WZContract]
    public class VPMCreateS9Dto : VPMCreateAbs
    {
        [WZMember(9)]
        public Element PentagramMainAttribute { get; set; }
        [WZMember(10)]
        public ushort wzLevel { get; set; }
        [WZMember(11)]
        public uint wzMaxLife { get; set; }
        [WZMember(12)]
        public uint wzLife { get; set; }

        [WZMember(13, typeof(ArrayWithScalarSerializer<byte>))]
        public byte[] ViewSkillState { get; set; }

        public VPMCreateS9Dto()
        {
            ViewSkillState = Array.Empty<byte>();
        }

        public ushort Level
        {
            get
            {
                return wzLevel.ShufleEnding();
            }

            set
            {
                wzLevel = value.ShufleEnding();
            }
        }

        public uint MaxLife
        {
            get
            {
                return wzMaxLife.ShufleEnding();
            }

            set
            {
                wzMaxLife = value.ShufleEnding();
            }
        }

        public uint Life
        {
            get
            {
                return wzLife.ShufleEnding();
            }

            set
            {
                wzLife = value.ShufleEnding();
            }
        }
    }

    [WZContract]
    public class VPMCreateS12Dto : VPMCreateAbs
    {
        [WZMember(9)]
        public Element PentagramMainAttribute { get; set; }
        [WZMember(10)]
        public ushortle Level { get; set; }
        [WZMember(11)]
        public uint wzMaxLife { get; set; }
        [WZMember(12)]
        public uint wzLife { get; set; }

        [WZMember(13, typeof(ArraySerializer))]
        public byte[] Test { get; set; }

        /*[WZMember(13, typeof(ArrayWithScalarSerializer<uint>))]
        public SkillStates[] ViewSkillState { get; set; }*/
        /*[WZMember(14)]
        public ushort padding { get; set; }*/


        public VPMCreateS12Dto()
        {
            //ViewSkillState = Array.Empty<SkillStates>();
            Test = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 };
        }

        public uint MaxLife
        {
            get
            {
                return wzMaxLife.ShufleEnding();
            }

            set
            {
                wzMaxLife = value.ShufleEnding();
            }
        }

        public uint Life
        {
            get
            {
                return wzLife.ShufleEnding();
            }

            set
            {
                wzLife = value.ShufleEnding();
            }
        }
    }
}
