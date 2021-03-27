using MU.Network;
using MU.Network.Game;
using MU.Resources;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Duel
    {
        public bool Free { get; set; }
        public bool Started { get; set; }
        public List<Player> Observers { get; }
        public Player Challenger { get; set; }
        public Player Challenged { get; set; }
        public int ChallengerPoints { get; set; }
        public int ChallengedPoints { get; set; }
        public int ChallengerGate { get; }
        public int ChallengedGate { get; }
        public int ObserversGate { get; }

        public Duel(int challengerGate, int challengedGate, int observersGate)
        {
            Free = true;
            ChallengerGate = challengerGate;
            ChallengedGate = challengedGate;
            ObserversGate = observersGate;
            Observers = new List<Player>();
        }

        private void BroadcastMessage(object msg)
        {
            Challenger.Session.SendAsync(msg).Wait();
            Challenged.Session.SendAsync(msg).Wait();

            Observers.Select(x => x.Session).SendAsync(msg).Wait();
        }

        public void Clear(Player winner, Player loser)
        {
            Free = true;
            Started = false;

            BroadcastMessage(new SDuelBroadcastRound() { Flag = 1 });
            BroadcastMessage(new SDuelBroadcastResult(winner.Character.Name, loser.Character.Name));
            Challenger.Session.SendAsync(new SDuelAnsExit(DuelResults.NoError, (ushort)Challenged.Session.ID, Challenged.Character.Name)).Wait();
            Challenged.Session.SendAsync(new SDuelAnsExit(DuelResults.NoError, (ushort)Challenger.Session.ID, Challenger.Character.Name)).Wait();

            Observers.ForEach(x => x.Character.Spells.ClearBuffByEffect(SkillStates.DuelInterface).Wait());
            Observers.Clear();
            ChallengerPoints = 0;
            ChallengedPoints = 0;
            Challenger.Character.Duel = null;
            Challenged.Character.Duel = null;
            Challenger.Character.CharacterRegen -= Character_CharacterRegen;
            Challenged.Character.CharacterRegen -= Character_CharacterRegen;
            Challenger.Character.CharacterDie -= ChallengerDie;
            Challenged.Character.CharacterDie -= ChallengedDie;
            Challenger.Character.WarpTo(17).Wait();
            Challenged.Character.WarpTo(17).Wait();
            Challenger = null;
            Challenged = null;
        }

        public void Join()
        {
            Started = true;
            Challenger.Character.CharacterDie += ChallengerDie;
            Challenged.Character.CharacterDie += ChallengedDie;
            Challenger.Character.CharacterRegen += Character_CharacterRegen;
            Challenged.Character.CharacterRegen += Character_CharacterRegen;
            Challenger.Character.WarpTo(ChallengerGate).Wait();
            Challenged.Character.WarpTo(ChallengedGate).Wait();
            Challenger.Session.SendAsync(new SDuelAnsDuelInvite(DuelResults.NoError, (ushort)Challenged.Session.ID, Challenged.Character.Name)).Wait();
            Challenged.Session.SendAsync(new SDuelAnsDuelInvite(DuelResults.NoError, (ushort)Challenger.Session.ID, Challenger.Character.Name)).Wait();
            BroadcastScore();
            BroadcastMessage(new SDuelBroadcastRound() { Flag = 0 });
        }

        public void Leave(Player plr)
        {
            if(Challenger == plr)
            {
                Challenged.Character.Spells.SetBuff(SkillStates.DuelMedal, TimeSpan.FromHours(1));
                Clear(Challenged, plr);
            }else if(Challenged == plr)
            {
                Challenger.Character.Spells.SetBuff(SkillStates.DuelMedal, TimeSpan.FromHours(1));
                Clear(Challenger, plr);
            }
        }

        private void Character_CharacterRegen(object sender, EventArgs e)
        {
            Challenger.Character.WarpTo(ChallengerGate).Wait();
            Challenged.Character.WarpTo(ChallengedGate).Wait();
        }

        private void ChallengerDie(object sender, EventArgs e)
        {
            ChallengedPoints++;
            BroadcastScore();
            if (ChallengedPoints == 10)
            {
                Challenged.Character.Spells.SetBuff(SkillStates.DuelMedal, TimeSpan.FromHours(1));
                Clear(Challenged, Challenger);
            }
        }

        private void ChallengedDie(object sender, EventArgs e)
        {
            ChallengerPoints++;
            BroadcastScore();
            if (ChallengerPoints == 10)
            {
                Challenger.Character.Spells.SetBuff(SkillStates.DuelMedal, TimeSpan.FromHours(1));
                Clear(Challenger, Challenged);
            }
        }

        public void BroadcastScore()
        {
            BroadcastMessage(new SDuelBroadcastScore((ushort)Challenger.Session.ID, (ushort)Challenged.Session.ID, (byte)ChallengerPoints, (byte)ChallengedPoints));
        }

        public void BroadcastRound()
        {
            Observers.Select(x => x.Session).SendAsync(new SDuelBroadcastRound() { Flag = 0 }).Wait();
        }

        internal DuelResults AddObserver(Player plr)
        {
            if (Observers.Count == 10)
                return DuelResults.ObserverMax;

            Observers.Select(x => x.Session).SendAsync(new SDuelRoomBroadcastJoin(plr.Character.Name)).Wait();
            Observers.Add(plr);
            plr.Session.SendAsync(new SDuelRoomBroadcastObservers(Observers.Select(x => x.Character.Name).ToArray())).Wait();
            plr.Character.WarpTo(ObserversGate).Wait();
            plr.Character.Spells.SetBuff(SkillStates.DuelInterface, TimeSpan.FromDays(1));
            BroadcastScore();

            return DuelResults.NoError;
        }

        internal DuelResults DelObserver(Player player)
        {
            player.Character.Spells.ClearBuffByEffect(SkillStates.DuelInterface).Wait();
            Observers.Remove(player);
            Observers.Select(x => x.Session).SendAsync(new SDuelRoomBroadcastLeave(player.Character.Name)).Wait();
            player.Character.WarpTo(17).Wait();
            return DuelResults.NoError;
        }
    }
    public class DuelSystem
    {
        private static DuelSystem _instance;
        private Duel[] _duelChannels;

        public DuelSystem()
        {
            _duelChannels = new Duel[4];

            _duelChannels[0] = new Duel(295, 296, 303);
            _duelChannels[1] = new Duel(297, 298, 304);
            _duelChannels[2] = new Duel(299, 300, 305);
            _duelChannels[3] = new Duel(301, 302, 306);
        }

        public static void Initialize()
        {
            if (_instance != null)
                return;

            _instance = new DuelSystem();
        }

        public static bool CreateDuel(Player challenger, Player challenged)
        {
            lock(_instance._duelChannels)
            {
                var res = _instance._duelChannels.FirstOrDefault(x => x.Free);
                if (res == null)
                    return false;

                res.Free = false;
                res.Challenger = challenger;
                res.Challenged = challenged;
                challenger.Character.Duel = res;
                challenged.Character.Duel = res;
                return true;
            }
        }

        public static void NPCTalk(Player plr)
        {

            var msg = new SDuelChannelList(
                    _instance._duelChannels
                    .Select(x => 
                        new DuelChannel(x.Challenger?.Character.Name??"", x.Challenged?.Character.Name??"", x.Started, x.Observers.Count < 4))
                    .ToArray()
                );
        }

        public static DuelResults TryJoinRoom(Player plr, byte room)
        {
            if (room >= 4)
            {
                return DuelResults.InvalidChannelId;
            }

            var duelChannel = _instance._duelChannels[room];
            return duelChannel.AddObserver(plr);
        }

        public static DuelResults LeaveRoom(Player player, byte room)
        {
            if (room >= 4)
            {
                return DuelResults.InvalidChannelId;
            }

            var duelChannel = _instance._duelChannels[room];
            return duelChannel.DelObserver(player);
        }
    }
}
