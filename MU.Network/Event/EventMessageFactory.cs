using MU.Network.CastleSiege;
using MU.Resources;
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
        public EventMessageFactory(ServerSeason Season)
        {
            // C2S
            if(Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, true);
            Register<CEventRemainTime>(EventOpCode.RemainTime);
            //LuckyCoins
            Register<CLuckyCoinsCount>(EventOpCode.LuckyCoinsCount);
            Register<CLuckyCoinsRegistre>(EventOpCode.LuckyCoinsRegistre);
            Register<CBloodCastleMove>(EventOpCode.BloodCastleEnter);

            // Crywolf
            Register<CCrywolfState>(EventOpCode.CrywolfState);
            Register<CCrywolfContract>(EventOpCode.CrywolfContract);
            Register<CCrywolfBenefit>(EventOpCode.CrywolfBenefit);
            // DevilSquare
            Register<CDevilSquareMove>(EventOpCode.DevilSquareMove);

            // ChaosCastle
            Register<CChaosCastleMove>(EventOpCode.ChaosCastleMove);

            Register<CKanturuStateInfo>(EventOpCode.KanturuState);
            Register<CKanturuEnterBossMap>(EventOpCode.KanturuEnter);
            Register<CImperialGuardianEnter>(EventOpCode.ImperialGuardianEnter);
            // MuRummy
            Register<CEventInventoryOpenS16>(EventOpCode.EventInventoryOpen);
            Register<CMuRummyStart>(EventOpCode.MuRummyStart);
            Register<CMuRummyPlayCard>(EventOpCode.MuRummyPlayCard);
            Register<CMuRummyMatch>(EventOpCode.MuRummyMatch);
            Register<CMuRummySpecialMatch>(EventOpCode.MuRummySpecialMatch);
            Register<CMuRummyThrow>(EventOpCode.MuRummyThrow);
            Register<CMuRummyExit>(EventOpCode.MuRummyExit);
            Register<CMuRummyReveal>(EventOpCode.MuRummyReveal);

            //MineSweeper
            Register<CMineSweeperOpen>(EventOpCode.MineSweeper);
            Register<CMineSweeperStart>(EventOpCode.MineSweeperStart);
            Register<CMineSweeperReveal>(EventOpCode.MineSweeperReveal);
            Register<CMineSweeperMark>(EventOpCode.MineSweeperMark);
            Register<CMineSweeperGetReward>(EventOpCode.MineSweeperGetReward);
            //JewelBingo
            //Register<CJewelBingoMove>(EventOpCode.JeweldryBingoState);
            Register<CJewelBingoStart>(EventOpCode.JeweldryBingoState);
            Register<CJewelBingoMove>(EventOpCode.JeweldryBingoInfo);
            Register<CJewelBingoBox>(EventOpCode.JeweldryBingoBox);
            Register<CJewelBingoSelect>(EventOpCode.JeweldryBingoPlayInfo);
            Register<CJewelBingoGetReward>(EventOpCode.JeweldryBingoPlayResult);
            //BallsAndCows
            Register<CBallsAndCowsStart>(EventOpCode.BallsAndCowsStart);
            Register<CBallsAndCowsPick>(EventOpCode.BallsAndCowsPick);
            Register<CEventItemGet>(EventOpCode.EventItemGet);
            Register<CEventItemThrow>(EventOpCode.EventItemThrow);
            Register<CAcheronEventEnter>(EventOpCode.AcheronEnterReq);
            //CastleSiege
            Register<CSiegeState>(EventOpCode.CastleSiegeState);
            Register<CGuildRegisteInfo>(EventOpCode.CastleSiegeGuildInfo);
            Register<CGuildMarkOfCastleOwner>(EventOpCode.CastleSiegeGuildMarkOfOwner);
            Register<CGuildRegiste>(EventOpCode.CastleSiegeRegiste);
            Register<CSiegeGuildList>(EventOpCode.CastleSiegeGuildList);
            Register<CSiegeRegisteMark>(EventOpCode.CastleSiegeRegisteMark);

            if (Season == ServerSeason.Season17Kor75) Converter = (opCode) => Data.ProtocolXChangeS17K75(opCode, false);
            Register<SCrywolfState>(EventOpCode.CrywolfState);
            Register<SCrywolfLeftTime>(EventOpCode.CrywolfLeftTime);
            Register<SCrywolfStatueAndAltarInfo>(EventOpCode.CrywolfStatueAndAltarInfo);
            Register<SCrywolfBossMonsterInfo>(EventOpCode.CrywolfBossMonsterInfo);
            Register<SCrywolfStageEffect>(EventOpCode.CrywolfStageEffect);
            Register<SCrywolfPersonalRank>(EventOpCode.CrywolfPersonalRank);
            Register<SCrywolfHeroList>(EventOpCode.CrywolfHeroList);


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
            Register<SKanturuBattleTime>(EventOpCode.KanturuBattleTime);
            Register<SKanturuMonsterUserCount>(EventOpCode.KanturuMonsterUserCount);
            Register<SKanturuBattleResult>(EventOpCode.KanturuBattleResult);
            Register<SKanturuWideAttack>(EventOpCode.KanturuWideAttack);

            // Imperial
            Register<SImperialEnterResult>(EventOpCode.ImperialGuardianEnterResult);
            Register<SImperialNotifyZoneTime>(EventOpCode.ImperialGuardianNotifyZoneTime);
            Register<SImperialNotifyZoneClear>(EventOpCode.ImperialGuardianNotifyZoneAllClear);

            // ArcaBattle
            Register<SArcaBattleState>(EventOpCode.ArcaBattleState);

            // Banner
            Register<SSendBanner>(EventOpCode.Banner);

            Register<SEventInventoryOpenS16>(EventOpCode.EventInventoryOpen);
            Register<SMuRummyStart>(EventOpCode.MuRummyStart);
            Register<SMuRummyMessage>(EventOpCode.MuRummyMessage);
            Register<SMuRummyPlayCard>(EventOpCode.MuRummyPlayCard);
            Register<SMuRummyMatch>(EventOpCode.MuRummyMatch);
            Register<SMuRummyCardList>(EventOpCode.MuRummyCardList);
            Register<SMuRummyReveal>(EventOpCode.MuRummyReveal);
            Register<SMuRummyExit>(EventOpCode.MuRummyExit);
            Register<SMineSweeperOpen>(EventOpCode.MineSweeper);
            Register<SMineSweeperStart>(EventOpCode.MineSweeperStart);
            Register<SMineSweeperCreateCell>(EventOpCode.MineSweeperCreateCell);
            Register<SMineSweeperReveal>(EventOpCode.MineSweeperReveal);
            Register<SMineSweeperMark>(EventOpCode.MineSweeperMark);
            Register<SMineSweeperEnd>(EventOpCode.MineSweeperEnd);
            Register<SMineSweeperGetReward>(EventOpCode.MineSweeperGetReward);

            Register<SJewelBingoState>(EventOpCode.JeweldryBingoState);
            Register<SJewelBingoInfo>(EventOpCode.JeweldryBingoInfo);
            Register<SJewelBingoBox>(EventOpCode.JeweldryBingoBox);
            Register<SJewelBingoPlayInfo>(EventOpCode.JeweldryBingoPlayInfo);
            Register<SJewelBingoPlayResult>(EventOpCode.JeweldryBingoPlayResult);

            Register<SBallsAndCowsOpen>(EventOpCode.BallsAndCowsOpen);
            Register<SBallsAndCowsStart>(EventOpCode.BallsAndCowsStart);
            Register<SBallsAndCowsResult>(EventOpCode.BallsAndCowsResult);

            //
            Register<SEventItemGet>(EventOpCode.EventItemGet);
            Register<SEventItemGetS16>(EventOpCode.EventItemGet);
            VersionSelector.Register<SEventItemGet>(Resources.ServerSeason.Season9Eng, EventOpCode.EventItemGet);
            VersionSelector.Register<SEventItemGetS16>(Resources.ServerSeason.Season16Kor, EventOpCode.EventItemGet);
            Register<SEventItemThrow>(EventOpCode.EventItemThrow);

            Register<SEventInventory>(EventOpCode.EventInventory);
            Register<SEventInventoryS17>(EventOpCode.EventInventory);
            VersionSelector.Register<SEventInventory>(ServerSeason.Season6Kor, EventOpCode.EventInventory);
            VersionSelector.Register<SEventInventoryS17>(ServerSeason.Season17Kor75, EventOpCode.EventInventory);

            // Acheron Guardian
            Register<SAcheronEventEnter>(EventOpCode.AcheronEnter);

            Register<SLeftTimeAlarm>(EventOpCode.CastleSiegeLeftTimeAlarm);
            Register<SSiegeState>(EventOpCode.CastleSiegeState);
            Register<SGuildRegisteInfo>(EventOpCode.CastleSiegeGuildInfo);
            Register<SGuildMarkOfCastleOwner>(EventOpCode.CastleSiegeGuildMarkOfOwner);
            Register<SGuildRegiste>(EventOpCode.CastleSiegeRegiste);
            Register<SSiegeGuildList>(EventOpCode.CastleSiegeGuildList);
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
