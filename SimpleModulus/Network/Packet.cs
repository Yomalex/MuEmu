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
using System.Net.Http.Headers;

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
                        throw new Exception("Invalid Packet " + type.ToString("X2") + " size " + posPacket.Length + " -");
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
                        throw new Exception("Invalid Packet " + type.ToString("X2") + " size " + posPacket.Length + " -");
                    }

                    var tmph = Serializer.Deserialize<WZWPacket>(posPacket);
                    opCode = tmph.Operation;
                    pkSize = tmph.Size;
                }

                //posPacket.Seek(0, SeekOrigin.Begin);

                var factory = _factories.FirstOrDefault(f => f.ContainsOpCode(opCode));
                try
                {
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
                            Logger.Error("Invalid OpCoder {opCodea:X4}|{opCode:X2}", orgOpCode, opCode & 0xff);
                        }
                    }
                }catch(Exception ex)
                {
                    throw new Exception("Factory: " + factory, ex);
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

        public static void PacketPrint(MemoryStream mem)
        {
            var l = mem.Length;
            var b = mem.ToArray();
            var s = "";

            foreach(var i in b)
            {
                s += i.ToString("X2");
            }

            Logger.Debug("Packet: {0}", s);
        }

        public byte[] Encode(object message, ref short serial)
        {
            MessageFactory factory = null;
            try
            {
                factory = _factories.First(f => f.ContainsType(message.GetType()));
            }catch(Exception ex)
            {
                throw new Exception("Unregisted message " + message.GetType().ToString(), ex);
            }
            
            ushort opCode = factory.GetOpCode(message.GetType());

            var att = message
                .GetType()
                .GetCustomAttributes(false)
                .Where(x => x.GetType() == typeof(WZContractAttribute))
                .FirstOrDefault() as WZContractAttribute;

            if (att == null)
                throw new InvalidOperationException("Invalid message format");

            byte[] res;

            using (var data = new MemoryStream())
            {
                var opCodeSize = (opCode & 0xFF00) == 0xFF00 ? 1 : 2;
                if (att.LongMessage)
                {
                    Serializer.Serialize(data, new WZWPacket(0xC2, (ushort)data.Length, opCode));
                }
                else
                {
                    Serializer.Serialize(data, new WZBPacket(0xC1, (byte)data.Length, opCode));
                }

                try
                {
                    data.Position = (att.LongMessage ? 3 : 2) + opCodeSize;
                    Serializer.Serialize(data, message);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "");
                }

                if (att.Serialized)
                {
                    data.Position = (att.LongMessage ? 3 : 2);
                    if (_packetRijndael == true)
                    {
                        PacketEncrypt.Encrypt(data, data);
                        //PacketPrint(data);
                    }
                    else
                        SimpleModulus.Encrypt(data, (byte)serial, data);

                    serial++;
                    data.Position = 0;
                    data.WriteByte((byte)(att.LongMessage ? 0xC4 : 0xC3));
                }

                data.Position = 1;
                if (att.LongMessage)
                {
                    data.Write(BitConverter.GetBytes(((ushort)data.Length).ShufleEnding()), 0, 2);
                }
                else
                {
                    data.Write(BitConverter.GetBytes((byte)data.Length), 0, 1);
                }
                res = data.ToArray();

                if(att.Serialized)
                    Logger.Debug("Packet Len:{0}", res.Length);

                //if(!att.Serialized)
                    return res;
            }
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

            var att = message
                .GetType()
                .GetCustomAttributes(false)
                .Where(x => x.GetType() == typeof(WZContractAttribute))
                .FirstOrDefault() as WZContractAttribute;

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
        public WZBPacket() { }
        public WZBPacket(byte type, byte size, ushort op)
        {
            Type = type;
            Size = size;
            Operation = op;
        }
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

        public WZWPacket() { }
        public WZWPacket(byte type, ushort size, ushort op)
        {
            Type = type;
            Size = size;
            Operation = op;
        }
    }
}
