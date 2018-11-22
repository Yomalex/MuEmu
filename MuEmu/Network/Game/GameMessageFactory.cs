using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.Game
{
    public interface IGameMessage
    { }

    public class GameMessageFactory : MessageFactory<GameOpCode, IGameMessage>
    {
        public GameMessageFactory()
        {
            // C2S
            Register<CCheckSum>(GameOpCode.GameSecurity);
            Register<CClientMessage>(GameOpCode.ClientMessage);
            Register<CCloseWindow>(GameOpCode.CloseWindow);

            // S2C
            Register<SInventory>(GameOpCode.Inventory);
            Register<SEquipament>(GameOpCode.Equipament);
            Register<SCheckSum>(GameOpCode.GameSecurity);
            Register<SWeather>(GameOpCode.Weather);
            Register<SSpells>(GameOpCode.Spells);
            Register<SQuestInfo>(GameOpCode.QuestInfo);
            Register<SFriends>(GameOpCode.FriendList);
            Register<SKillCount>(GameOpCode.KillCount);
            Register<SViewPortCreate>(GameOpCode.ViewPortCreate);
            Register<SViewPortChange>(GameOpCode.ViewPortChange);
            Register<SViewPortMonCreate>(GameOpCode.ViewPortMCreate);
            Register<SNotice>(GameOpCode.Notice);
            Register<SEventState>(GameOpCode.EventState);
            Register<SNewQuestInfo>(GameOpCode.NewQuestInfo);
        }
    }
}
