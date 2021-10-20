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
            Register<CMuRummyOpen>(EventOpCode.MuRummyOpen);
            Register<SMuRummyOpen>(EventOpCode.MuRummyOpen);

            //
            Register<CEventItemGet>(EventOpCode.EventItemGet);
            Register<SEventItemGet>(EventOpCode.EventItemGet);
            Register<CEventItemThrow>(EventOpCode.EventItemThrow);
            Register<SEventItemThrow>(EventOpCode.EventItemThrow);

            Register<SEventInventory>(EventOpCode.EventInventory);
        }
    }
}
