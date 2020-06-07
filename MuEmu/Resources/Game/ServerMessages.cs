using Google.Protobuf.WellKnownTypes;
using MuEmu.Resources.XML;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources.Game
{
    public enum Messages
    {
        Server_Cfg,
        Server_Title,
        Server_MySQL_Error,
        Server_Error,
        Server_CSServer_Error,
        Server_Disconnecting_Accounts,
        Server_Ready,
        Server_GlobalAnnouncement,
        Server_MapAnnouncement,
        Server_NoEventMapAnnouncement,
        Server_Close,
        RCache_Initialized,
        RCache_Loading_Items,
        RCache_Loading_Spells,
        RCache_Loading_Maps,
        RCache_Loading_DefClass,
        RCache_Loading_Shops,
        RCache_Loading_NPCs,
        RCache_Loading_JoHs,
        RCache_Loading_Gates,
        RCache_Loading_Quests,
        RCache_Loading_ChaosMixs,
        MonsterMng_Loading,
        MonsterMng_Loading2,
        MonsterMng_Types,
        MonsterMng_Loaded,
        BC_Closed,
        BC_Open,
        BC_DoorKiller,
        BC_StatueKiller,
        DS_Closed,
        Chat_Player_Offline,
        Game_Close,
        Game_Close_Message,
        Game_Vault_active,
        Game_NoQuestAvailable,
        Game_DefaultNPCMessage,
        Game_Warp,
        Game_ChaosBoxMixError,
    }
    public class ServerMessages
    {
        private Dictionary<Messages, string> _messages = new Dictionary<Messages, string>();

        public void LoadMessages(string file)
        {
            var xml = ResourceLoader.XmlLoader<ServerMessagesDto>(file);

            _messages.Clear();
            foreach(var row in xml.Messages)
            {
                _messages.Add(System.Enum.Parse<Messages>(row.ID), row.Message);
            }
        }
        public string GetMessage(Messages message, params object[] values)
        {
            if (!_messages.ContainsKey(message))
                return "{"+message+"}";

            var res = _messages[message];

            for (var i = 0; i < values.Length; i++)
            {
                res = res.Replace("{"+i+"}", values[i].ToString());
            }

            return res;
        }
    }
}
