using CSEmu.Network.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebZen.Util;

namespace CSEmu
{
    public class ClientManager
    {
        private Dictionary<int, List<SCAdd>> _serverClientList = new Dictionary<int, List<SCAdd>>();

        public void AddServer(int ServerCode)
        {
            _serverClientList.Add(ServerCode, new List<SCAdd>());
        }
        public void AddClient(SCAdd client)
        {
            if (!_serverClientList.ContainsKey(client.Server))
                throw new Exception("Invalid Server Code " + client.Server);

            ServerManager.Instance.BroadCast(client);
            _serverClientList[client.Server].Add(client);
        }
        public void RemClient(SCRem message)
        { 
            if (!_serverClientList.ContainsKey(message.Server))
                return;

            var name = message.List.First().btName.MakeString();

            var client = _serverClientList[message.Server].First(x => x.btName.MakeString() == name);

            ServerManager.Instance.BroadCast(message);
            _serverClientList[message.Server].Remove(client);
        }
        public void RemServer(byte ServerCode)
        {
            if (!_serverClientList.ContainsKey(ServerCode))
                return;

            ServerManager.Instance.BroadCast(new SCRem
            {
                Server = ServerCode,
                List = _serverClientList[ServerCode]
                    .Select(x => new CliRemDto { btName = x.btName })
                    .ToArray()
            });

            _serverClientList[ServerCode].Clear();
            _serverClientList.Remove(ServerCode);
        }
    }
}
