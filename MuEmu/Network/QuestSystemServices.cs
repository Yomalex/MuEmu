using MuEmu.Resources;
using MuEmu.Resources.XML;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using WebZen.Handlers;
using MU.Network.QuestSystem;
using MU.Resources;
using MU.Network.Game;
using MuEmu.Monsters;

namespace MuEmu.Network
{
    public class QuestSystemServices : MessageHandler
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(QuestSystemServices));

        [MessageHandler(typeof(CSetQuestState))]
        public async Task CSetQuestState(GSSession session, CSetQuestState message)
        {
            var quests = session.Player.Character.Quests;
            var result = quests.SetState(message.Index);
            var serverState = quests.GetByIndex(message.Index).StateByte;

            await session.SendAsync(new SSetQuestState(message.Index, result, serverState));
        }

        [MessageHandler(typeof(CQuestEXP))]
        public async Task CQuestEXP(GSSession session, CQuestEXP message)
        {
            var @char = session.Player.Character;
            await @char.Quests.QuestEXPInfo(message.Index);
        }

        [MessageHandler(typeof(CQuestEXPProgress))]
        public async Task CQuestEXPProgress(GSSession session, CQuestEXPProgress message)
        {
            var @char = session.Player.Character;
            @char.Quests.QuestEXPSetProgress(message.Index, message.Result);
           await @char.Quests.QuestEXPInfo(message.Index);
        }

        [MessageHandler(typeof(CQuestEXPComplete))]
        public async Task CQuestEXPComplete(GSSession session, CQuestEXPComplete message)
        {
            var @char = session.Player.Character;
            await @char.Quests.QuestEXPCompleted(message.Index);
        }


        [MessageHandler(typeof(CNewQuestInfo))]
        public async Task CNewQuestInfo(GSSession session, CNewQuestInfo message)
        {
            if (message.dwQuestInfoIndexID == 0)
            {
                return;
            }
            var @char = session.Player.Character;
            var quest2 = ResourceLoader.XmlLoader<QuestEXPDto>("./Data/QuestEXP.xml");

            QuestInfoIndex info = message.dwQuestInfoIndexID;// @char.Quests.GetEpisodeByIndex();

            var result = quest2
                .QuestList
                .FirstOrDefault(x => x.QuestInfo.Any(y => y.Episode == info.Episode))?
                .QuestInfo
                .FirstOrDefault(x => x.Episode == info.Episode) ?? null;

            if (result == null)
            {
                await session.SendAsync(new SQuestEXP { Result = 1 });
                return;
            }
            var state = result
                .QuestState
                .Where(x => x.State == info.Switch && (x.Class == @char.BaseClass || x.Class == HeroClass.End))
                .FirstOrDefault();

            if (state == null)
            {
                await session.SendAsync(new SQuestEXP { Result = 1 });
                return;
            }

            byte rewardCount = 0;
            var ask = new AskInfoDto[5];
            var reward = new RewardInfoDto[5];

            for (var i = 0; i < 5; i++)
            {
                ask[i] = new AskInfoDto();
                reward[i] = new RewardInfoDto();
            }

            if (state.RewardEXP > 0)
            {
                reward[rewardCount].Type = RewardType.Exp;
                reward[rewardCount].Value = state.RewardEXP;
                rewardCount++;
            }

            if (state.RewardGENS > 0)
            {
                reward[rewardCount].Type = RewardType.Point;
                reward[rewardCount].Value = state.RewardGENS;
                rewardCount++;
            }

            if (state.RewardZEN > 0)
            {
                reward[rewardCount].Type = RewardType.Zen;
                reward[rewardCount].Value = state.RewardZEN;
                rewardCount++;
            }

            byte askCount = 0;
            switch (state.Type)
            {
                case AskType.Tutorial:
                    ask[askCount].Type = state.Type;
                    break;
                case AskType.Item:
                    foreach (var it in state.Item)
                    {
                        var item = new Item(ItemNumber.FromTypeIndex(it.Type, it.Index), Options: new { Plus = it.Level });

                        var list = @char.Inventory.FindAllItems(ItemNumber.FromTypeIndex(it.Type, it.Index))
                        .Where(x => x.Plus == it.Level && it.Skill == x.Skill && it.Option == x.Option28 && it.Excellent == x.OptionExe);

                        ask[askCount].Type = state.Type;
                        ask[askCount].ItemInfo = item.GetBytes();
                        ask[askCount].CurrentValue = (uint)list.Count();
                        ask[askCount].Value = it.Count;
                        askCount++;
                    }
                    break;
                case AskType.Monster:
                    var infoM = @char.Quests.GetEpisode<QuestInfoMonster>((int)info.Episode, info.Switch);
                    infoM.Type = state.Type;
                    foreach (var it in state.Monster)
                    {
                        ask[askCount].Type = state.Type;
                        ask[askCount].Index = it.Index;
                        ask[askCount].CurrentValue = infoM.Current;
                        ask[askCount].Value = it.Count;
                        infoM.MonsterClass = it.Index;
                        askCount++;
                    }
                    break;
            }
            await session.SendAsync(new SSendQuestEXPInfo
            {
                dwQuestInfoIndexID = QuestInfoIndex.FromEpisodeSwitch(info.Episode, info.Switch),
                AskCnt = askCount,
                RandRewardCnt = 0,
                RewardCnt = rewardCount,
                Asks = ask,
                Rewards = reward,
            });
        }

        [MessageHandler(typeof(CQuestEXPProgressList))]
        public async Task CQuestEXPProgressList(GSSession session)
        {
            await session.SendAsync(new SQuestEXPProgressList
            {
                QuestInfoIndexID = session.Player.Character.Quests.QuestEXPProgressList()
            });
        }

        [MessageHandler(typeof(CQuestEXPEventItemEPList))]
        public async Task CQuestEXPEventItemEPList(GSSession session)
        {
            var @char = session.Player.Character;
            if(@char.BaseClass == HeroClass.DarkLord || @char.BaseClass == HeroClass.MagicGladiator || @char.BaseClass == HeroClass.RageFighter)
            {
                return;
            }
            var rings = session.Player.Character.Inventory.FindAllItems(ItemNumber.FromTypeIndex(13, 20)).Where(x => x.Plus == 1 || x.Plus == 2);
            if (@char.Level >= 80 || rings.Any())
            {
                return;
            }

            var qs = @char.Quests.GetEpisodeState(1);

            if(qs < 3)
            {
                var q = @char.Quests.GetEpisode<QuestInfoIndex>(1,0);
                await session.SendAsync(new SQuestSwitchListEvent
                {
                    QuestList = new uint[] { q.Index },
                });
            }
        }

        [MessageHandler(typeof(CQuestNPCTalk))]
        public async Task CQuestNPCTalk(GSSession session, CQuestNPCTalk message)
        {
            var npc = session.Player.Window as Monster;
            var list = session.Player.Character.Quests
                .EXPListNPC(npc.Info.Monster)
                .Select(x => new QuestNPCTalkDto { QuestInfoIndex = x, State = x.Switch==0? QuestState.Clear : x.Switch == 65535 ? QuestState.Complete : QuestState.Reg })
                .ToList();

            //list.Add(new QuestNPCTalkDto { QuestInfoIndex = QuestInfoIndex.FromEpisodeSwitch(1, 1), State = QuestState.Clear });
            //list.Add(new QuestNPCTalkDto { QuestInfoIndex = QuestInfoIndex.FromEpisodeSwitch(2, 1), State = QuestState.Clear });
            list.Add(new QuestNPCTalkDto { QuestInfoIndex = QuestInfoIndex.FromEpisodeSwitch(0, 0), State = QuestState.Clear });

            await session.SendAsync(new SQuestNPCTalk
            {
                QuestList = list.ToArray()
            });
        }

        [MessageHandler(typeof(CQuestNPCAccept))]
        public async Task CQuestNPCAccept(GSSession session, CQuestNPCAccept message)
        {
            var @char = session.Player.Character;
            @char.Quests.QuestEXPSetProgress(message.QuestInfoIndex, 1);
            await session.SendAsync(new SQuestNPCAccept { QuestInfoIndex = message.QuestInfoIndex, Result = 0 });
        }

        [MessageHandler(typeof(CCentMove))]
        public async Task CentMove(GSSession session)
        {
            await session.Player.Character.WarpTo(537);
        }

        [MessageHandler(typeof(CCentTestStart))]
        public async Task CentTestStart(GSSession session)
        {
            var @char = session.Player.Character;
            var party = @char.Party;

            var q6 = @char.Quests.GetByIndex(6);
            if (q6 == null || q6.State != QuestState.Complete || @char.MapID != Maps.LabyrinthEntrance)
            {
                return;
            }

            var instance = Quest4th.GetInfo(session.Player);
            instance.State = 1;

            if (party != null)
            {
                var inPlace = party.Members
                    .Where(x => x.Character.MapID == Maps.LabyrinthEntrance)
                    .Where(x => (x.Character.Quests.GetByIndex(6)?.State??QuestState.Clear) == QuestState.Complete)
                    .ToList();

                inPlace.ForEach(x => _=x.Character.WarpTo(540));
            }
            else
            {
                await @char.WarpTo(540);
            }
        }

        [MessageHandler(typeof(CCentBattleStart))]
        public async Task CentBattleStart(GSSession session)
        {
            var instance = Quest4th.GetInfo(session.Player);
            var q = session.Player.Character.Quests.Find(766);
            switch (q.Index)
            {
                case 7:
                    instance.State = 2;
                    break;
                case 8:
                    instance.State = 5;
                    break;
                case 9:
                    instance.State = 8;
                    break;
            }
            
        }
    }
}
