using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace WebZen.Network
{
    public interface ISessionFactory
    {
        WZClient Create(WZServer server, Socket socket, AsyncCallback onRecv);
    }
}
