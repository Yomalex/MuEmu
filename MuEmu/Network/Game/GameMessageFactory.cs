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
            Register<CAction>(GameOpCode.Rotation);
            Register<CMove>(GameOpCode.Move);
            Register<CChatNickname>(GameOpCode.GeneralChat0);
            Register<CNewQuestInfo>(GameOpCode.QuestDetails);
            Register<CPositionSet>(GameOpCode.Position);
            Register<CPointAdd>(GameOpCode.PointAdd);
            Register<CClientClose>(GameOpCode.ClientClose);
            Register<CMoveItem>(GameOpCode.MoveItem);
            Register<CUseItem>(GameOpCode.HealthUpdate); // Same OPCode
            Register<CEventEnterCount>(GameOpCode.EventEnterCount);
            Register<CTalk>(GameOpCode.Talk);
            Register<CWarehouseUseEnd>(GameOpCode.WarehouseUseEnd);
            Register<CBuy>(GameOpCode.Buy);
            Register<CSell>(GameOpCode.Sell);
            //Register<CAttackS5E2>(GameOpCode.Attack);
            Register<CAttack>(GameOpCode.Attack);
            Register<CWarp>(GameOpCode.Warp);
            Register<CDataLoadOK>(GameOpCode.DataLoadOK);
            Register<CJewelMix>(GameOpCode.JewelMix);
            Register<CJewelUnMix>(GameOpCode.JewelUnMix);
            Register<CChaosBoxItemMixButtonClick>(GameOpCode.ChaosBoxItemMixButtonClick);
            Register<CChaosBoxUseEnd>(GameOpCode.ChaosBoxUseEnd);
            Register<CItemThrow>(GameOpCode.ItemThrow);
            Register<CItemGet>(GameOpCode.ItemGet);
            Register<CMagicAttack>(GameOpCode.MagicAttack);

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
            Register<SViewPortDestroy>(GameOpCode.ViewPortDestroy);
            Register<SViewPortItemDestroy>(GameOpCode.ViewPortItemDestroy);
            Register<SNotice>(GameOpCode.Notice);
            Register<SEventState>(GameOpCode.EventState);
            Register<SNewQuestInfo>(GameOpCode.NewQuestInfo);
            Register<SHeatlUpdate>(GameOpCode.HealthUpdate);
            Register<SManaUpdate>(GameOpCode.ManaUpdate);
            Register<SSkillKey>(GameOpCode.SkillKey);
            Register<SAction>(GameOpCode.Rotation);
            Register<SMove>(GameOpCode.Move);
            Register<SPositionSet>(GameOpCode.PositionSet);
            Register<SPointAdd>(GameOpCode.PointAdd);
            Register<SLevelUp>(GameOpCode.LevelUp);
            Register<SClinetClose>(GameOpCode.ClientClose);
            Register<SMoveItem>(GameOpCode.MoveItem);
            Register<SEventEnterCount>(GameOpCode.EventEnterCount);
            Register<SCloseMsg>(GameOpCode.ClientClose);
            Register<STalk>(GameOpCode.Talk);
            Register<SShopItemList>(GameOpCode.CloseWindow); // Same OPCode
            Register<STax>(GameOpCode.Tax);
            Register<SWarehouseMoney>(GameOpCode.WarehouseMoney);
            Register<SQuestWindow>(GameOpCode.QuestWindow);
            Register<SBuy>(GameOpCode.Buy);
            Register<SSell>(GameOpCode.Sell);
            Register<SItemGet>(GameOpCode.ItemGet);
            Register<SChatTarget>(GameOpCode.GeneralChat1);
            Register<STeleport>(GameOpCode.Teleport);
            Register<SViewSkillState>(GameOpCode.ViewSkillState);
            Register<SInventoryItemDelete>(GameOpCode.InventoryItemDelete);
            Register<SJewelMix>(GameOpCode.JewelMix);
            Register<SCommand>(GameOpCode.Command);
            Register<SSetMapAttribute>(GameOpCode.SetMapAtt);
            Register<SItemThrow>(GameOpCode.ItemThrow);
            Register<SViewPortItemCreate>(GameOpCode.ViewPortItemCreate);
            Register<SInventoryItemSend>(GameOpCode.OneItemSend);
            Register<SInventoryItemDurSend>(GameOpCode.InventoryItemDurUpdate);
            Register<SChaosBoxItemMixButtonClick>(GameOpCode.ChaosBoxItemMixButtonClick);
            Register<SDamage>(GameOpCode.Damage);
            Register<SKillPlayer>(GameOpCode.KillPlayer);
            Register<SDiePlayer>(GameOpCode.DiePlayer);
            Register<SAttackResult>(GameOpCode.Attack);
            Register<SMagicAttack>(GameOpCode.MagicAttack);
            Register<SMagicDuration>(GameOpCode.MagicDuration);
        }
    }
}
