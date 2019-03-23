using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public ushort Port => (ushort)((IPEndPoint)_listener.LocalEndpoint).Port;

        public int MaxUsers { get; set; }

        public float Load => _clients.Count / MaxUsers * 100.0f;

        public bool SimpleStream { get; set; }

        public byte[] Encode(object message, ref short serial) => _encoder.Encode(message, ref serial);
        
        public WZServer()
        {
            _clients = new Dictionary<int, WZClient>();
            MaxUsers = 300;
        }

        public void Initialize(IPEndPoint address, MessageHandler[] handler, ISessionFactory factory, MessageFactory[] message)
        {
            _listener = new TcpListener(address);
            _decoder = new WZPacketDecoder(message);
            _decoderSimple = new WZPacketDecoderSimple(message);
            _encoder = new WZPacketEncoder(message);
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
                TotalRecv = @in.Length;
                type = @in[0];
                using (var received = new MemoryStream(2048))
                {
                    received.Write(@in, 0, (int)TotalRecv);
                    received.Seek(0, SeekOrigin.Begin);
                    int readed = 0;

                    if (received.Length == 0)
                    {
                        sender.Disconnect();
                        return;
                    }

                    received.SetLength(1024);

                    do
                    {
                        if (SimpleStream)
                        {
                            readed += _decoderSimple.Decode(received, out serial, messages);
                        }
                        else
                        {
                            readed += _decoder.Decode(received, out serial, messages);
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
                    Console.WriteLine("[C->S] " + message.GetType());
                    foreach (var handler in _handler)
                    {
                        await handler.OnMessageReceived(sender, message);
                    }
                }
            }catch(Exception e)
            {
                Logger.Error(e, $"packet decode pSize:{TotalRecv} pType:{type}");
                sender.Disconnect();
                return;
            }
        }

        private int FreeIndex()
        {
            for(var i = 0; i < 1000; i++)
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

        public byte[] Data => _recvBuffer;
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
        }

        public void Recv()
        {
            _sock.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, _onRecv, this);
        }

        public async void Send(byte[] data)
        {
            try
            {
                await _sock.SendAsync(data, SocketFlags.None);//, OnSend, this
            }catch(Exception)
            {

            }
        }

        //private void OnSend(IAsyncResult result)
        //{

        //}

        public bool IsValidSerial(short serial)
        {
            var valid = serial == _inSerial;
            _inSerial++;
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
    }
}
