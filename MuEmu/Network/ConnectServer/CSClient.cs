using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CSClient));
        private WZPacketDecoderSimple _decoder;
        private WZPacketEncoder _encoder;
        private MessageHandler[] _handler;
        private TcpClient _client;
        private byte[] _buffer;
        private string _token;

        public CSClient(IPEndPoint ip, MessageHandler[] handlers, MessageFactory[] factories, ushort index, WZServer server, byte show, string token) : base(null, null, null)
        {
            _client = new TcpClient();
            _client.Connect(ip);
            _sock = _client.Client;

            _buffer = new byte[1024];
            _handler = handlers;

            _decoder = new WZPacketDecoderSimple(factories);
            _encoder = new WZPacketEncoder(factories,false);

            _client.Client.BeginReceive(_buffer, 0, 1024, SocketFlags.None, ReceiveCallback, this);

            Index = index;
            _server = server;

            var thread = new Thread(Worker);
            thread.Start(this);

            _token = token;
            SendAsync(new CRegistryReq { Index = index, Address = server.IPAddress.ToString(), Port = server.Port, Show = show, Token = token });
        }

        private static void Worker(object param)
        {
            CSClient instance = param as CSClient;

            while (true)
            {
                Thread.Sleep(10000);

                instance.SendAsync(new CKeepAlive { Index = instance.Index, Load = (byte)instance._server.Load, Token = instance._token });
            }
        }

        private static async void ReceiveCallback(IAsyncResult ar)
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
                //Logger.Information($"Receive " + btRecived + " Buffer:" + string.Join("", instance._buffer.Select(x => x.ToString("X2"))));
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
                        await handler.OnMessageReceived(instance, message);
                    }
                }

                instance._client.Client
                    .BeginReceive(instance._buffer, 0, 1024, SocketFlags.None, ReceiveCallback, instance);
            }
            catch(Exception)
            {
                instance._client.Client.Disconnect(false);
            }
        }

        public void SendAsync(object message)
        {
            try
            {
                _client.Client.Send(_encoder.Encode(message, ref _outSerial));
            }
            catch (SocketException)
            {
                _client.Client.Disconnect(false);
            }
            catch(Exception)
            { }
        }
    }
}
