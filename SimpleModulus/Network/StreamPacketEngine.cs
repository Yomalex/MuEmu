using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebZen.Network
{
    public class StreamPacketEngine : IDisposable
    {
        private MemoryStream _stream = new MemoryStream();
        private readonly byte[] XORFilter;

        public StreamPacketEngine(bool _rijndael)
        {
            if (_rijndael)
            {
                XORFilter = new byte[]
                {
                0xAB, 0x11, 0xCD, 0xFE, 0x18, 0x23, 0xC5, 0xA3, 0xCA, 0x33, 0xC1, 0xCC, 0x66,
                0x67, 0x21, 0xF3, 0x32, 0x12, 0x15, 0x35, 0x29, 0xFF, 0xFE, 0x1D, 0x44, 0xEF, 
                0xCD, 0x41, 0x26, 0x3C, 0x4E, 0x4D
                };
            }
            else
            {
                XORFilter = new byte[]
                {
                0xE7, 0x6D, 0x3A, 0x89, 0xBC, 0xB2, 0x9F, 0x73, 0x23, 0xA8, 0xFE, 0xB6, 0x49,
                0x5D, 0x39, 0x5D, 0x8A, 0xCB, 0x63, 0x8D, 0xEA, 0x7D, 0x2B, 0x5F, 0xC3, 0xB1,
                0xE9, 0x83, 0x29, 0x51, 0xE8, 0x56
                };
            }
        }

        public bool AddData(byte[] data)
        {
            _stream.Write(data, 0, data.Length);

            return true;
        }

        public byte[] ExtractPacket()
        {
            _stream.Seek(0, SeekOrigin.Begin);

            var type = _stream.ReadByte();
            ushort size = 0;
            switch(type)
            {
                case 0xC1:
                    size = (ushort)_stream.ReadByte();
                    break;
                case 0xC2:
                    size = (ushort)((_stream.ReadByte() << 8) | _stream.ReadByte());
                    break;
                default:
                    throw new Exception($"Invalid packet type 0x{type:X2}");
            }

            return XorData(size, type == 0xC1 ? 2 : 3);
        }

        public byte[] ExtractData()
        {
            _stream.Seek(0, SeekOrigin.Begin);

            var type = _stream.ReadByte();
            ushort size = 0;
            switch (type)
            {
                case 0xC1:
                    size = (ushort)_stream.ReadByte();
                    break;
                case 0xC2:
                    size = (ushort)((_stream.ReadByte() << 8) | _stream.ReadByte());
                    break;
                default:
                    throw new Exception($"Invalid packet type 0x{type:X2}");
            }

            return IXorData(size, type == 0xC1 ? 2 : 3);
        }

        private byte[] XorData(int Size, int Skip)
        {
            var packet = new byte[Size];
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Read(packet, 0, Size);
            var tmpstream = new MemoryStream();
            tmpstream.Write(_stream.ToArray(), Size, (int)(_stream.Length - Size));
            _stream.Dispose();
            _stream = tmpstream;

            for (int i = Size - 1; i != Skip; i--)
            {
                packet[i] ^= (byte)(packet[i - 1] ^ XORFilter[(i & (XORFilter.Length - 1))]);
            }

            return packet;
        }

        private byte[] IXorData(int Size, int Skip)
        {
            var packet = new byte[Size];
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Read(packet, 0, Size);
            var tmpstream = new MemoryStream();
            tmpstream.Write(_stream.ToArray(), Size, (int)(_stream.Length - Size));
            _stream.Dispose();
            _stream = tmpstream;

            for (int i = Skip; i < Size - 1; i++)
            {
                packet[i+1] ^= (byte)(packet[i] ^ XORFilter[(i+1 & (XORFilter.Length - 1))]);
            }

            return packet;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
