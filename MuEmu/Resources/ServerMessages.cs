using MU.Resources;
using MuEmu.Resources.XML;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources
{
    public class ServerMessages
    {
        private static ServerMessages s_instance;
        private Dictionary<Messages, string> _messages = new Dictionary<Messages, string>();

        public static void Initialize()
        {
            s_instance = new ServerMessages();
        }
        public static void LoadMessages(string file)
        {
            var xml = ResourceLoader.XmlLoader<ServerMessagesDto>(file);

            s_instance._messages.Clear();
            foreach(var row in xml.Messages)
            {
                s_instance._messages.Add(Enum.Parse<Messages>(row.ID), row.Message);
            }
        }
        public static string GetMessage(Messages message, params object[] values)
        {
            if (!s_instance._messages.ContainsKey(message))
                return "{"+message+"}";

            var res = s_instance._messages[message];

            for (var i = 0; i < values.Length; i++)
            {
                res = res.Replace("{"+i+"}", values[i].ToString());
            }

            return res;
        }
    }
}
