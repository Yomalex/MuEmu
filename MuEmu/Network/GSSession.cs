using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network
{
    public enum LoginStatus
    {
        NotLogged,
        Logged,
        Playing
    }

    public class GSSession : WZClient
    {
        public string Account { get; set; }

        public string Nickname { get; set; }

        public LoginStatus status { get; set; }

        public GSSession(WZServer server, Socket socket, AsyncCallback onRecv)
            : base(server, socket, onRecv)
        { }

        public void SendAsync(object message)
        {
            Send(_server.Encode(message, ref _outSerial));
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
