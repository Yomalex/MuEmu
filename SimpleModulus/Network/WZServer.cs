using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace WebZen.Network
{
    public class WZServer
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WZServer));
        protected Dictionary<int, WZClient> _clients;

        private TcpListener _listener;
        private WZPacketDecoder _decoder;
        private WZPacketDecoderSimple _decoderSimple;
        private ISessionFactory _factory;
        private MessageHandler[] _handler;
        private WZPacketEncoder _encoder;

        public IPAddress IPAddress => ((IPEndPoint)_listener.LocalEndpoint).Address;
        public string IPPublic { get; set; }

        public ushort Port => (ushort)((IPEndPoint)_listener.LocalEndpoint).Port;

        public int MaxUsers { get; set; }

        public float Load => _clients.Count / MaxUsers * 100.0f;

        public bool SimpleStream { get; set; }

        public byte[] Encode(object message, ref short serial, WZClient client) => _encoder.Encode(message, ref serial, client);
        
        public WZServer()
        {
            _clients = new Dictionary<int, WZClient>();
            MaxUsers = 300;
        }

        public void Initialize(IPEndPoint address, MessageHandler[] handler, ISessionFactory factory, MessageFactory[] message, bool useRijndael)
        {
            _listener = new TcpListener(address);
            _decoder = new WZPacketDecoder(message, useRijndael);
            _decoderSimple = new WZPacketDecoderSimple(message);
            _encoder = new WZPacketEncoder(message, useRijndael);
            _listener.Start();
            _factory = factory;
            _handler = handler;

            Task.Run(AcceptClients);

            Logger.Information("Started on TCP-IP:{ip} TCP-PORT:{port}", address.Address, address.Port);
        }

        protected virtual void OnConnect(WZClient session)
        { }

        public virtual void OnDisconnect(WZClient session)
        {
            Logger.Information($"Client at Index {session.ID} closed");
            var element = _clients.FirstOrDefault(c => c.Value == session);
            _clients.Remove(element.Key);
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                var client = _factory.Create(this, await _listener.AcceptSocketAsync(), OnRecv);
                client.ID = FreeIndex();
                if(client.ID == -1)
                {
                    Logger.Error("All index are in use");
                    client.Disconnect();
                    continue;
                }
                _clients.Add(client.ID, client);
                Logger.Information($"New Client added at index {client.ID}");
                OnConnect(client);
                client.Recv();
            }
        }

        private async void OnRecv(IAsyncResult result)
        {
            var sender = (WZClient)result.AsyncState;
            var messages = new List<object>();
            short serial = 0;
            long TotalRecv = 0;
            byte type = 0x00;

            try
            {
                var @in = sender.Received(result);
                if (@in.Length == 0)
                {
                    sender.Disconnect();
                    return;
                }

                TotalRecv = @in.Length;
                type = @in[0];
                using (var received = new MemoryStream(2048))
                {
                    received.Write(@in, 0, (int)TotalRecv);
                    received.Seek(0, SeekOrigin.Begin);
                    int readed = 0;
                    received.SetLength(1024);
                    do
                    {
                        if (SimpleStream)
                        {
                            readed += _decoderSimple.Decode(received, out serial, messages);
                        }
                        else
                        {
                            readed += _decoder.Decode(received, out serial, messages, sender);
                        }

                        if (serial != -1 && !sender.IsValidSerial(serial))
                        {
                            sender.Disconnect();
                            Logger.Error($"Serialized packet with invalid serial {serial}");
                            return;
                        }
                    } while (readed < TotalRecv);
                }

                sender.Recv();

                foreach (var message in messages)
                {
                    //Console.WriteLine("[C->S] " + message.GetType());
                    foreach (var handler in _handler)
                    {
                        var handled = await handler.OnMessageReceived(sender, message);
                        if(!handled)
                        {
                            Logger.Warning("Un handled message {0}", message.GetType());
                        }
                    }
                }
            }catch(Exception e)
            {
                Logger.Error(e, $"packet decode pSize:{TotalRecv} pType:{type:X}");
                sender.Disconnect();
                return;
            }
        }

        private int FreeIndex()
        {
            for(var i = 1; i < 1000; i++)
            {
                if (_clients.Keys.Any(k => k == i))
                    continue;

                return i;
            }

            return -1;
        }
    }

    public class WZClient
    {
        public int ID { get; set; }
        protected WZServer _server;
        protected byte[] _recvBuffer = new byte[2048];
        protected Socket _sock;
        protected AsyncCallback _onRecv;
        protected short _inSerial;
        public short _outSerial;

        private static RandomNumberGenerator _keyGenerator;
        protected Rijndael _rijndael;

        private bool _closed;
        public bool Closed { get => _closed | !(_sock?.Connected??false); private set => _closed = value; }

        public byte[] Data => _recvBuffer;

        public int BlockSize => _rijndael.BlockSize;

        public byte[] Received(IAsyncResult ar)
        {
            int recv = 0;
            try
            {
                recv = _sock.EndReceive(ar);
            }catch(SocketException)
            {
                _sock.Close();
                return Array.Empty<byte>();
            }catch(ObjectDisposedException)
            {
                return Array.Empty<byte>();
            }

            if (recv <= 0)
                return Array.Empty<byte>();

            var packet = new byte[recv];
            Array.Copy(_recvBuffer, packet, recv);
            Array.Fill<byte>(_recvBuffer, 0);

            return packet;
        }

        public WZClient(WZServer server, Socket socket, AsyncCallback onRecv)
        {
            _server = server;
            _sock = socket;
            _onRecv = onRecv;
            Closed = false;

            if (_keyGenerator == null)
                _keyGenerator = RandomNumberGenerator.Create();

            _rijndael = Rijndael.Create();
            var key = new byte[32];
            _keyGenerator.GetBytes(key);
            _rijndael.Key = key;
            _rijndael.Mode = CipherMode.ECB;
            _rijndael.Padding = PaddingMode.None;
        }

        public void Recv()
        {
            _sock.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, _onRecv, this);
        }

        public async Task<int> Send(byte[] data)
        {
            int result = 0;
            try
            {
                result = await _sock.SendAsync(data, SocketFlags.None);//, OnSend, this
            }catch(Exception)
            {
                Closed = true;
            }
            return result;
        }

        //private void OnSend(IAsyncResult result)
        //{

        //}

        public bool IsValidSerial(short serial)
        {
            var valid = serial == _inSerial;
            _inSerial++;
            if (_inSerial > 255)
                _inSerial = 0;
            return valid;
        }

        public void Disconnect()
        {
            _server.OnDisconnect(this);
            try
            {
                _sock.Disconnect(true);
            }catch(Exception)
            {

            }
        }

        internal ICryptoTransform CreateEncryptor()
        {
            return _rijndael.CreateEncryptor();
        }

        internal ICryptoTransform CreateDecryptor()
        {
            return _rijndael.CreateDecryptor();
        }
    }
}
