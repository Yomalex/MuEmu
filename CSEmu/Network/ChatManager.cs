using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSEmu.Network
{
    internal class RoomMember
    {

    }
    internal class Room
    {
        private Dictionary<CSSession, RoomMember> _room = new Dictionary<CSSession, RoomMember>();
        public int Index { get; private set; }
        public Room(int id)
        {
            Index = id;
        }

        public void Join(CSSession session, string member)
        {
            _room.Add(session, new RoomMember() { });
        }
    }

    internal class ChatManager
    {
        private Dictionary<string, Room> _rooms;
        private static ChatManager _instance;

        public static void Initialize()
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new ChatManager();
        }

        public static Room CreateRoom()
        {
            var id = (_instance._rooms.Count() + (int)(DateTime.Now.ToBinary() & 0xFFFFFFFF));
            var hash = id.ToString("X");

            var r = new Room(id);
            _instance._rooms.Add(hash, r);
            return r;
        }

        public static Room GetRoom(int rid, string auth)
        {
            Room result;
            if (!_instance._rooms.TryGetValue(auth, out result))
                return null;

            if(rid == result.Index)
                return result;

            return null;
        }
    }
}
