using MuEmu.Network.Event;
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
            Register<CPositionSet>(GameOpCode.Position);

            #region Client ChatMessages
            Register<CChatNickname>(GameOpCode.GeneralChat0);
            Register<CChatNumber>(GameOpCode.GeneralChat1);
            Register<CChatWhisper>(GameOpCode.WhisperChat);
            #endregion

            Register<CTeleport>(GameOpCode.Teleport);
            Register<CInventory>(GameOpCode.Inventory);
            Register<CNewQuestInfo>(GameOpCode.QuestDetails);
            Register<CPointAdd>(GameOpCode.PointAdd);
            Register<CClientClose>(GameOpCode.ClientClose);
            Register<CMoveItem>(GameOpCode.MoveItem);
            Register<CUseItem>(GameOpCode.HealthUpdate); // Same OPCode
            Register<CEventEnterCount>(GameOpCode.EventEnterCount);
            Register<CTalk>(GameOpCode.Talk);
            Register<CWarehouseUseEnd>(GameOpCode.WarehouseUseEnd);
            Register<CBuy>(GameOpCode.Buy);
            Register<CSell>(GameOpCode.Sell);
            Register<CWarp>(GameOpCode.Warp);
            Register<CDataLoadOK>(GameOpCode.DataLoadOK);
            Register<CJewelMix>(GameOpCode.JewelMix);
            Register<CJewelUnMix>(GameOpCode.JewelUnMix);
            Register<CChaosBoxItemMixButtonClick>(GameOpCode.ChaosBoxItemMixButtonClick);
            Register<CChaosBoxUseEnd>(GameOpCode.ChaosBoxUseEnd);
            Register<CItemThrow>(GameOpCode.ItemThrow);
            Register<CItemGet>(GameOpCode.ItemGet);
            Register<CItemModify>(GameOpCode.ItemModify);

            #region Client PersonalShopMessages
            Register<CPShopSetItemPrice>(GameOpCode.PShopSetItemPrice);
            Register<CPShopRequestOpen>(GameOpCode.PShopRequestOpen);
            Register<CPShopRequestClose>(GameOpCode.PShopRequestClose);
            Register<CPShopRequestList>(GameOpCode.PShopRequestList);
            Register<CPShopRequestBuy>(GameOpCode.PShopRequestBuy);
            #endregion

            #region Client AttackMessages
            //Register<CAttackS5E2>(GameOpCode.Attack);
            Register<CAttack>(GameOpCode.Attack);
            Register<CMagicAttack>(GameOpCode.MagicAttack);
            Register<CMagicDuration>(GameOpCode.MagicDuration);
            #endregion

            #region Client PartyMessages
            Register<CPartyRequest>(GameOpCode.PartyRequest);
            Register<CPartyRequestResult>(GameOpCode.PartyResult);
            Register<CPartyList>(GameOpCode.PartyList);
            Register<CPartyDelUser>(GameOpCode.PartyDelUser);
            #endregion

            #region Client DuelMessages
            Register<CDuelRequest>(GameOpCode.DuelRequest);
            Register<CDuelAnswer>(GameOpCode.DuelAnswer);
            Register<CDuelLeave>(GameOpCode.DuelLeave);
            Register<CDuelJoinRoom>(GameOpCode.DuelRoomJoin);
            Register<CDuelLeaveRoom>(GameOpCode.DuelRoomLeave);
            #endregion

            #region Client FriendMessages
            Register<CFriendList>(GameOpCode.FriendList);
            Register<CFriendAdd>(GameOpCode.FriendAdd);
            #endregion

            // S2C
            Register<SInventory>(GameOpCode.Inventory);
            Register<SEquipament>(GameOpCode.Equipament);
            Register<SCheckSum>(GameOpCode.GameSecurity);
            Register<SWeather>(GameOpCode.Weather);
            Register<SSpells>(GameOpCode.Spells);
            Register<SQuestInfo>(GameOpCode.QuestInfo);
            Register<SFriends>(GameOpCode.FriendList);
            Register<SKillCount>(GameOpCode.KillCount);

            #region Server ViewPortMessages
            Register<SViewPortCreate>(GameOpCode.ViewPortCreate);
            Register<SViewPortChange>(GameOpCode.ViewPortChange);
            Register<SViewPortMonCreate>(GameOpCode.ViewPortMCreate);
            Register<SViewPortDestroy>(GameOpCode.ViewPortDestroy);
            Register<SViewPortItemDestroy>(GameOpCode.ViewPortItemDestroy);
            #endregion

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
            Register<SCharRegen>(GameOpCode.CharRegen);
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
            Register<SViewPortPShop>(GameOpCode.ViewPortPShop);
            Register<SInventoryItemSend>(GameOpCode.OneItemSend);
            Register<SInventoryItemDurSend>(GameOpCode.InventoryItemDurUpdate);
            Register<SChaosBoxItemMixButtonClick>(GameOpCode.ChaosBoxItemMixButtonClick);
            Register<SDamage>(GameOpCode.Damage);
            Register<SKillPlayer>(GameOpCode.KillPlayer);
            Register<SDiePlayer>(GameOpCode.DiePlayer);
            Register<SAttackResult>(GameOpCode.Attack);
            Register<SMagicAttack>(GameOpCode.MagicAttack);
            Register<SMagicDuration>(GameOpCode.MagicDuration);
            Register<SEffect>(GameOpCode.Effect);
            Register<SItemModify>(GameOpCode.ItemModify);
            Register<SItemUseSpecialTime>(GameOpCode.ItemUseSpecialTime);

            Register<SPShopSetItemPrice>(GameOpCode.PShopSetItemPrice);
            Register<SPShopRequestOpen>(GameOpCode.PShopRequestOpen);
            Register<SPShopRequestClose>(GameOpCode.PShopRequestClose);
            Register<SPShopRequestList>(GameOpCode.PShopRequestList);
            Register<SPShopRequestBuy>(GameOpCode.PShopRequestBuy);
            Register<SPShopRequestSold>(GameOpCode.PShopRequestSold);

            #region Server PartyMessages
            Register<SPartyResult>(GameOpCode.PartyResult);
            Register<SPartyList>(GameOpCode.PartyList);
            Register<SPartyDelUser>(GameOpCode.PartyDelUser);
            Register<SPartyLifeAll>(GameOpCode.PartyLifeUpdate);
            #endregion

            #region Server DuelMessages
            Register<SDuelAnsDuelInvite>(GameOpCode.DuelRequest);
            Register<SDuelAnswerReq>(GameOpCode.DuelAnswer);
            Register<SDuelAnsExit>(GameOpCode.DuelLeave);
            Register<SDuelBroadcastScore>(GameOpCode.DuelScoreBroadcast);
            Register<SDuelBroadcastHP>(GameOpCode.DuelHPBroadcast);
            Register<SDuelChannelList>(GameOpCode.DuelChannelList);
            Register<SDuelRoomJoin>(GameOpCode.DuelRoomJoin);
            Register<SDuelRoomBroadcastJoin>(GameOpCode.DuelRoomJoinBroadcast);
            Register<SDuelRoomLeave>(GameOpCode.DuelRoomLeave);
            Register<SDuelRoomBroadcastLeave>(GameOpCode.DuelRoomLeaveBroadcast);
            Register<SDuelRoomBroadcastObservers>(GameOpCode.DuelRoomObserversBroadcast);
            Register<SDuelBroadcastResult>(GameOpCode.DuelResultBroadcast);
            Register<SDuelBroadcastRound>(GameOpCode.DuelRoundBroadcast);
            #endregion
        }
    }
}
