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

            Register<CAction>(GameOpCode.Rotation);
            Register<CMove>(GameOpCode.Move);
            Register<CMoveEng>(GameOpCode.MoveEng);
            Register<CMove12Eng>(GameOpCode.Move12Eng);

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
            Register<CItemGet>(GameOpCode.ItemGet);
            Register<CItemModify>(GameOpCode.ItemModify);

            #region Client PersonalShopMessages
            Register<CPShopSetItemPrice>(GameOpCode.PShopSetItemPrice);
            Register<CPShopRequestOpen>(GameOpCode.PShopRequestOpen);
            Register<CPShopRequestClose>(GameOpCode.PShopRequestClose);
            Register<CPShopRequestList>(GameOpCode.PShopRequestList);
            Register<CPShopRequestBuy>(GameOpCode.PShopRequestBuy);
            Register<CPShopCloseDeal>(GameOpCode.PShopCloseDeal);
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

            switch (Season)
            {
                case ServerSeason.Season12Eng:
                    Register<SMove>(GameOpCode.Move12Eng);
                    Register<CAttack>(GameOpCode.Attack12Eng);
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
                    Register<SMove>(GameOpCode.MoveEng);
                    break;
                default:
                    Register<CAttack>(GameOpCode.Attack);
                    Register<CMagicAttack>(GameOpCode.MagicAttack);
                    Register<CMagicDuration>(GameOpCode.MagicDuration);
                    Register<CTeleport>(GameOpCode.Teleport);
                    Register<CPositionSet>(GameOpCode.Position);
                    Register<CBeattack>(GameOpCode.Beattack);
                    Register<SMove>(GameOpCode.Move);
                    Register<SMagicDuration>(GameOpCode.MagicDuration);
                    Register<SMagicAttack>(GameOpCode.MagicAttack);
                    break;
            }

            Register<SAttackResultS12Eng>(GameOpCode.Attack12Eng);
            Register<SAttackResultS9Eng>(GameOpCode.AttackEng);
            Register<SAttackResult>(GameOpCode.Attack);
            VersionSelector.Register<SAttackResult>(ServerSeason.Season6Kor, GameOpCode.Attack);
            VersionSelector.Register<SAttackResultS9Eng>(ServerSeason.Season9Eng, GameOpCode.Attack);
            VersionSelector.Register<SAttackResultS12Eng>(ServerSeason.Season12Eng, GameOpCode.Attack);
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
            Register<SViewPortChange>(GameOpCode.ViewPortChange);
            Register<SViewPortCreateS9>(GameOpCode.ViewPortCreate);
            Register<SViewPortChangeS9>(GameOpCode.ViewPortChange);
            Register<SViewPortMonCreate>(GameOpCode.ViewPortMCreate);
            Register<SViewPortMonCreateS9>(GameOpCode.ViewPortMCreate);
            Register<SViewPortDestroy>(GameOpCode.ViewPortDestroy);
            Register<SViewPortItemDestroy>(GameOpCode.ViewPortItemDestroy);
            #endregion

            Register<SNotice>(GameOpCode.Notice);
            Register<SEventState>(GameOpCode.EventState);
            //Register<SNewQuestInfo>(GameOpCode.NewQuestInfo);
            Register<SHeatlUpdate>(GameOpCode.HealthUpdate);
            Register<SManaUpdate>(GameOpCode.ManaUpdate);
            Register<SSkillKey>(GameOpCode.SkillKey);
            Register<SAction>(GameOpCode.Rotation);
            Register<SPositionSet>(GameOpCode.Position);
            Register<SPositionSetS9Eng>(GameOpCode.Position9Eng);
            Register<SPointAdd>(GameOpCode.PointAdd);
            Register<SCharRegen>(GameOpCode.CharRegen);
            Register<SCharRegenS12Eng>(GameOpCode.CharRegen);
            VersionSelector.Register<SCharRegen>(ServerSeason.Season6Kor, GameOpCode.CharRegen);
            VersionSelector.Register<SCharRegenS12Eng>(ServerSeason.Season12Eng, GameOpCode.CharRegen);
            Register<SLevelUp>(GameOpCode.LevelUp);
            Register<SClinetClose>(GameOpCode.ClientClose);
            Register<SMoveItem>(GameOpCode.MoveItem);
            Register<SEventEnterCount>(GameOpCode.EventEnterCount);
            Register<SCloseMsg>(GameOpCode.ClientClose);
            Register<STalk>(GameOpCode.Talk);
            Register<SShopItemList>(GameOpCode.CloseWindow); // Same OPCode
            Register<STax>(GameOpCode.Tax);
            Register<CWarehouseMoney>(GameOpCode.WarehouseMoney);
            Register<SWarehouseMoney>(GameOpCode.WarehouseMoney);
            Register<SQuestWindow>(GameOpCode.QuestWindow);
            Register<SBuy>(GameOpCode.Buy);
            Register<SSell>(GameOpCode.Sell);
            Register<SItemGet>(GameOpCode.ItemGet);
            Register<SItemGetS12Eng>(GameOpCode.ItemGet);
            Register<STeleport>(GameOpCode.Teleport);
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

            Register<CGremoryCaseOpen>(GameOpCode.GremoryCaseOpen);
            Register<SGremoryCaseOpen>(GameOpCode.GremoryCaseOpen);
            Register<SGremoryCaseReceiveItem>(GameOpCode.GremoryCaseReceive);
            Register<SGremoryCaseNotice>(GameOpCode.GremoryCaseNotice);
            Register<SGremoryCaseDelete>(GameOpCode.GremoryCaseDelete);
            Register<SGremoryCaseList>(GameOpCode.GremoryCaseList);

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
            Register<SXCharacterInfo>(GameOpCode.SXCharacterInfo);
            Register<SXUpPront>(GameOpCode.SXUpPront);

            Register<SNQWorldLoad>(GameOpCode.NewQuestWorldLoad);
            Register<SNQWorldList>(GameOpCode.NewQuestWorldList);

            Register<SPKLevel>(GameOpCode.PKLevel);

            Register<SMonsterSkillS9Eng>(GameOpCode.MonsterSkill);
        }
    }
}
