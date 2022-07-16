using MU.Network.CastleSiege;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.Event
{
    public interface IEventMessage
    { }

    public class EventMessageFactory : MessageFactory<EventOpCode, IEventMessage>
    {
        public EventMessageFactory()
        {
            // C2S
            Register<CEventRemainTime>(EventOpCode.RemainTime);
            //LuckyCoins
            Register<CLuckyCoinsCount>(EventOpCode.LuckyCoinsCount);
            Register<CLuckyCoinsRegistre>(EventOpCode.LuckyCoinsRegistre);
            Register<CBloodCastleMove>(EventOpCode.BloodCastleEnter);

            // Crywolf
            Register<CCrywolfState>(EventOpCode.CrywolfState);
            Register<CCrywolfContract>(EventOpCode.CrywolfContract);
            Register<SCrywolfState>(EventOpCode.CrywolfState);
            Register<SCrywolfLeftTime>(EventOpCode.CrywolfLeftTime);
            Register<SCrywolfStatueAndAltarInfo>(EventOpCode.CrywolfStatueAndAltarInfo);
            Register<SCrywolfBossMonsterInfo>(EventOpCode.CrywolfBossMonsterInfo);
            Register<SCrywolfStageEffect>(EventOpCode.CrywolfStageEffect);
            Register<SCrywolfPersonalRank>(EventOpCode.CrywolfPersonalRank);
            Register<SCrywolfHeroList>(EventOpCode.CrywolfHeroList);
            Register<CCrywolfBenefit>(EventOpCode.CrywolfBenefit);

            // DevilSquare
            Register<CDevilSquareMove>(EventOpCode.DevilSquareMove);

            // ChaosCastle
            Register<CChaosCastleMove>(EventOpCode.ChaosCastleMove);

            Register<CKanturuStateInfo>(EventOpCode.KanturuState);

            // S2C
            Register<SEventRemainTime>(EventOpCode.RemainTime);
            //LuckyCoins
            Register<SLuckyCoinsCount>(EventOpCode.LuckyCoinsCount);
            Register<SLuckyCoinsRegistre>(EventOpCode.LuckyCoinsRegistre);
            // EventChip
            Register<SEventChipInfo>(EventOpCode.EventChipInfo);
            // BloodCastle
            Register<SBloodCastleMove>(EventOpCode.BloodCastleEnter);
            Register<SBloodCastleState>(EventOpCode.BloodCastleState);
            Register<SBloodCastleReward>(EventOpCode.BloodCastleReward);

            //DevilSquare
            Register<SDevilSquareSet>(EventOpCode.DevilSquareSet);
            Register<SDevilSquareResult>(EventOpCode.BloodCastleReward);

            // Crywolf
            Register<SCrywolfBenefit>(EventOpCode.CrywolfBenefit);

            // Kanturu
            Register<SKanturuStateInfo>(EventOpCode.KanturuState);
            Register<SKanturuStateChange>(EventOpCode.KanturuStateChange);
            Register<CKanturuEnterBossMap>(EventOpCode.KanturuEnter);
            Register<SKanturuBattleTime>(EventOpCode.KanturuBattleTime);
            Register<SKanturuMonsterUserCount>(EventOpCode.KanturuMonsterUserCount);
            Register<SKanturuBattleResult>(EventOpCode.KanturuBattleResult);
            Register<SKanturuWideAttack>(EventOpCode.KanturuWideAttack);

            // Imperial
            Register<CImperialGuardianEnter>(EventOpCode.ImperialGuardianEnter);
            Register<SImperialEnterResult>(EventOpCode.ImperialGuardianEnterResult);
            Register<SImperialNotifyZoneTime>(EventOpCode.ImperialGuardianNotifyZoneTime);
            Register<SImperialNotifyZoneClear>(EventOpCode.ImperialGuardianNotifyZoneAllClear);

            // ArcaBattle
            Register<SArcaBattleState>(EventOpCode.ArcaBattleState);

            // Banner
            Register<SSendBanner>(EventOpCode.Banner);

            // MuRummy
            Register<CEventInventoryOpenS16>(EventOpCode.EventInventoryOpen);
            Register<SEventInventoryOpenS16>(EventOpCode.EventInventoryOpen);
            Register<CMuRummyStart>(EventOpCode.MuRummyStart);
            Register<SMuRummyStart>(EventOpCode.MuRummyStart);
            Register<SMuRummyMessage>(EventOpCode.MuRummyMessage);
            Register<CMuRummyPlayCard>(EventOpCode.MuRummyPlayCard);
            Register<SMuRummyPlayCard>(EventOpCode.MuRummyPlayCard);
            Register<CMuRummyMatch>(EventOpCode.MuRummyMatch);
            Register<CMuRummySpecialMatch>(EventOpCode.MuRummySpecialMatch);
            Register<SMuRummyMatch>(EventOpCode.MuRummyMatch);
            Register<SMuRummyCardList>(EventOpCode.MuRummyCardList);
            Register<CMuRummyThrow>(EventOpCode.MuRummyThrow);
            Register<CMuRummyReveal>(EventOpCode.MuRummyReveal);
            Register<SMuRummyReveal>(EventOpCode.MuRummyReveal);
            Register<CMuRummyExit>(EventOpCode.MuRummyExit);
            Register<SMuRummyExit>(EventOpCode.MuRummyExit);

            //MineSweeper
            Register<CMineSweeperOpen>(EventOpCode.MineSweeper);
            Register<SMineSweeperOpen>(EventOpCode.MineSweeper);
            Register<CMineSweeperStart>(EventOpCode.MineSweeperStart);
            Register<SMineSweeperStart>(EventOpCode.MineSweeperStart);
            Register<SMineSweeperCreateCell>(EventOpCode.MineSweeperCreateCell);
            Register<CMineSweeperReveal>(EventOpCode.MineSweeperReveal);
            Register<SMineSweeperReveal>(EventOpCode.MineSweeperReveal);
            Register<CMineSweeperMark>(EventOpCode.MineSweeperMark);
            Register<SMineSweeperMark>(EventOpCode.MineSweeperMark);
            Register<SMineSweeperEnd>(EventOpCode.MineSweeperEnd);
            Register<CMineSweeperGetReward>(EventOpCode.MineSweeperGetReward);
            Register<SMineSweeperGetReward>(EventOpCode.MineSweeperGetReward);

            //JewelBingo
            //Register<CJewelBingoMove>(EventOpCode.JeweldryBingoState);
            Register<CJewelBingoStart>(EventOpCode.JeweldryBingoState);
            Register<SJewelBingoState>(EventOpCode.JeweldryBingoState);
            Register<CJewelBingoMove>(EventOpCode.JeweldryBingoInfo);
            Register<SJewelBingoInfo>(EventOpCode.JeweldryBingoInfo);
            Register<CJewelBingoBox>(EventOpCode.JeweldryBingoBox);
            Register<SJewelBingoBox>(EventOpCode.JeweldryBingoBox);
            Register<CJewelBingoSelect>(EventOpCode.JeweldryBingoPlayInfo);
            Register<SJewelBingoPlayInfo>(EventOpCode.JeweldryBingoPlayInfo);
            Register<CJewelBingoGetReward>(EventOpCode.JeweldryBingoPlayResult);
            Register<SJewelBingoPlayResult>(EventOpCode.JeweldryBingoPlayResult);

            //BallsAndCows
            Register<SBallsAndCowsOpen>(EventOpCode.BallsAndCowsOpen);
            Register<CBallsAndCowsStart>(EventOpCode.BallsAndCowsStart);
            Register<SBallsAndCowsStart>(EventOpCode.BallsAndCowsStart);
            Register<CBallsAndCowsPick>(EventOpCode.BallsAndCowsPick);
            Register<SBallsAndCowsResult>(EventOpCode.BallsAndCowsResult);

            //
            Register<CEventItemGet>(EventOpCode.EventItemGet);
            Register<SEventItemGet>(EventOpCode.EventItemGet);
            Register<SEventItemGetS16>(EventOpCode.EventItemGet);
            VersionSelector.Register<SEventItemGet>(Resources.ServerSeason.Season9Eng, EventOpCode.EventItemGet);
            VersionSelector.Register<SEventItemGetS16>(Resources.ServerSeason.Season16Kor, EventOpCode.EventItemGet);
            Register<CEventItemThrow>(EventOpCode.EventItemThrow);
            Register<SEventItemThrow>(EventOpCode.EventItemThrow);

            Register<SEventInventory>(EventOpCode.EventInventory);

            // Acheron Guardian
            Register<CAcheronEventEnter>(EventOpCode.AcheronEnterReq);
            Register<SAcheronEventEnter>(EventOpCode.AcheronEnter);

            //CastleSiege
            Register<SLeftTimeAlarm>(EventOpCode.CastleSiegeLeftTimeAlarm);
            Register<CSiegeState>(EventOpCode.CastleSiegeState);
            Register<SSiegeState>(EventOpCode.CastleSiegeState);
            Register<CGuildRegisteInfo>(EventOpCode.CastleSiegeGuildInfo);
            Register<SGuildRegisteInfo>(EventOpCode.CastleSiegeGuildInfo);
            Register<CGuildMarkOfCastleOwner>(EventOpCode.CastleSiegeGuildMarkOfOwner);
            Register<SGuildMarkOfCastleOwner>(EventOpCode.CastleSiegeGuildMarkOfOwner);
            Register<CGuildRegiste>(EventOpCode.CastleSiegeRegiste);
            Register<SGuildRegiste>(EventOpCode.CastleSiegeRegiste);
            Register<CSiegeGuildList>(EventOpCode.CastleSiegeGuildList);
            Register<SSiegeGuildList>(EventOpCode.CastleSiegeGuildList);
            Register<CSiegeRegisteMark>(EventOpCode.CastleSiegeRegisteMark);
            Register<SSiegeRegisteMark>(EventOpCode.CastleSiegeRegisteMark);
            Register<SJoinSideNotify>(EventOpCode.CastleSiegejoinSide);
            Register<SCastleSiegeNotifyStart>(EventOpCode.CastleSiegeNotifyStart);
            Register<SCastleSiegeNotifySwitch>(EventOpCode.CastleSiegeSwitchNotify);
            Register<SCastleSiegeMinimapData>(EventOpCode.CastleSiegeMinimap);
            Register<SCastleSiegeNotifySwitchInfo>(EventOpCode.CastleSiegeNotifySwitchInfo);
            Register<SCastleSiegeNotifyCrownState>(EventOpCode.CastleSiegeCrownState);
        }
    }
}
