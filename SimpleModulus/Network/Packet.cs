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
using WebZen.Util;

namespace WebZen.Network
{
    public class WZPacketDecoder
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZPacketDecoder));
        private readonly MessageFactory[] _factories;
        private bool _packetRijndael;

        public WZPacketDecoder(MessageFactory[] factories, bool useRijndael)
        {
            _factories = factories;
            _packetRijndael = useRijndael;
        }

        public int Decode(MemoryStream rawPacket, out short serial, List<object> messages)
        {
            using (var decPacket = new MemoryStream())
            using (var posPacket = new MemoryStream())
            {
                //var type = rawPacket[0];
                var type = (byte)rawPacket.ReadByte();
                ushort size = 0;
                serial = -1;

                if (type == 0xC1 || type == 0xC3)
                    size = (ushort)rawPacket.ReadByte();
                else if (type == 0xC2 || type == 0xC4)
                    size = (ushort)((rawPacket.ReadByte() << 8) | rawPacket.ReadByte());
                else
                    throw new InvalidProgramException($"Invalid packet type {type:X2}");

                // Packet Encrypted
                byte[] tmp;
                byte[] dec;
                switch(type)
                {
                    case 0xC1:
                        decPacket.WriteByte(type);
                        decPacket.WriteByte((byte)size);
                        tmp = new byte[size - 2];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        decPacket.Write(tmp, 0, tmp.Length);
                        break;
                    case 0xC2:
                        decPacket.WriteByte(type);
                        decPacket.WriteByte((byte)((size >> 8) & 0xff));
                        decPacket.WriteByte((byte)(size & 0xff));
                        tmp = new byte[size - 3];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        decPacket.Write(tmp, 0, tmp.Length);
                        break;
                    case 0xC3:
                        tmp = new byte[size - 2];
                        rawPacket.Read(tmp, 0, tmp.Length);

                        if(_packetRijndael)
                        {
                            dec = PacketEncrypt.Decrypt(tmp);
                            if (dec.Length == 0)
                                return size;

                            decPacket.WriteByte(0xC1);
                            decPacket.WriteByte((byte)(dec.Length + 2));
                            decPacket.Write(dec, 0, dec.Length);
                        }else
                        {
                            dec = SimpleModulus.Decoder(tmp);
                            serial = dec[0];
                            decPacket.WriteByte(0xC1);
                            decPacket.WriteByte((byte)(dec.Length + 1));
                            decPacket.Write(dec, 1, dec.Length - 1);
                        }
                        break;
                    case 0xC4:
                        tmp = new byte[size - 3];
                        rawPacket.Read(tmp, 0, tmp.Length);

                        if (_packetRijndael)
                        {
                            dec = PacketEncrypt.Decrypt(tmp);
                            if (dec.Length == 0)
                                return size;

                            decPacket.WriteByte(0xC2);
                            decPacket.WriteByte((byte)((dec.Length + 3) >> 8));
                            decPacket.WriteByte((byte)((dec.Length + 3) & 255));
                            decPacket.Write(dec, 0, dec.Length);
                        }
                        else
                        {
                            dec = SimpleModulus.Decoder(tmp);
                            serial = dec[0];
                            decPacket.WriteByte(0xC2);
                            decPacket.WriteByte((byte)((dec.Length + 2) >> 8));
                            decPacket.WriteByte((byte)((dec.Length + 2) & 255));
                            decPacket.Write(dec, 1, dec.Length - 1);
                        }
                        break;
                }
                
                using (var spe = new StreamPacketEngine(_packetRijndael))
                {
                    spe.AddData(decPacket.ToArray());
                    var posProcess = spe.ExtractPacket();
                    posPacket.Write(posProcess, 0, posProcess.Length);
                }

                posPacket.Seek(0, SeekOrigin.Begin);

                ushort opCode;
                ushort pkSize;

                if (type == 0xC1 || type == 0xC3)
                {
                    if (posPacket.Length == 3)
                    {
                        posPacket.Seek(0, SeekOrigin.End);
                        posPacket.WriteByte(0);
                        posPacket.Seek(0, SeekOrigin.Begin);
                    }
                    else if (posPacket.Length < 3)
                    {
                        throw new Exception("Invalid Packet " + type + " size " + posPacket.Length + " -");
                    }

                    var tmph = Serializer.Deserialize<WZBPacket>(posPacket);
                    opCode = tmph.Operation;
                    pkSize = tmph.Size;
                }
                else
                {
                    if (posPacket.Length == 4)
                    {
                        posPacket.Seek(0, SeekOrigin.End);
                        posPacket.WriteByte(0);
                        posPacket.Seek(0, SeekOrigin.Begin);
                    }
                    else if(posPacket.Length < 4)
                    {
                        throw new Exception("Invalid Packet " + type + " size " + posPacket.Length + " -");
                    }

                    var tmph = Serializer.Deserialize<WZWPacket>(posPacket);
                    opCode = tmph.Operation;
                    pkSize = tmph.Size;
                }

                //posPacket.Seek(0, SeekOrigin.Begin);

                var factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                if (factory != null)
                {
                    messages.Add(factory.GetMessage(opCode, posPacket));
                }
                else
                {
                    var orgOpCode = opCode;
                    opCode |= 0xFF00;
                    factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                    if (factory != null)
                    {
                        posPacket.Position--;
                        messages.Add(factory.GetMessage(opCode, posPacket));
                    }
                    else
                    {
                        Logger.Error("Invalid OpCoder {opCodea:X4}|{opCode:X4}", orgOpCode, opCode);
                    }
                }

                return size;
            }
        }
    }

    public class WZPacketDecoderSimple
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZPacketDecoder));
        private readonly MessageFactory[] _factories;

        public WZPacketDecoderSimple(MessageFactory[] factories)
        {
            _factories = factories;
        }

        public int Decode(MemoryStream rawPacket, out short serial, List<object> messages)
        {
            using (var decPacket = new MemoryStream())
            using (var posPacket = new MemoryStream())
            {
                //var type = rawPacket[0];
                var type = (byte)rawPacket.ReadByte();
                ushort size = 0;
                serial = -1;

                if (type == 0xC1 || type == 0xC3)
                    size = (ushort)rawPacket.ReadByte();
                else if (type == 0xC2 || type == 0xC4)
                    size = (ushort)((rawPacket.ReadByte() << 8) | rawPacket.ReadByte());
                else
                    throw new InvalidProgramException($"Invalid packet type {type:X2}");

                // Packet Encrypted
                byte[] tmp;
                byte[] dec;
                switch (type)
                {
                    case 0xC1:
                        decPacket.WriteByte(type);
                        decPacket.WriteByte((byte)size);
                        tmp = new byte[size - 2];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        decPacket.Write(tmp, 0, tmp.Length);
                        break;
                    case 0xC2:
                        decPacket.WriteByte(type);
                        decPacket.WriteByte((byte)((size >> 8) & 0xff));
                        decPacket.WriteByte((byte)(size & 0xff));
                        tmp = new byte[size - 3];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        decPacket.Write(tmp, 0, tmp.Length);
                        break;
                    case 0xC3:
                        tmp = new byte[size - 2];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        dec = SimpleModulus.Decoder(tmp);
                        serial = dec[0];
                        decPacket.WriteByte(0xC1);
                        decPacket.WriteByte((byte)(dec.Length + 1));
                        decPacket.Write(dec, 1, dec.Length - 1);
                        break;
                    case 0xC4:
                        tmp = new byte[size - 3];
                        rawPacket.Read(tmp, 0, tmp.Length);
                        dec = SimpleModulus.Decoder(tmp);
                        serial = dec[0];
                        decPacket.WriteByte(0xC2);
                        decPacket.WriteByte((byte)((dec.Length + 2) >> 8));
                        decPacket.WriteByte((byte)((dec.Length + 2) & 255));
                        decPacket.Write(dec, 1, dec.Length - 1);
                        break;
                }

                //using (var spe = new StreamPacketEngine())
                //{
                //    spe.AddData(decPacket.ToArray());
                //    var posProcess = spe.ExtractPacket();
                //    posPacket.Write(posProcess, 0, posProcess.Length);
                //}

                var posProcess = decPacket.ToArray();
                posPacket.Write(posProcess, 0, posProcess.Length);
                posPacket.Seek(0, SeekOrigin.Begin);

                ushort opCode;
                ushort pkSize;

                if (posPacket.Length < 3)
                {
                    //Logger.Error("Invalid Size {0}", posPacket.Length);
                    throw new Exception("Invalid Packet size " + posPacket.Length);
                }

                if (type == 0xC1 || type == 0xC3)
                {
                    var tmph = Serializer.Deserialize<WZBPacket>(posPacket);
                    opCode = tmph.Operation;
                    pkSize = tmph.Size;
                }
                else
                {
                    var tmph = Serializer.Deserialize<WZWPacket>(posPacket);
                    opCode = tmph.Operation;
                    pkSize = tmph.Size;
                }

                //posPacket.Seek(0, SeekOrigin.Begin);

                var factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                if (factory != null)
                {
                    messages.Add(factory.GetMessage(opCode, posPacket));
                }
                else
                {
                    var orgOpCode = opCode;
                    opCode |= 0xFF00;
                    factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                    if (factory != null)
                    {
                        posPacket.Position--;
                        messages.Add(factory.GetMessage(opCode, posPacket));
                    }
                    else
                    {
                        Logger.Error("Invalid OpCoder {opCodea:X4}|{opCode:X4}", orgOpCode, opCode);
                    }
                }

                return size;
            }
        }
    }

    public class WZPacketEncoder
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZPacketEncoder));

        private readonly MessageFactory[] _factories;
        private bool _packetRijndael;

        public WZPacketEncoder(MessageFactory[] factories, bool useRijndael)
        {
            _factories = factories;
            _packetRijndael = useRijndael;
        }

        public byte[] Encode(object message, ref short serial)
        {

            var factory = _factories.First(f => f.ContainsType(message.GetType()));
            byte[] result = null;
            
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

            //Console.WriteLine("[S->C] {0} {1} {2}", message.GetType().Name, att.Serialized, serial);

            using (var h = new MemoryStream())
            using (var b = new MemoryStream())
            {
                try
                {
                    Serializer.Serialize(b, message);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                var body = b.ToArray();
                var length = body.Length;
                var sizeFix = (((opCode & 0xFF00) == 0xFF00) ? 1 : 0);
                var sizeFix2 = 0;

                if (att.LongMessage)
                {
                    var header = new WZWPacket
                    {
                        Type = (byte)0xC2,
                        Size = (ushort)(length + 5 - sizeFix),
                        Operation = opCode
                    };
                    Serializer.Serialize(h, header);

                    sizeFix2 = 2;
                }
                else
                {
                    var header = new WZBPacket
                    {
                        Type = (byte)0xC1,
                        Size = (byte)(length + 4 - sizeFix),
                        Operation = opCode
                    };
                    Serializer.Serialize(h, header);

                    sizeFix2 = 1;
                }

                var head = h.ToArray();
                var headLen = head.Length - sizeFix;
                result = new byte[headLen + body.Length];
                Array.Copy(head, result, headLen);
                Array.Copy(body, 0, result, headLen, body.Length);

                if (att.Serialized)
                {
                    result[0] += 2;

                    var temp = new byte[result.Length - sizeFix2];
                    Array.Copy(result, sizeFix2, temp, 0, temp.Length);

                    temp[0] = (byte)serial;

                    byte[] enc;// = SimpleModulus.Encoder(temp);
                    using (var ms = new MemoryStream())
                    {
                        if (_packetRijndael == true)
                            PacketEncrypt.Encrypt(ms, new MemoryStream(temp));
                        else
                            SimpleModulus.Encrypt(ms, new MemoryStream(temp));

                        enc = new byte[ms.Length];
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Read(enc, 0, (int)ms.Length);
                    }

                    var resultTemp = new byte[sizeFix2 + enc.Length + 1];
                    Array.Copy(result, resultTemp, sizeFix2 + 1);
                    if(resultTemp[0] == 0xC3)
                    {
                        resultTemp[1] = (byte)(resultTemp.Length);
                    }else
                    {
                        resultTemp[1] = (byte)(resultTemp.Length >>8);
                        resultTemp[2] = (byte)(resultTemp.Length &0xff);
                    }
                    Array.Copy(enc, 0, resultTemp, sizeFix2 + 1, enc.Length);
                    serial++;

                    return resultTemp;
                }
            }

            return result;
        }
    }

    public class WZPacketEncoderClient
    {
        private readonly MessageFactory[] _factories;

        public WZPacketEncoderClient(MessageFactory[] factories)
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
                var sizeFix = (((opCode & 0xFF00) == 0xFF00) ? 1 : 0);

                if (att.LongMessage)
                {
                    var header = new WZWPacket
                    {
                        Type = (byte)0xC2,
                        Size = (ushort)(length + 5 - sizeFix),
                        Operation = opCode
                    };

                    if (att.Serialized)
                    {
                        var temp = new byte[header.Size - 2];

                        temp[0] = (byte)serial;

                        Array.Copy(body, 0, temp, 1, body.Length);

                        body = SimpleModulus.Encoder(temp);
                        header.Type += 2;

                        serial++;
                    }

                    Serializer.Serialize(h, header);
                }
                else
                {
                    var header = new WZBPacket
                    {
                        Type = (byte)0xC1,
                        Size = (byte)(length + 4 - sizeFix),
                        Operation = opCode
                    };

                    if (att.Serialized)
                    {
                        var temp = new byte[header.Size - 1];

                        temp[0] = (byte)serial;

                        Array.Copy(body, 0, temp, 1, body.Length);

                        body = SimpleModulus.Encoder(temp);
                        header.Type += 2;

                        serial++;
                    }

                    Serializer.Serialize(h, header);
                }

                var head = h.ToArray();
                var headLen = head.Length - sizeFix;

                result = new byte[headLen + body.Length];
                Array.Copy(head, result, headLen);
                Array.Copy(body, 0, result, headLen, body.Length);

                using (var spe = new StreamPacketEngine(false))
                {
                    spe.AddData(result);
                    result = spe.ExtractData();
                }
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
        public ushort wzSize { get; set; }

        [BlubMember(3)]
        public ushort Operation { get; set; }

        public ushort Size {
            get => wzSize.ShufleEnding();
            set
            {
                wzSize = value.ShufleEnding();
            }
        }
    }
}
