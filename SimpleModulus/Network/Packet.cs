using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using BlubLib.Serialization;
using WebZen.Handlers;
using WebZen.Serialization;
using Serilog;
using Serilog.Core;

namespace WebZen.Network
{
    public class WZPacketDecoder
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZPacketDecoder));
        private readonly MessageFactory[] _factories;

        public WZPacketDecoder(MessageFactory[] factories)
        {
            _factories = factories;
        }

        public int Decode(MemoryStream rawPacket, out short serial, List<object> messages)
        {
            using (var tempMS = new MemoryStream())
            {
                //var type = rawPacket[0];
                MemoryStream decPacket = rawPacket;
                var type = rawPacket.ReadByte();
                ushort size = 0;
                serial = -1;

                if (type == 0xC1 || type == 0xC3)
                    size = (ushort)rawPacket.ReadByte();
                else if (type == 0xC2 || type == 0xC4)
                    size = (ushort)((rawPacket.ReadByte() << 8) | rawPacket.ReadByte());
                else
                    throw new InvalidProgramException($"Invalid packet type {type}");

                // Packet Encrypted
                if (type == 0xC3)
                {
                    decPacket = tempMS;
                    var temp = new byte[size - 2];

                    //Array.Copy(rawPacket, 2, temp, 0, temp.Length);
                    rawPacket.Read(temp, 0, temp.Length);

                    var dec = SimpleModulus.Decoder(temp);

                    serial = dec[0];
                    decPacket.WriteByte(0xC1);
                    decPacket.WriteByte((byte)(dec.Length + 1));
                    decPacket.Write(dec, 1, dec.Length - 1);

                    //Array.Copy(dec, 1, rawPacket, 2, dec.Length - 1);

                    //rawPacket[0] = 0xC1;
                    //rawPacket[1] = (byte)(dec.Length + 1);
                }
                else if (type == 0xC4)
                {
                    decPacket = tempMS;
                    var temp = new byte[size - 3];

                    //Array.Copy(rawPacket, 3, temp, 0, temp.Length);
                    rawPacket.Read(temp, 0, temp.Length);

                    var dec = SimpleModulus.Decoder(temp);

                    serial = dec[0];
                    decPacket.WriteByte(0xC2);
                    decPacket.WriteByte((byte)((dec.Length + 2) >> 8));
                    decPacket.WriteByte((byte)((dec.Length + 2) & 255));
                    decPacket.Write(dec, 1, dec.Length - 1);

                    //Array.Copy(dec, 1, rawPacket, 3, dec.Length - 1);

                    //rawPacket[0] = 0xC2;
                    //rawPacket[1] = (byte)((dec.Length + 2) >> 8);
                    //rawPacket[2] = (byte)((dec.Length + 2) & 255);
                }

                using (var spe = new StreamPacketEngine())
                {
                    spe.AddData(decPacket.ToArray());
                    decPacket.Dispose();
                    decPacket = new MemoryStream(spe.ExtractPacket());
                }

                ushort opCode;
                if (type == 0xC1 || type == 0xC3)
                    opCode = Serializer.Deserialize<WZBPacket>(decPacket).Operation;
                else
                    opCode = Serializer.Deserialize<WZWPacket>(decPacket).Operation;

                var factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                if (factory != null)
                {
                    messages.Add(factory.GetMessage(opCode, decPacket));
                }
                else
                {
                    Logger.Error("Invalid OpCoder {opCode}", opCode);
                }

                return size;
            }
        }
    }

    public class WZPacketEncoder
    {
        private readonly MessageFactory[] _factories;

        public WZPacketEncoder(MessageFactory[] factories)
        {
            _factories = factories;
        }

        public byte[] Encode(object message, ref short serial)
        {
            var factory = _factories.FirstOrDefault(f => f.ContainsType(message.GetType()));
            byte[] result = null;

            if (factory == null)
                throw new InvalidProgramException($"Invalid message type {message.GetType()}");

            ushort opCode = factory.GetOpCode(message.GetType());

            WZContractAttribute att = null;

            foreach (var a in message.GetType().GetCustomAttributes(false))
            {
                if (a.GetType() == typeof(WZContractAttribute))
                {
                    att = a as WZContractAttribute;
                }
            }

            if (att == null)
                throw new InvalidOperationException("Invalid message format");

            using (var h = new MemoryStream())
            using (var b = new MemoryStream())
            {
                Serializer.Serialize(b, message);
                var body = b.ToArray();
                var length = body.Length;
                
                if (att.LongMessage)
                {
                    var header = new WZWPacket
                    {
                        Type = (byte)0xC2,
                        Size = (ushort)(length+5),
                        Operation = opCode
                    };

                    if(att.Serialized)
                    {
                        var temp = new byte[header.Size - 2];

                        temp[0] = (byte)serial;

                        Array.Copy(body, 0, temp, 1, temp.Length - 1);

                        body = SimpleModulus.Encoder(temp);
                        header.Type = 0xC4;

                        serial++;
                    }

                    Serializer.Serialize(h, header);
                }
                else
                {
                    var header = new WZBPacket
                    {
                        Type = (byte)0xC1,
                        Size = (byte)(length+4),
                        Operation = opCode
                    };

                    if (att.Serialized)
                    {
                        var temp = new byte[header.Size - 1];

                        temp[0] = (byte)serial;

                        Array.Copy(body, 0, temp, 1, temp.Length - 1);

                        body = SimpleModulus.Encoder(temp);
                        header.Type = 0xC4;

                        serial++;
                    }

                    Serializer.Serialize(h, header);
                }

                var head = h.ToArray();

                result = new byte[head.Length+body.Length];
                Array.Copy(head, result, head.Length);
                Array.Copy(body, 0, result, head.Length, body.Length);
            }

            return result;
        }
    }

    public class WZPacket
    {
        public short Serial { get; set; }

        public object message { get; set; }
    }

    [BlubContract]
    public class WZBPacket
    {
        [BlubMember(0)]
        public byte Type { get; set; }

        [BlubMember(1)]
        public byte Size { get; set; }

        [BlubMember(2)]
        public ushort Operation { get; set; }
    }

    [BlubContract]
    public class WZWPacket
    {
        [BlubMember(0)]
        public byte Type { get; set; }

        [BlubMember(1)]
        public ushort Size { get; set; }

        [BlubMember(2)]
        public ushort Operation { get; set; }
    }
}
