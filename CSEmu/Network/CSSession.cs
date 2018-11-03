using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using WebZen.Network;

namespace CSEmu.Network
{
    public class CSSession : WZClient
    {
        public CSSession(WZServer server, Socket socket, AsyncCallback onRecv)
            : base(server, socket, onRecv)
        { }

        public void SendAsync(object message)
        {
            Send(_server.Encode(message, ref _outSerial));
        }
    }

    public class CSSessionFactory : ISessionFactory
    {
        public WZClient Create(WZServer server, Socket socket, AsyncCallback onRecv)
        {
            return new CSSession(server, socket, onRecv);
        }
    }
}
