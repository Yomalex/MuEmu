using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using MU.Resources.XML;
using MuEmu.Resources;

namespace MuEmu.Events.Minigames
{
    public class MiniGame<T> : Event
    {
        private Dictionary<Player, T> _games = new Dictionary<Player, T>();
        private MiniGameDto _config;

        public MiniGame(string file)
        {
            LoadConfig(file);
        }

        public void LoadConfig(string file)
        {
            _config = ResourceLoader.XmlLoader<MiniGameDto>(file);
            _openTime = TimeSpan.FromSeconds(_config.OpenTime);
            _closedTime = TimeSpan.FromSeconds(_config.ClosedTime);
        }
        public override Item GetItem(ushort mobLevel, Maps map)
        {
            if (Program.RandomProvider(10000) < _config.DropRate * 100)
                return new Item(_config.ItemDrop);

            return null;
        }

        public override void Initialize()
        {
            base.Initialize();

            Trigger(EventState.Open, TimeSpan.FromSeconds(_config.NoneTime));
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

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnTransition(EventState NextState)
        {
            switch (NextState)
            {
                case EventState.Open:
                    Trigger(EventState.Closed, _openTime);
                    break;
                case EventState.Closed:
                    Trigger(EventState.Open, _closedTime);
                    break;
            }
        }
        public T GetGame(Player plr)
        {
            if (!_games.ContainsKey(plr))
            {
                var obj = (T)Activator.CreateInstance(typeof(T), plr);
                _games.Add(plr, obj);
                plr.OnStatusChange += Plr_OnStatusChange;
            }

            return _games[plr];
        }

        private void Plr_OnStatusChange(object sender, EventArgs e)
        {
            var plr = sender as Player;
            ClearGame(plr);
        }

        public void ClearGame(Player plr)
        {
            plr.OnStatusChange -= Plr_OnStatusChange;
            _games.Remove(plr);
        }
    }
}
