using MU.Network.MuunSystem;
using MU.Network.Pentagrama;
using MU.Resources;
using MuEmu.Network.UBFSystem;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.Game
{
    public interface IGameMessage
    { }

    public class GameMessageFactory : MessageFactory<GameOpCode, IGameMessage>
    {
        public GameMessageFactory(ServerSeason Season)
        {
            // C2S
            Register<CCheckSum>(GameOpCode.GameSecurity);
            Register<SMapMoveCheckSum>(GameOpCode.MapMoveCheckSum);
            Register<CClientMessage>(GameOpCode.ClientMessage);
            Register<CCloseWindow>(GameOpCode.CloseWindow);
            Register<CCancelItemSale>(GameOpCode.CancelItemSale);
            Register<CCancelItemSaleClose>(GameOpCode.CancelItemSaleClose);
            Register<CCancelItemSaleItem>(GameOpCode.CancelItemSaleItem);
            Register<SCancelItemSaleResult>(GameOpCode.CancelItemSaleItem);
            Register<SCancelItemSaleListS16>(GameOpCode.CancelItemSale);

            Register<CAction>(GameOpCode.Rotation);

            Register<COpenBox>(GameOpCode.OpenBox);
            Register<SOpenBox>(GameOpCode.OpenBox);

            Register<CItemSplit>(GameOpCode.ItemSplit);
            Register<SItemSplit>(GameOpCode.ItemSplit);

            #region Client ChatMessages
            Register<CChatNickname>(GameOpCode.GeneralChat0);
            Register<CChatNumber>(GameOpCode.GeneralChat1);
            Register<CChatWhisper>(GameOpCode.WhisperChat);
            #endregion

            Register<CInventory>(GameOpCode.Inventory);
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
            Register<CItemModify>(GameOpCode.ItemModify);

            Register<CChangeSkin>(GameOpCode.ChangeSkin);

            #region Client PersonalShopMessages
            Register<CPShopSetItemPrice>(GameOpCode.PShopSetItemPrice);
            Register<CPShopRequestOpen>(GameOpCode.PShopRequestOpen);
            Register<CPShopRequestClose>(GameOpCode.PShopRequestClose);
            Register<CPShopRequestList>(GameOpCode.PShopRequestList);
            Register<CPShopRequestBuy>(GameOpCode.PShopRequestBuy);
            Register<CPShopCloseDeal>(GameOpCode.PShopCloseDeal);

            Register<CPShopSearch>(GameOpCode.PShopSearchS16Kor);
            Register<SPShopSearch>(GameOpCode.PShopSearchS16Kor);
            Register<CPShopItemSearch>(GameOpCode.PShopItemSearchS16Kor);
            Register<SPShopItemSearch>(GameOpCode.PShopItemSearchS16Kor);
            Register<CPShopItemSearch2>(GameOpCode.PShopItemSearch2S16Kor);
            Register<CPShopRequestList2S16Kor>(GameOpCode.PShopRequestList2S16Kor);
            Register<SPShopSellList>(GameOpCode.PShopRequestList2S16Kor);
            Register<CPShopSetItemPriceS16Kor>(GameOpCode.PShopSetItemPriceS16Kor);
            Register<SPShopSetItemPriceS16Kor>(GameOpCode.PShopSetItemPriceS16Kor);
            Register<CPShopChangeStateS16Kor>(GameOpCode.PShopChangeState);
            Register<SPShopChangeStateS16Kor>(GameOpCode.PShopChangeState);
            Register<CPShopItemViewS16Kor>(GameOpCode.PShopItemViewS16Kor);
            Register<SPShopItemViewS16Kor>(GameOpCode.PShopItemViewS16Kor);
            Register<CPShopCancelItemSaleS16Kor>(GameOpCode.PShopCancelItemSaleS16Kor);
            Register<SPShopCancelItemSaleS16Kor>(GameOpCode.PShopCancelItemSaleS16Kor);
            #endregion


            Register<CNPCJulia>(GameOpCode.NPCJulia);
            Register<SNPCDialog>(GameOpCode.NPCDialog);
            #region Client AttackMessages
            //Register<CAttackS5E2>(GameOpCode.Attack);

            VersionSelector.Register<SMagicDuration>(ServerSeason.Season6Kor, GameOpCode.MagicDuration);
            VersionSelector.Register<SMagicDurationS9Eng>(ServerSeason.Season9Eng, GameOpCode.MagicDuration);
            VersionSelector.Register<SMagicAttack>(ServerSeason.Season6Kor, GameOpCode.MagicAttack);
            VersionSelector.Register<SMagicAttackS9Eng>(ServerSeason.Season9Eng, GameOpCode.MagicAttack);
            VersionSelector.Register<SMagicAttackS12Eng>(ServerSeason.Season12Eng, GameOpCode.MagicAttack);

            Register<CItemGet>(GameOpCode.ItemGet);
            switch (Season)
            {
                case ServerSeason.Season17Kor:
                case ServerSeason.Season16Kor:
                    Register<CBeattackS9>(GameOpCode.BeattackS16);
                    Register<CMove12Eng>(GameOpCode.Move12Eng);
                    Register<SMove>(GameOpCode.Move12Eng);
                    Register<CAttack>(GameOpCode.Attack12Eng);
                    Register<CMagicAttackS9>(GameOpCode.MagicAttack);
                    Register<SMagicAttackS12Eng>(GameOpCode.MagicAttack);
                    Register<CMagicDurationS16>(GameOpCode.MagicDuration);
                    Register<SMagicDurationS9Eng>(GameOpCode.MagicDuration);
                    break;
                case ServerSeason.Season12Eng:
                    Register<SMove>(GameOpCode.Move12Eng);
                    Register<CAttack>(GameOpCode.Attack12Eng);
                    Register<CBeattackS9>(GameOpCode.Position);
                    Register<CMagicAttackS9>(GameOpCode.MagicAttack);
                    Register<CMagicDurationS9>(GameOpCode.MagicDuration);
                    Register<SMagicAttackS12Eng>(GameOpCode.MagicAttack);
                    Register<SMagicDurationS9Eng>(GameOpCode.MagicDuration);
                    break;
                case ServerSeason.Season9Eng:// ENG
                    Register<CAttack>(GameOpCode.AttackEng);
                    Register<CMagicAttackS9>(GameOpCode.MagicAttack);
                    Register<CTeleportS9>(GameOpCode.Teleport);
                    Register<CMagicDurationS9>(GameOpCode.MagicDuration);
                    Register<CPositionSetS9>(GameOpCode.Position9Eng);
                    Register<CBeattackS9>(GameOpCode.Position);
                    Register<SMagicAttackS9Eng>(GameOpCode.MagicAttack);
                    Register<SMagicDurationS9Eng>(GameOpCode.MagicDuration);
                    Register<CMoveEng>(GameOpCode.MoveEng);
                    Register<SMove>(GameOpCode.MoveEng);
                    break;
                default:
                    Register<CAttack>(GameOpCode.Attack);
                    Register<CMagicAttack>(GameOpCode.MagicAttack);
                    Register<CMagicDuration>(GameOpCode.MagicDuration);
                    Register<CTeleport>(GameOpCode.Teleport);
                    Register<CPositionSet>(GameOpCode.Position);
                    Register<CBeattack>(GameOpCode.Beattack);
                    Register<CMove>(GameOpCode.Move);
                    Register<SMove>(GameOpCode.Move);
                    Register<SMagicDuration>(GameOpCode.MagicDuration);
                    Register<SMagicAttack>(GameOpCode.MagicAttack);
                    break;
            }

            Register<SAttackResultS16Kor>(GameOpCode.Attack12Eng);
            Register<SAttackResultS12Eng>(GameOpCode.Attack12Eng);
            Register<SAttackResultS9Eng>(GameOpCode.AttackEng);
            Register<SAttackResult>(GameOpCode.Attack);
            VersionSelector.Register<SAttackResult>(ServerSeason.Season6Kor, GameOpCode.Attack);
            VersionSelector.Register<SAttackResultS9Eng>(ServerSeason.Season9Eng, GameOpCode.Attack);
            VersionSelector.Register<SAttackResultS12Eng>(ServerSeason.Season12Eng, GameOpCode.Attack);
            VersionSelector.Register<SAttackResultS16Kor>(ServerSeason.Season16Kor, GameOpCode.Attack);
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
            Register<CWaitFriendAddReq>(GameOpCode.FriendAddWait);
            Register<SFriendAddReq>(GameOpCode.FriendAdd);
            Register<SFriendAddSin>(GameOpCode.FriendAddWait);
            #endregion

            #region MasterSystemMessages
            Register<CMasterSkill>(GameOpCode.MasterLevelSkill);
            #endregion

            // S2C
            Register<SInventory>(GameOpCode.Inventory);
            Register<SEquipament>(GameOpCode.Equipament);
            Register<SCheckSum>(GameOpCode.GameSecurity);
            Register<SWeather>(GameOpCode.Weather);
            Register<SSpells>(GameOpCode.Spells);
            Register<SSpellsS12Eng>(GameOpCode.Spells);
            VersionSelector.Register<SSpells>(ServerSeason.Season6Kor, GameOpCode.Spells);
            VersionSelector.Register<SSpellsS12Eng>(ServerSeason.Season12Eng, GameOpCode.Spells);
            Register<SQuestInfo>(GameOpCode.QuestInfo);
            Register<SFriends>(GameOpCode.FriendList);
            Register<SKillCount>(GameOpCode.KillCount);

            Register<SChatNickName>(GameOpCode.GeneralChat0);
            Register<SChatTarget>(GameOpCode.GeneralChat1);

            #region Server ViewPortMessages
            Register<SViewPortCreate>(GameOpCode.ViewPortCreate);
            Register<SViewPortCreateS9>(GameOpCode.ViewPortCreate);
            Register<SViewPortCreateS12>(GameOpCode.ViewPortCreate);
            Register<SViewPortCreateS16Kor>(GameOpCode.ViewPortCreate);
            VersionSelector.Register<SViewPortCreate>(ServerSeason.Season6Kor, GameOpCode.ViewPortCreate);
            VersionSelector.Register<SViewPortCreateS9>(ServerSeason.Season9Eng, GameOpCode.ViewPortCreate);
            VersionSelector.Register<SViewPortCreateS12>(ServerSeason.Season12Eng, GameOpCode.ViewPortCreate);
            VersionSelector.Register<SViewPortCreateS16Kor>(ServerSeason.Season16Kor, GameOpCode.ViewPortCreate);
            Register<SViewPortChange>(GameOpCode.ViewPortChange);
            Register<SViewPortChangeS9>(GameOpCode.ViewPortChange);
            Register<SViewPortChangeS12>(GameOpCode.ViewPortChange);
            Register<SViewPortMonCreateS6Kor>(GameOpCode.ViewPortMCreate);
            Register<SViewPortMonCreateS9Eng>(GameOpCode.ViewPortMCreate);
            Register<SViewPortMonCreateS12Eng>(GameOpCode.ViewPortMCreate);
            Register<SVPortMonCreateS16Kor>(GameOpCode.ViewPortMCreate);
            VersionSelector.Register<SViewPortMonCreateS6Kor>(ServerSeason.Season6Kor, GameOpCode.ViewPortMCreate);
            VersionSelector.Register<SViewPortMonCreateS9Eng>(ServerSeason.Season9Eng, GameOpCode.ViewPortMCreate);
            VersionSelector.Register<SViewPortMonCreateS12Eng>(ServerSeason.Season12Eng, GameOpCode.ViewPortMCreate);
            VersionSelector.Register<SVPortMonCreateS16Kor>(ServerSeason.Season16Kor, GameOpCode.ViewPortMCreate);
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
            Register<SPositionSet>(GameOpCode.Position);
            Register<SPositionSetS9Eng>(GameOpCode.Position9Eng);
            Register<CPositionSet>(GameOpCode.Position16Kor);
            Register<SPositionSetS16Kor>(GameOpCode.Position16Kor);
            VersionSelector.Register<SPositionSet>(ServerSeason.Season6Kor, GameOpCode.Position);
            VersionSelector.Register<SPositionSetS9Eng>(ServerSeason.Season9Eng, GameOpCode.Position);
            VersionSelector.Register<SPositionSetS16Kor>(ServerSeason.Season16Kor, GameOpCode.Position);
            Register<SPointAdd>(GameOpCode.PointAdd);
            Register<SCharRegen>(GameOpCode.CharRegen);
            Register<SCharRegenS12Eng>(GameOpCode.CharRegen);
            Register<SCharRegenS16Kor>(GameOpCode.CharRegen);
            VersionSelector.Register<SCharRegen>(ServerSeason.Season6Kor, GameOpCode.CharRegen);
            VersionSelector.Register<SCharRegenS12Eng>(ServerSeason.Season12Eng, GameOpCode.CharRegen);
            VersionSelector.Register<SCharRegenS16Kor>(ServerSeason.Season16Kor, GameOpCode.CharRegen);
            Register<SLevelUp>(GameOpCode.LevelUp);
            Register<SClinetClose>(GameOpCode.ClientClose);
            Register<SMoveItem>(GameOpCode.MoveItem);
            Register<SMoveItemS16Kor>(GameOpCode.MoveItem);
            VersionSelector.Register<SMoveItem>(ServerSeason.Season6Kor, GameOpCode.MoveItem);
            VersionSelector.Register<SMoveItemS16Kor>(ServerSeason.Season16Kor, GameOpCode.MoveItem);
            Register<SEventEnterCount>(GameOpCode.EventEnterCount);
            Register<SCloseMsg>(GameOpCode.ClientClose);
            Register<STalk>(GameOpCode.Talk);
            Register<SShopItemList>(GameOpCode.CloseWindow); // Same OPCode
            Register<SMonsterSoulShop>(GameOpCode.MonsterSoulShop);
            Register<SMonsterSoulAvailableShop>(GameOpCode.MonsterSoulAvailableShop);
            Register<STax>(GameOpCode.Tax);
            Register<CWarehouseMoney>(GameOpCode.WarehouseMoney);
            Register<SWarehouseMoney>(GameOpCode.WarehouseMoney);
            Register<SQuestWindow>(GameOpCode.QuestWindow);
            Register<SBuy>(GameOpCode.Buy);
            Register<SSell>(GameOpCode.Sell);
            Register<SItemGet>(GameOpCode.ItemGet);
            Register<SItemGetS12Eng>(GameOpCode.ItemGet);
            Register<SItemGetS16Kor>(GameOpCode.ItemGet);
            Register<STeleport>(GameOpCode.Teleport);
            Register<STeleportS12Eng>(GameOpCode.Teleport);
            VersionSelector.Register<STeleport>(ServerSeason.Season6Kor, GameOpCode.Teleport);
            VersionSelector.Register<STeleportS12Eng>(ServerSeason.Season12Eng, GameOpCode.Teleport);
            Register<SViewSkillState>(GameOpCode.ViewSkillState);
            Register<SPeriodicEffectS12Eng>(GameOpCode.PeriodicEffect);
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
            Register<SKillPlayerEXT>(GameOpCode.KillPlayerEXT);
            Register<SDiePlayer>(GameOpCode.DiePlayer);
            Register<SEffect>(GameOpCode.Effect);
            Register<SItemModify>(GameOpCode.ItemModify);
            Register<SItemUseSpecialTime>(GameOpCode.ItemUseSpecialTime);

            VersionSelector.Register<SItemGetS16Kor>(ServerSeason.Season16Kor, GameOpCode.ItemGet);
            VersionSelector.Register<SItemGetS12Eng>(ServerSeason.Season12Eng, GameOpCode.ItemGet);
            VersionSelector.Register<SItemGet>(ServerSeason.Season6Kor, GameOpCode.ItemGet);
            VersionSelector.Register<SPShopRequestList>(ServerSeason.Season6Kor, GameOpCode.PShopRequestList);
            VersionSelector.Register<SPShopRequestListS9Eng>(ServerSeason.Season9Eng, GameOpCode.PShopRequestList);
            Register<SPShopSetItemPrice>(GameOpCode.PShopSetItemPrice);
            Register<SPShopRequestOpen>(GameOpCode.PShopRequestOpen);
            Register<SPShopRequestClose>(GameOpCode.PShopRequestClose);
            Register<SPShopRequestList>(GameOpCode.PShopRequestList);
            Register<SPShopRequestListS9Eng>(GameOpCode.PShopRequestList);
            Register<SPShopRequestBuy>(GameOpCode.PShopRequestBuy);
            Register<SPShopRequestSold>(GameOpCode.PShopRequestSold);
            Register<SPShopAlterVault>(GameOpCode.PShopAlterVault);

            #region Server PartyMessages
            Register<SPartyResult>(GameOpCode.PartyResult);
            Register<SPartyList>(GameOpCode.PartyList);
            Register<SPartyListS9>(GameOpCode.PartyList);
            Register<SPartyListS16>(GameOpCode.PartyList);
            VersionSelector.Register<SPartyList>(ServerSeason.Season6Kor, GameOpCode.PartyList);
            VersionSelector.Register<SPartyListS9>(ServerSeason.Season9Eng, GameOpCode.PartyList);
            VersionSelector.Register<SPartyListS16>(ServerSeason.Season16Kor, GameOpCode.PartyList);
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

            #region Server MasterLevelMessages
            Register<SMasterInfo>(GameOpCode.MasterLevelInfo);
            Register<SMasterLevelUp>(GameOpCode.MasterLevelUp);
            Register<SMasterLevelSkillS9ENG>(GameOpCode.MasterLevelSkill);
            Register<SMasterLevelSkillListS9ENG>(GameOpCode.MasterLevelSkills);
            Register<SMajesticInfo>(GameOpCode.MajesticInfo);
            Register<SMajesticStatsInfo>(GameOpCode.MajesticStatsInfo);
            #endregion

            Register<CTradeRequest>(GameOpCode.TradeRequest);
            Register<STradeRequest>(GameOpCode.TradeRequest);
            Register<CTradeResponce>(GameOpCode.TradeResponce);
            Register<STradeResponce>(GameOpCode.TradeResponce);
            Register<STradeOtherAdd>(GameOpCode.TradeOtherAdd);
            Register<CTradeMoney>(GameOpCode.TradeMoney);
            Register<STradeMoney>(GameOpCode.TradeMoney);
            Register<STradeOtherMoney>(GameOpCode.TradeOtherMoney);
            Register<CTradeButtonOk>(GameOpCode.TradeButtonOk);
            Register<CTradeButtonCancel>(GameOpCode.TradeButtonCancel);
            Register<STradeResult>(GameOpCode.TradeButtonCancel);

            #region MuunSystem
            Register<CMuunItemExchange>(GameOpCode.MuunItemExchange);
            Register<CMuunItemGet>(GameOpCode.MuunItemGet);
            Register<SMuunItemGet>(GameOpCode.MuunItemGet);
            Register<CMuunItemRideSelect>(GameOpCode.MuunItemRideSelect);
            Register<CMuunItemSell>(GameOpCode.MuunItemSell);
            Register<CMuunItemUse>(GameOpCode.MuunItemUse);
            Register<SMuunRideVP>(GameOpCode.MuunRideViewPort);
            #endregion

            Register<SMiniMapNPC>(GameOpCode.MiniMapNPC);

            Register<SPeriodItemCount>(GameOpCode.PeriodItemCount);

            Register<SPentagramaJewelInfo>(GameOpCode.PentagramaJInfo);

            Register<SUBFInfo>(GameOpCode.UBFInfo);
            Register<CUsePopUpType>(GameOpCode.PopUpType);
            Register<SUBFPopUpType>(GameOpCode.PopUpType);

            Register<SMuunInventory>(GameOpCode.MuunInventory);

            Register<CMemberPosInfoStart>(GameOpCode.MemberPosInfoStart);
            Register<CMemberPosInfoStop>(GameOpCode.MemberPosInfoStop);

            Register<SLifeInfo>(GameOpCode.LifeInfo);
            Register<CMuHelperState>(GameOpCode.MuHelperSwitch);
            Register<CMUBotData>(GameOpCode.MuHelper);
            Register<SMuHelperState>(GameOpCode.MuHelperSwitch);
            Register<SAttackSpeed>(GameOpCode.AttackSpeed);
            Register<CQuestExp>(GameOpCode.QuestExp);
            Register<CShadowBuff>(GameOpCode.ShadowBuff);
            Register<SChainMagic>(GameOpCode.ChainMagic);

            Register<CGremoryCaseOpenS16>(GameOpCode.GremoryCaseOpenS16);
            Register<CGremoryCaseOpen>(GameOpCode.GremoryCaseOpen);
            Register<SGremoryCaseOpenS16>(GameOpCode.GremoryCaseOpenS16);
            Register<SGremoryCaseOpen>(GameOpCode.GremoryCaseOpen);
            Register<SGremoryCaseReceiveItem>(GameOpCode.GremoryCaseReceive);
            Register<SGremoryCaseNotice>(GameOpCode.GremoryCaseNotice);
            Register<SGremoryCaseDelete>(GameOpCode.GremoryCaseDelete);
            Register<SGremoryCaseDeleteS16>(GameOpCode.GremoryCaseDelete);
            VersionSelector.Register<SGremoryCaseDelete>(ServerSeason.Season6Kor, GameOpCode.GremoryCaseDelete);
            VersionSelector.Register<SGremoryCaseDeleteS16>(ServerSeason.Season16Kor, GameOpCode.GremoryCaseDelete);
            Register<SGremoryCaseList>(GameOpCode.GremoryCaseList);
            Register<CGremoryCaseUseItem>(GameOpCode.GremoryCaseUse);
            Register<SGremoryCaseUseItem>(GameOpCode.GremoryCaseUse);

            Register<CPShopSearchItem>(GameOpCode.PShopSearchItem);
            Register<SPShopSearchItem>(GameOpCode.PShopSearchItem);

            Register<CAcheronEnterReq>(GameOpCode.AcheronEnter);

            Register<CRefineJewelReq>(GameOpCode.RefineJewel);
            Register<CPentagramaJewelIn>(GameOpCode.PentagramaJewelIn);
            Register<SPentagramJewelIn>(GameOpCode.PentagramaJewelIn);
            Register<SPentagramJewelInOut>(GameOpCode.PentagramaJewelInOut);
            Register<SPentagramJewelInfo>(GameOpCode.PentagramaJInfo);
            Register<SElementalDamage>(GameOpCode.ElementDamage);
            Register<SNeedSpiritMap>(GameOpCode.NeedSpiritMap);

            Register<CPetInfo>(GameOpCode.PetInfo);
            Register<SPetInfo>(GameOpCode.PetInfo);
            Register<CPetCommand>(GameOpCode.PetCommand);
            Register<SPetAttack>(GameOpCode.PetAttack);
            Register<CInventoryEquipament>(GameOpCode.InventoryEquipament);
            Register<SEquipamentChange>(GameOpCode.EquipamentChange);

            Register<SExpEventInfo>(GameOpCode.ExpEventInfo);

            Register<CSXInfo>(GameOpCode.SXInfo);
            Register<SXElementalData>(GameOpCode.SXElementalData);
            Register<SXCharacterInfo>(GameOpCode.SXCharacterInfo);
            Register<SXUpPront>(GameOpCode.SXUpPront);

            Register<SNQWorldLoad>(GameOpCode.NewQuestWorldLoad);
            Register<SNQWorldList>(GameOpCode.NewQuestWorldList);

            Register<SPKLevel>(GameOpCode.PKLevel);

            Register<SMonsterSkillS9Eng>(GameOpCode.MonsterSkill);

            Register<CFavoritesList>(GameOpCode.FavoritesList);
            if(Season == ServerSeason.Season16Kor)
            {
                ChangeOPCode<CFavoritesList>(GameOpCode.FavoritesListS16Kor);
            }
            Register<SEventNotificationS16Kor>(GameOpCode.Eventnotification);


            Register<CPartyMRegister>(GameOpCode.PartyMatchingRegister);
            Register<SPartyMRegister>(GameOpCode.PartyMatchingRegister);
            Register<CPartyMSearch>(GameOpCode.PartyMatchingSearch);
            Register<SPartyMSearch>(GameOpCode.PartyMatchingSearch);
            Register<CPartyMJoin>(GameOpCode.PartyMatchingJoin);
            Register<SPartyMJoin>(GameOpCode.PartyMatchingJoin);
            Register<CPartyMJoinData>(GameOpCode.PartyMatchingJoinData);
            Register<CPartyMJoinList>(GameOpCode.PartyMatchingJoinList);
            Register<SPartyMJoinList>(GameOpCode.PartyMatchingJoinList);
            Register<CPartyMAccept>(GameOpCode.PartyMatchingAccept);
            Register<CPartyMCancel>(GameOpCode.PartyMatchingCancel);
            Register<SPartyMCancel>(GameOpCode.PartyMatchingCancel);
            Register<CPartyLeaderChange>(GameOpCode.PartyLeaderChange);
            Register<SPartyMJoinNotify>(GameOpCode.PartyJoinNotify);

            Register<CHuntingRecordRequest>(GameOpCode.HuntingRecordRequest);
            Register<CHuntingRecordClose>(GameOpCode.HuntingRecordClose);
            Register<CHuntingRecordVisibility>(GameOpCode.HuntingRecordVisibility);
            Register<SHuntingRecordList>(GameOpCode.HuntingRecordRequest);
            Register<SHuntingRecordDay>(GameOpCode.HuntingRecordDay);
            Register<SHuntingRecordTime>(GameOpCode.HuntingRecordCurrent);

            Register<SMossMerchantOpen>(GameOpCode.MossMerchant);
            Register<CMossMerchantOpenBox>(GameOpCode.MossMerchantOpenBox);
            Register<SMossMerchantOpenBox>(GameOpCode.MossMerchantOpenBoxReward);

            Register<CRuudBuy>(GameOpCode.Ruudbuy);
            Register<SRuudBuy>(GameOpCode.Ruudbuy);
            Register<SRuudSend>(GameOpCode.RuudSend);
        }
    }
}
