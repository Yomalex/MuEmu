using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebZen.Handlers;
using WebZen.Network;

namespace MuEmu.Network.ConnectServer
{
    public class CSClient : WZClient
    {
        public ushort Index { get; }
        private WZPacketDecoderSimple _decoder;
        private WZPacketEncoder _encoder;
        private MessageHandler[] _handler;
        private TcpClient _client;
        private byte[] _buffer;
        private WZServer _GameServer;

        public CSClient(IPEndPoint ip, MessageHandler[] handlers, MessageFactory[] factories, ushort index, WZServer server) : base(null, null, null)
        {
            _client = new TcpClient();
            _client.Connect(ip);

            _buffer = new byte[1024];
            _handler = handlers;

            _decoder = new WZPacketDecoderSimple(factories);
            _encoder = new WZPacketEncoder(factories);

            _client.Client.BeginReceive(_buffer, 0, 1024, SocketFlags.None, ReceiveCallback, this);

            var thread = new Thread(Worker);
            thread.Start(this);

            Index = index;

            SendAsync(new CRegistryReq { Index = index, Address = server.IPAddress.ToString(), Port = server.Port });

            _GameServer = server;
        }

        private static void Worker(object param)
        {
            CSClient instance = param as CSClient;

            while (true)
            {
                Thread.Sleep(10000);

                instance.SendAsync(new CKeepAlive { Index = instance.Index, Load = (byte)instance._GameServer.Load });
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            CSClient instance = ar.AsyncState as CSClient;
            var messages = new List<object>();

            try
            {
                var btRecived = instance._client.Client.EndReceive(ar);

                if (btRecived == 0)
                {
                    instance._client.Client.Disconnect(false);
                    return;
                }

                using (var mem = new MemoryStream(instance._buffer, 0, btRecived))
                {
                    int readed = 0;
                    do
                    {
                        short serial;
                        readed += instance._decoder.Decode(mem, out serial, messages);

                    } while (readed < mem.Length);
                }

                foreach (var message in messages)
                {
                    foreach (var handler in instance._handler)
                    {
                        handler.OnMessageReceived(instance, message);
                    }
                }
            }
            catch(Exception)
            {
                instance._client.Client.Disconnect(false);
            }
        }

        public void SendAsync(object message)
        {
            _client.Client.Send(_encoder.Encode(message, ref _outSerial));
        }
    }
}
