using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using WebZen.Network;

namespace MuEmu.Network
{
    public class GSSession : WZClient
    {
        public Player Player { get; set; }
        public ushort PreviousCode { get; set; }
        public byte[] Key => _rijndael.Key;
        public byte TryLoginCount { get; set; }

        public GSSession(WZServer server, Socket socket, AsyncCallback onRecv)
            : base(server, socket, onRecv)
        { }

        public async Task SendAsync(object message)
        {
            if (Closed)
                return;

            await Send(_server.Encode(message, ref _outSerial, this));
        }
    }

    public class GSSessionFactory : ISessionFactory
    {
        public WZClient Create(WZServer server, Socket socket, AsyncCallback onRecv)
        {
            return new GSSession(server, socket, onRecv);
        }
    }
}
