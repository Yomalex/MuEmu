using MuEmu.Monsters;
using MuEmu.Network.Event;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu.Events.BloodCastle
{
    public class BloodCastle
    {
        private readonly ILogger Logger;

        private MapInfo _map;
        private BloodCastles _manager;
        private BCState _state;
        internal TimeSpan _nextStateIn;

        public Maps MapID { get; set; }

        internal BCState State { get => _state;
            set
            {
                if(_state != value)
                {
                    _state = value;
                    if (_manager.State != value)
                        Logger.Information("State: {0} -> {1}", _state, value);

                    OnTransition(value);
                }
            }
        }

        public const int MaxPlayers = 10;
        public int Bridge { get; }
        public List<Player> Players { get; set; }
        public List<Monster> Monsters { get; set; }
        public Monster Gate { get; set; }
        public Monster Statue { get; set; }
        public Monster Archangel { get; set; }
        public ushort MonsterCount { get; set; }
        public ushort MonsterKill { get; set; }
        public ushort BossCount { get; set; }
        public ushort BossKill { get; set; }
        
        public BloodCastle(BloodCastles mng, int bridge)
        {
            Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BloodCastle) + " " + (bridge+1));

            _manager = mng;
            Bridge = bridge;
            Players = new List<Player>();
            Monsters = new List<Monster>();
            MapID = (bridge < 8 ? (Maps)((int)Maps.BloodCastle1 + bridge) : Maps.BloodCastle8);
            _map = ResourceCache.Instance.GetMaps()[MapID];
            _map.PlayerLeaves += Remove;
            _map.MonsterAdd += AddMonster;
        }

        public bool TryAdd(Player plr)
        {
            if (_state != BCState.Open)
                return false;

            if (Players.Count >= MaxPlayers)
                return false;

            if(Bridge < 6)
            {
                plr.Character.WarpTo(66 + Bridge);
            }else if(Bridge == 6)
            {
                plr.Character.WarpTo(80);
            }
            else if (Bridge == 7)
            {
                plr.Character.WarpTo(271);
            }else
            {
                return false;
            }

            Players.Add(plr);

            return true;
        }

        private void Remove(object sender, EventArgs e)
        {
            var plr = sender as Player;
            Players.Remove(plr);

            if (Players.Count == 0 && _state == BCState.Started)
                State = BCState.Close;
        }

        private void AddMonster(object sender, EventArgs e)
        {
            var mons = sender as Monster;
            mons.Active = false;

            switch (mons.Info.Monster)
            {
                // Spirit Sorcerer
                case 89:
                case 95:
                case 112:
                case 118:
                case 124:
                case 130:
                case 143:
                case 433:
                    mons.Die += SorcererDead;
                    break;
                // Castle Gate
                case 131:
                    mons.Die += GateDead;
                    Logger.Information("Gate Added");
                    Gate = mons;
                    break;
                // Statue of Saint
                case 132:
                case 133:
                case 134:
                    mons.Die += StatueDead;
                    Logger.Information("Statue Added");
                    Statue = mons;
                    break;
                case 232:
                    mons.Die += StatueDead;
                    Logger.Information("Archangel Added");
                    mons.Active = true;
                    Archangel = mons;
                    break;
                default:
                    mons.Die += MonsterDead;
                    Monsters.Add(mons);
                    break;
            }
        }

        private void MonsterDead(object sender, EventArgs e)
        {
            MonsterKill++;
            //if(MonsterKill == MonsterCount)
            //{
            //    SendAsync(new SSetMapAttribute(0, 4, 1, new MapRectDto[] { new MapRectDto { StartX = 13, StartY = 15, EndX = 15, EndY = 23 } }));
            //}
        }

        private void SorcererDead(object sender, EventArgs e)
        {
            BossKill++;
        }

        private void GateDead(object sender, EventArgs e)
        {

        }

        private void StatueDead(object sender, EventArgs e)
        {

        }

        private void Clear()
        {
            foreach(var mons in Monsters)
            {
                mons.Active = false;
            }

            //if(Gate!=null)
            //    Gate.Active = true;

            //if (Statue != null)
            //    Statue.Active = true;

            if (Archangel != null)
                Archangel.Active = true;

            SendAsync(new SDevilSquareSet(DevilSquareState.Quit));
        }

        public void Update()
        {
            switch(_state)
            {
                case BCState.Started:
                    if(MonsterKill < MonsterCount)
                    {
                        SendAsync(new SBloodCastleState(1, (ushort)_nextStateIn.TotalSeconds, MonsterCount, MonsterKill, 0xffff, 1));
                    }
                    else if(BossKill < BossCount)
                    {
                        SendAsync(new SBloodCastleState(4, (ushort)_nextStateIn.TotalSeconds, BossCount, BossKill, 0xffff, 1));
                    }else
                    {

                    }

                    if (_nextStateIn <= TimeSpan.Zero)
                        State = BCState.Close;
                    break;
                case BCState.Ended:
                    if (_nextStateIn <= TimeSpan.Zero)
                        State = BCState.Close;
                    break;
            }

            _nextStateIn -= TimeSpan.FromSeconds(1);
        }

        private void OnTransition(BCState next)
        {
            switch(next)
            {
                case BCState.Started:
                    if(Players.Count == 0)
                    {
                        State = BCState.Close;
                        return;
                    }
                    MonsterCount = (ushort)(Players.Count * 40);
                    BossCount = (ushort)(10 * Players.Count);
                    _nextStateIn = TimeSpan.FromSeconds(600);
                    foreach (var mons in Monsters)
                    {
                        mons.State = ObjectState.Regen;
                        mons.Active = true;
                    }
                    SendAsync(new SBloodCastleState(0, (ushort)_nextStateIn.TotalSeconds, 0, 0, 0xffff, 1));
                    SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.GoAheadBC));
                    // Bridge Start
                    //SendAsync(new SSetMapAttribute(0, 4, 1, new MapRectDto[] { new MapRectDto { StartX = 13, StartY = 15, EndX = 15, EndY = 23 } }));
                    _map.RemoveAttribute(MapAttributes.NoWalk, new System.Drawing.Rectangle(13, 15, 2, 8));
                    break;
                case BCState.Ended:
                    Clear();
                    break;
                case BCState.Close:
                    _map.AddAttribute(MapAttributes.NoWalk, new System.Drawing.Rectangle(13, 15, 2, 8));

                    foreach (var plr in Players)
                        plr.Character.WarpTo(22);
                    break;
            }
        }

        private async Task SendAsync(object message)
        {
            foreach(var plr in Players)
            {
                await plr.Session.SendAsync(message);
            }
        }
    }
}
