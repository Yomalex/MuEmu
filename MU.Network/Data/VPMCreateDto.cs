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
        public ushortle Level { get; set; }
        [WZMember(11, typeof(ArraySerializer))]
        public byte[] wzMaxLife { get; set; }
        [WZMember(12, typeof(ArraySerializer))]
        public byte[] wzLife { get; set; }
        public uint MaxLife
        {
            get
            {
                uint result = 0;
                result |= (uint)wzMaxLife[0] << 24;//HB-HW
                result |= (uint)wzMaxLife[1] << 8;
                result |= (uint)wzMaxLife[2] << 16;
                result |= (uint)wzMaxLife[3];
                return result;
            }

            set
            {
                wzMaxLife = new byte[4];
                wzMaxLife[0] = (byte)(value >> 24);
                wzMaxLife[1] = (byte)(value >> 8);
                wzMaxLife[2] = (byte)(value >> 16);
                wzMaxLife[3] = (byte)(value);
            }
        }

        public uint Life
        {
            get
            {
                uint result = 0;
                result |= (uint)wzLife[0] << 24;//HB-HW
                result |= (uint)wzLife[1] << 8;
                result |= (uint)wzLife[2] << 16;
                result |= (uint)wzLife[3];
                return result;
            }

            set
            {
                wzLife = new byte[4];
                wzLife[0] = (byte)(value >> 24);
                wzLife[1] = (byte)(value >> 8);
                wzLife[2] = (byte)(value >> 16);
                wzLife[3] = (byte)(value);
            }
        }

        [WZMember(13, typeof(ArrayWithScalarSerializer<byte>))]
        public byte[] ViewSkillState { get; set; }

        public VPMCreateS9Dto()
        {
            ViewSkillState = Array.Empty<byte>();
        }        
    }

    [WZContract]
    public class VPMCreateS12Dto : VPMCreateAbs
    {
        [WZMember(9)]
        public Element PentagramMainAttribute { get; set; }
        [WZMember(10)]
        public ushortle Level { get; set; }
        [WZMember(11, typeof(ArraySerializer))]
        public byte[] wzMaxLife { get; set; }
        [WZMember(12, typeof(ArraySerializer))]
        public byte[] wzLife { get; set; }

        [WZMember(14, typeof(ArrayWithScalarSerializer<byte>))]
        public SkillStates[] ViewSkillState { get; set; }
        /*[WZMember(14)]
        public ushort padding { get; set; }*/


        public VPMCreateS12Dto()
        {
            ViewSkillState = new SkillStates[32];
        }

        public uint MaxLife
        {
            get
            {
                uint result = 0;
                result |= (uint)wzMaxLife[0] << 24;//HB-HW
                result |= (uint)wzMaxLife[1] << 8;
                result |= (uint)wzMaxLife[2] << 16;
                result |= (uint)wzMaxLife[3];
                return result;
            }

            set
            {
                wzMaxLife = new byte[4];
                wzMaxLife[0] = (byte)(value >> 24);
                wzMaxLife[1] = (byte)(value >> 8);
                wzMaxLife[2] = (byte)(value >> 16);
                wzMaxLife[3] = (byte)(value);
            }
        }

        public uint Life
        {
            get
            {
                uint result = 0;
                result |= (uint)wzLife[0] << 24;//HB-HW
                result |= (uint)wzLife[1] << 8;
                result |= (uint)wzLife[2] << 16;
                result |= (uint)wzLife[3];
                return result;
            }

            set
            {
                wzLife = new byte[4];
                wzLife[0] = (byte)(value >> 24);
                wzLife[1] = (byte)(value >> 8);
                wzLife[2] = (byte)(value >> 16);
                wzLife[3] = (byte)(value);
            }
        }
    }

    [WZContract]
    public class VPMCreateS16KorDto : VPMCreateAbs
    {
        [WZMember(9)] public Element PentagramMainAttribute { get; set; }
        [WZMember(10)] public byte CriticalDMGResistance { get; set; }
        [WZMember(11)] public byte ExcellentDMGResistance { get; set; }
        [WZMember(12)] public ushortle DebuffResistance { get; set; } = 0;
        [WZMember(13)] public byte DamageAbsorb { get; set; }
        [WZMember(14)] public byte Elite { get; set; } //??
        [WZMember(15)] public ushortle Level { get; set; }
        [WZMember(16, typeof(ArraySerializer))] public byte[] wzMaxLife { get; set; }
        [WZMember(17, typeof(ArraySerializer))] public byte[] wzLife { get; set; }

        [WZMember(18, typeof(ArrayWithScalarSerializer<ushort>))]
        public SkillStates[] ViewSkillState { get; set; }
        [WZMember(18)]  public byte unk64 { get; set; }

        //public ushort EffectCount { get => (ushort)ViewSkillState.Length; set => _len = value; }
        //private ushort _len;

        public VPMCreateS16KorDto()
        {
            //ViewSkillState = new SkillStates[32];
        }

        public uint MaxLife
        {
            get
            {
                uint result = 0;
                result |= (uint)wzMaxLife[0] << 24;//HB-HW
                result |= (uint)wzMaxLife[1] << 8;
                result |= (uint)wzMaxLife[2] << 16;
                result |= (uint)wzMaxLife[3];
                return result;
            }

            set
            {
                wzMaxLife = new byte[4];
                wzMaxLife[0] = (byte)(value >> 24);
                wzMaxLife[1] = (byte)(value >> 8);
                wzMaxLife[2] = (byte)(value >> 16);
                wzMaxLife[3] = (byte)(value);
            }
        }

        public uint Life
        {
            get
            {
                uint result = 0;
                result |= (uint)wzLife[0] << 24;//HB-HW
                result |= (uint)wzLife[1] << 8;
                result |= (uint)wzLife[2] << 16;
                result |= (uint)wzLife[3];
                return result;
            }

            set
            {
                wzLife = new byte[4];
                wzLife[0] = (byte)(value >> 24);
                wzLife[1] = (byte)(value >> 8);
                wzLife[2] = (byte)(value >> 16);
                wzLife[3] = (byte)(value);
            }
        }
    }
}
