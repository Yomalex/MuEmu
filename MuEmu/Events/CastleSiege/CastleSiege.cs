using MU.Network.CastleSiege;
using MU.Resources;
using MuEmu.Monsters;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.CastleSiege
{
    internal enum SiegeStates
    {
        None = -1,
        Idle1,
        RegisteSiege,
        Idle2,
        RegisteMark,
        Idle3,
        Notify,
        ReadySiege,
        StartSiege,
        EndSiege,
        EndCycle,
    }

    internal class CastleSiegeState : StateMachine<SiegeStates>
    {
        private CastleSiege main;
        private MapInfo map;
        internal List<Monster> gates = new List<Monster>();
        internal List<Monster> guardianStatues = new List<Monster>();
        internal Monster sw1;
        internal Monster sw2;
        internal Monster crown;
        private Dictionary<SiegeStates, TimeSpan> _periods = new Dictionary<SiegeStates, TimeSpan>
        {
            {SiegeStates.Idle1,         TimeSpan.Zero },
            {SiegeStates.RegisteSiege,  TimeSpan.FromMinutes(2879) },
            {SiegeStates.Idle2,         TimeSpan.FromMinutes(1) },
            {SiegeStates.RegisteMark,   TimeSpan.FromHours(24) },
            {SiegeStates.Idle3,         TimeSpan.FromHours(36) },
            {SiegeStates.Notify,        TimeSpan.FromHours(7) },
            {SiegeStates.ReadySiege,    TimeSpan.FromMinutes(2) },//FromHours
            {SiegeStates.StartSiege,    TimeSpan.FromHours(2) },
            {SiegeStates.EndSiege,      TimeSpan.FromHours(1) },
            {SiegeStates.EndCycle,      TimeSpan.Zero },
        };
        private Dictionary<SiegeStates, int> _totals = new Dictionary<SiegeStates, int>();

        public CastleSiegeState(CastleSiege mng)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CastleSiegeState));
            map = ResourceCache.Instance.GetMaps()[Maps.ValleyofLoren];
            map.MonsterAdd += Map_MonsterAdd;
            main = mng;
            ChangeState(SiegeStates.None);

            var total = 0;
            foreach(var p in _periods)
            {
                _totals.Add(p.Key, total);
                total += (int)p.Value.TotalSeconds;
            }
        }

        private void Map_MonsterAdd(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            switch(mob.Info.Monster)
            {
                case 277:
                    gates.Add(mob);
                    mob.Type = ObjectType.Gate;
                    mob.Active = false;
                    mob.Die += Mob_Die;
                    break;
                case 283:
                    guardianStatues.Add(mob);
                    mob.Type = ObjectType.Gate;
                    mob.Active = false;
                    mob.Die += Mob_Die;
                    break;
                case 216:
                    crown = mob;
                    break;
                case 217:
                    sw1 = mob;
                    break;
                case 218:
                    sw2 = mob;
                    break;
            }
        }

        private void Mob_Die(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }

        public DateTime NextSiegePeriod { get; set; }

        public override void Initialize()
        {
            GetNextSiegePeriod();
        }

        public DateTime GetNextSiegePeriod()
        {
            var now = DateTime.Now;
            DateTime firstSunday;
            do
            {
                var firstDay = new DateTime(now.Year, now.Month, 1);
                var addDays = (7 - (int)firstDay.DayOfWeek) % 7;
                firstSunday = firstDay.AddDays(addDays);
                now = now.AddMonths(1);
            } while (firstSunday < DateTime.Now && DateTime.Now - firstSunday > TimeSpan.FromDays(7));

            NextSiegePeriod = firstSunday.AddSeconds(_totals[SiegeStates.StartSiege]);

            if (DateTime.Now > firstSunday)
            {
                var sub = DateTime.Now - firstSunday;

                var currentPeriod = _totals.Last(x => sub.TotalSeconds > x.Value);

                var lastState = currentPeriod.Key;
                var duration = _periods[lastState];
                var trans = DateTime.Now - firstSunday.AddSeconds(currentPeriod.Value);
                var left = duration - trans;

                ChangeState(lastState);
                if (lastState + 1 < SiegeStates.EndCycle)
                    Trigger(lastState + 1, left);
                else
                    Trigger(SiegeStates.Idle1, left);
            }
            else
            {
                Trigger(SiegeStates.RegisteSiege, firstSunday - DateTime.Now);
            }

            return NextSiegePeriod;
        }

        public override void OnTransition(SiegeStates NextState)
        {
            if (NextState == SiegeStates.None)
                return;

            var nextState = NextState + 1;
            if (nextState > SiegeStates.EndCycle)
                nextState = SiegeStates.Idle1;

            Trigger(nextState, _periods[NextState]);
            _logger.Information("Sync State: {0}, NextState {2} at {1}", NextState, DateTime.Now.Add(_periods[NextState]), nextState);

            switch(NextState)
            {
                case SiegeStates.None:
                    break;
                case SiegeStates.Idle1:
                    break;
                case SiegeStates.RegisteSiege:
                    break;
                case SiegeStates.Idle2:
                    break;
                case SiegeStates.RegisteMark:
                    break;
                case SiegeStates.Idle3:
                    break;
                case SiegeStates.Notify:
                    break;
                case SiegeStates.ReadySiege:
                    break;
                case SiegeStates.StartSiege:
                    {
                        var curSkill = SkillStates.CastleSiegeAttackOne;
                        byte side = 0;
                        foreach (var g in main.AttackGuild.Take(3))
                        {
                            var membersOnline = g.Guild.Members.Where(x => x.Player != null).ToList();
                            membersOnline.ForEach(x => x.Player.Character.Spells.SetBuff(curSkill, TimeLeft));
                            _ = membersOnline.Select(x => x.Player.Session).SendAsync(new SCastleSiegeNotifyStart { State = 1 });
                            _ = membersOnline.Select(x => x.Player.Session).SendAsync(new SJoinSideNotify { Side = side++ });
                            curSkill++;
                        }
                        gates.ForEach(x => x.Active = true);
                        guardianStatues.ForEach(x => x.Active = true);
                        TimeLeftSend();
                    }
                    break;
                case SiegeStates.EndSiege:
                    break;
                case SiegeStates.EndCycle:
                    break;
            }
        }

        internal void NotifyAllUserSide()
        {
            var list = main.AttackGuild
                .Select(x => x.Guild).ToList();

            list.ForEach(x => x.Union.SelectMany(x => x.Members).Select(x => x.Player.Session).SendAsync(new SJoinSideNotify { Side = 1 }));
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case SiegeStates.RegisteSiege:
                    RegisteSiege();
                    break;
                case SiegeStates.RegisteMark:
                    RegisteMark();
                    break;
                case SiegeStates.Idle3:
                    Idle3();
                    break;
                case SiegeStates.Notify:
                    Notify();
                    break;
                case SiegeStates.ReadySiege:
                    ReadySiege();
                    break;
                case SiegeStates.StartSiege:
                    StartSiege();
                    break;
            }
            base.Update();
        }

        private void RegisteSiege()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 1800) == 0;
            if (!notify)
                return;

            _=Program.GlobalAnoucement(ServerMessages.GetMessage(MU.Resources.Messages.CS_RegisteNotify));
        }

        private void RegisteMark()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 1800) == 0;
            if (!notify)
                return;

            _ = Program.GlobalAnoucement(ServerMessages.GetMessage(MU.Resources.Messages.CS_RegisteMarkNotify));
        }

        private void Idle3()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 7200) == 0;
            if (!notify)
                return;

            _ = Program.GlobalAnoucement(ServerMessages.GetMessage(MU.Resources.Messages.CS_Idle3));
        }

        private void Notify()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 1800) == 0;
            if (!notify)
                return;

            _ = Program.GlobalAnoucement(ServerMessages.GetMessage(MU.Resources.Messages.CS_Notify));
        }

        private void ReadySiege()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 1800) == 0;
            seconds = (int)TimeLeft.TotalSeconds;
            var lastNotify = seconds <= 300 && (seconds % 60) == 0;
            if (notify)
            {
                _ = Program.GlobalAnoucement(ServerMessages.GetMessage(Messages.CS_Notify));
            }
            if(lastNotify)
            {
                _ = Program.GlobalAnoucement(ServerMessages.GetMessage(Messages.CS_LastNotify, seconds/60));
            }
        }

        private void NotifySwitch(Player plr, Monster sw, Player owner, byte state)
        {
            _ = plr.Session.SendAsync(new SCastleSiegeNotifySwitch
            {
                Index = sw.Index,
                UserIndex = (owner?.ID ?? 0xffff),
                State = state
            });
        }

        private void NotifySwitchInfo(Monster sw, Player plr)
        {
            var msg = new SCastleSiegeNotifySwitchInfo
            {
                GuildName = plr?.Character.Guild.Name??"",
                UserName = plr?.Character.Name??"",
                Index = sw.Index,
                Side = 0,
                State = (byte)(plr != null ? 1 : 0),
            };
            if (plr != null)
            {
                _ = plr.Session.SendAsync(msg);
            }
            _ = sw.ViewPort.Select(x => x.Session).SendAsync(msg);
        }

        private void NotifyCrownState(byte state)
        {
            var msg = new SCastleSiegeNotifyCrownState
            {
                State = state
            };

            if (state == 0)
                crown.Spells.SetBuff(SkillStates.CastleSiegeCrownState, TimeLeft);
            else
                crown.Spells.ClearBuffByEffect(SkillStates.CastleSiegeCrownState).Wait();

            _ = crown.ViewPort.SendAsync(msg);
        }

        private void StartSiege()
        {
            var seconds = (int)Time.TotalSeconds;
            var notify = (seconds % 180) == 0;
            seconds = (int)TimeLeft.TotalSeconds;
            if (notify)
            {
                TimeLeftSend();
            }
            if(seconds % 1800 == 0)
            {
                _ = Program.GlobalAnoucement(ServerMessages.GetMessage(Messages.CS_Notify));
            }
            if(seconds < 180 && (seconds % 60) == 0)
            {
                TimeLeftSend();
            }
            if((seconds % 3) == 0)
            {
                MinimapUpdate();
            }

            if (main.Switch1 != null)
            {
                var sw = main.Switch1.Window as Monster;
                if(sw.Position.Substract(main.Switch1.Character.Position).LengthSquared() > 3)
                {
                    main.Switch1 = null;
                    NotifyCrownState(1);
                }
            }

            if (main.Switch2 != null)
            {
                var sw = main.Switch2.Window as Monster;
                if (sw.Position.Substract(main.Switch2.Character.Position).LengthSquared() > 3)
                {
                    main.Switch2 = null;
                    NotifyCrownState(1);
                }
            }

            NotifySwitchInfo(sw1, main.Switch1);
            NotifySwitchInfo(sw2, main.Switch2);

            if(main.Switch1 != null && main.Switch2 != null)
            {
                if(main.Switch1.Character.Guild.Union[0] == main.Switch2.Character.Guild.Union[0])
                {
                    NotifyCrownState(0);
                }
                else
                {
                    NotifyCrownState(1);
                }
            }
        }

        private void MinimapUpdate()
        {
            var list = new List<Guild>();
            list.Add(main.Owner);
            list.AddRange(main.AttackGuild.Select(x => x.Guild));
            list.RemoveAll(x => x == null);

            foreach(var g in list)
            {
                var master = g.Union.FirstOrDefault()?.Master??g.Master;
                var members = g.Union.SelectMany(x => x.Members).Where(x => x.Player != null);

                if (master == null)
                    continue;

                _ = master.Player.Session.SendAsync(new SCastleSiegeMinimapData
                {
                    List = members.Select(x => new CastleSiegeMinimapDta
                    {
                        X = (byte)x.Player.Character.Position.X,
                        Y = (byte)x.Player.Character.Position.Y
                    }).ToArray()
                });
            }

            if(main.Owner != null)
            {
                var listMob = new List<CastleSiegeMinimapNPCDta>();
                listMob.AddRange(gates.Select(x => new CastleSiegeMinimapNPCDta { Type = 0, X = (byte)x.Position.X, Y = (byte)x.Position.Y }));
                listMob.AddRange(guardianStatues.Select(x => new CastleSiegeMinimapNPCDta { Type = 0, X = (byte)x.Position.X, Y = (byte)x.Position.Y }));

                _ = main.Owner.Master.Player.Session.SendAsync(new SCastleSiegeMinimapNPCData
                {
                    List = listMob.ToArray()
                });
            }
        }

        private void TimeLeftSend()
        {
            _ = Program.NoEventMapSendAsync(new SLeftTimeAlarm
            {
                Hour = (byte)TimeLeft.Hours,
                Minute = (byte)TimeLeft.Minutes
            });
        }
    }

    public class AttackGuildInfo
    {
        public int Marks { get; set; }
        public Guild Guild { get; set; }
        public bool GiveUp { get; set; }
    }
    public class CastleSiege : Event
    {
        private CastleSiegeState subStates;
        internal SiegeStates State => subStates.CurrentState;

        public int StageTimeLeft => (int)subStates.TimeLeft.TotalSeconds;
        public DateTimeOffset StateStart => subStates.Start;
        public DateTimeOffset StateEnd => subStates.End;
        public DateTime SiegeExpectedPeriod => subStates.NextSiegePeriod;

        public Guild Owner { get; internal set; }
        public List<AttackGuildInfo> AttackGuild { get; set; } = new List<AttackGuildInfo>();
        public Player Switch1 { get; set; }
        public Player Switch2 { get; set; }

        public CastleSiege()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CastleSiegeState));
            subStates = new CastleSiegeState(this);
        }

        public override void Initialize()
        {
            base.Initialize();
            subStates.Initialize();

            Trigger(EventState.Open);
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    subStates.Trigger(SiegeStates.Idle1);
                    break;
            }
        }

        public override void Update()
        {
            subStates.Update();
            base.Update();
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }

        internal void CrownTalk(Player player)
        {
            throw new NotImplementedException();
        }

        internal void CrownSwitchTalk(Monster mob, Player player)
        {
            if (Switch1?.Session.Closed??false)
                Switch1 = null;
            if (Switch2?.Session.Closed??false)
                Switch2 = null;

            if (State != SiegeStates.StartSiege)
                return;

            if (subStates.guardianStatues.Any(x => x.Active))
                return;

            switch (mob.Info.Monster)
            {
                case 217:
                    _ = player.Session.SendAsync(new SCastleSiegeNotifySwitch
                    {
                        Index = mob.Index,
                        UserIndex = (Switch1?.ID??0xffff),
                        State = (byte)(Switch1 != null ? 2 : 1)
                    });
                    if (Switch1 == null)
                    {
                        Switch1 = player;
                        player.Window = mob;
                        player.Character.CharacterDie += Character_CharacterDie;
                    }
                    break;
                case 218:
                    _ = player.Session.SendAsync(new SCastleSiegeNotifySwitch
                    {
                        Index = mob.Index,
                        UserIndex = (Switch2?.ID ?? 0xffff),
                        State = (byte)(Switch2 != null ? 2 : 1)
                    });
                    if (Switch2 == null)
                    {
                        Switch2 = player;
                        player.Window = mob;
                        player.Character.CharacterDie += Character_CharacterDie;
                    }
                    break;
            }
        }

        private void Character_CharacterDie(object sender, EventArgs e)
        {
            var @char = sender as Character;
            @char.CharacterDie -= Character_CharacterDie;
            if (Switch1 == @char.Player)
                Switch1 = null;
            if (Switch2 == @char.Player)
                Switch2 = null;
        }

        internal void RegisteAttackGuild(Player player)
        {
            var g = player.Character.Guild.Union[0];
            AttackGuild.Add(new AttackGuildInfo { GiveUp = false, Guild = g, Marks = 0 });
            _ = player.Session.SendAsync(new SGuildRegiste { Result = 1, GuildName = g.Name });
        }

        internal int RegisteMark(Guild guild, byte durability)
        {
            var info = AttackGuild.FirstOrDefault(x => x.Guild == guild);
            if (info == null)
                return 0;

            info.Marks += durability;
            return info.Marks;
        }
    }
}
