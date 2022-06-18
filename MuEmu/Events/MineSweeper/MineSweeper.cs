using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MuEmu.Events.MineSweeper
{
    internal class MineSweeperCell
    {
        private MineSweeperGame _game;
        private List<MineSweeperCell> EmptyCell = new List<MineSweeperCell>();
        private int _proximity = 0xff;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Proximity => Mark?0xFE:Mine?9:GetProximity();
        public bool Mine { get; set; }
        public bool Hide { get; set; }
        public bool Mark { get; set; }
        public MineSweeperCell(MineSweeperGame game, int index)
        {
            _game = game;
            X = index % 8;
            Y = index / 8;
            Hide = true;
        }

        private int GetProximity()
        {
            if (Mine)
                return 0;

            if(_proximity != 0xff)
                return _proximity;

            _proximity = 0;
            for(var i = 0; i < 9; i++)
            {
                var x = (X + (i % 3) - 1);
                var y = (Y + (i / 3) - 1);
                if (x >= 0 && y >= 0 && x < 8 && y < 6)
                {
                    var index = x + y * 8;
                    if (_game._board[index].Mine)
                        _proximity++;
                    else
                        EmptyCell.Add(_game._board[index]);
                }
            }
            return _proximity;
        }
        public IEnumerable<MineSweeperCell> PropagateReveal()
        {
            List<MineSweeperCell> result = new List<MineSweeperCell>();

            if (!Hide)
                return result;

            Hide = false;
            result.Add(this);
            if(Proximity==0)
            {
                result.AddRange(EmptyCell.Where(x => x.Proximity < 9).SelectMany(x => x.PropagateReveal()));
            }
            else if(Proximity<9)
            {
                result.AddRange(EmptyCell.Where(x => x.Proximity == 0).SelectMany(x => x.PropagateReveal()));
            }

            return result;
        }

        public ushort Cell => (ushort)((Proximity << 8) + (X + Y * 8));
        public ushort Score => (ushort)(Proximity * 10);
    }
    internal class MineSweeperGame
    {
        private Player _player;
        internal List<MineSweeperCell> _board = new List<MineSweeperCell>();
        private int _totalBombs = 0;

        public bool Finished { get; private set; }
        public bool Losed { get; private set; }
        public ushort Score { get; internal set; }
        public byte RemainMines => (byte)(_totalBombs - _board.Count(x => x.Mark));
        public ushort Correct => (ushort)_board.Where(x => x.Mark && x.Mine).Count();
        public ushort Incorrect => (ushort)_board.Where(x => x.Mark && !x.Mine).Count();
        public ushort TotalScore => (ushort)(Math.Max(Score + (Correct * 50) + (Incorrect * -20) + (Losed ? -50 : 500),0));

        public IEnumerable<MineSweeperCell> FailedBomb => _board.Where(x => x.Mark && !x.Mine);

        public MineSweeperGame(Player plr)
        {
            _player = plr;
            for(var i = 0; i < (6 * 8); i++)
            {
                _board.Add(new MineSweeperCell(this, i));
            }

            for(_totalBombs = 0; _totalBombs < 11; )
            {
                var pos = Program.RandomProvider(6 * 8);
                if(!_board[pos].Mine)
                {
                    _board[pos].Mine = true;
                    _totalBombs++;
                }
            }
        }

        public IEnumerable<ushort> GetBoard()
        {
            for(var i = 0; i < _board.Count; i++)
            {
                if (_board[i].Hide)
                    continue;

                yield return _board[i].Cell;
            }
        }

        internal IEnumerable<ushort> Reveal(byte cell)
        {
            if(_board[cell].Mine)
            {
                return Array.Empty<ushort>();
            }

            var reveal = _board[cell].PropagateReveal();
            Score += (ushort)reveal.Sum(x => x.Score);

            return reveal.Select(x => x.Cell);
        }

        internal byte Mark(byte cell)
        {
            _board[cell].Mark = !_board[cell].Mark;
            if(Correct == _totalBombs && Incorrect==0)
            {
                Finish();
            }
            return (byte)(_board[cell].Mark?1:0);
        }

        internal ushort Finish(bool lose = false)
        {
            Finished = true;
            Losed = lose;
            _board.ForEach(x => x.Hide = false);
            return (ushort)(Score + (Correct * 50) + (Incorrect * -20) + (lose ? -50 : 500));
        }

        internal bool IsClear()
        {
            var hideCount = _board.Where(x => x.Hide).Count();
            if (hideCount == _totalBombs)
                Finish();

            if (Finished)
                return true;

            return false;
        }

        internal Item GetReward()
        {
            if (TotalScore <= 150)
            {
                return new Item(7542);
            }
            if (TotalScore <= 850)
            {
                return new Item(7543);
            }
            return new Item(7544);
        }
    }
    internal class MineSweeper : Event
    {
        private Dictionary<Player, MineSweeperGame> _games = new Dictionary<Player, MineSweeperGame>();
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

            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }

        public MineSweeperGame GetGame(Player plr)
        {
            if(!_games.ContainsKey(plr))
            {
                _games.Add(plr, new MineSweeperGame(plr));
            }

            return _games[plr];
        }

        internal void Clear(Player player)
        {
            _games.Remove(player);
        }
    }
}
