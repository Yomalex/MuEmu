using MU.DataBase;
using MuEmu.Network.Data;
using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Quests
    {
        public Player Player { get; set; }
        private List<Quest> _quests;
        public Quests(Character @char, CharacterDto characterDto)
        {
            _quests = new List<Quest>();
            Player = @char.Player;
        }

        public async void SendList()
        {
            var standarQuest = _quests.Where(x => x.Standar).ToArray();
            await Player.Session.SendAsync(new SQuestInfo { Count = (byte)standarQuest.Length, State = Array.Empty<byte>() });
            var customQuest = _quests.Where(x => !x.Standar).ToArray();
            await Player.Session.SendAsync(new SNewQuestInfo { QuestList = Array.Empty<NewQuestInfoDto>() });
        }
    }

    public class Quest
    {
        public bool Standar { get; set; }
    }
}
