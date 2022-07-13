using MU.Network.Event;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.Minigames
{
    internal class JeweldryBingoCell
    {
        public JBType Type { get; set; } = JBType.Empty;
        public bool Selected { get; set; }

        public JBType Value => Type | (JBType)(Selected ? 0xF0 : 0x00);
    }
    internal class JeweldryBingoGame
    {
        private List<JeweldryBingoCell> _board = new List<JeweldryBingoCell>();
        private Dictionary<JBType, byte> _abailableJewels = new Dictionary<JBType, byte>();
        private List<JBType> _pool = new List<JBType>();
        private byte[] _matching = new byte[12];
        public JBState State { get; set; }
        public byte Box { get; set; }
        public ushort LuckyScore { get; set; }
        public ushort NormalScore { get; set; }
        public ushort JewelryScore { get; set; }

        public JeweldryBingoGame(Player plr)
        {
            for (int i = 0; i < 25; i++)
            {
                _board.Add(new JeweldryBingoCell());
            }

            _board[12].Selected = true;

            for (var i = 0; i < 6; i++)
            {
                _abailableJewels.Add((JBType)i, 4);
            }

            State = JBState.Open;
        }

        public JBType[] GetGrid()
        {
            return _board.Select(x => x.Value).ToArray();
        }

        public byte[] AvailableJewels => _abailableJewels.Values.ToArray();

        public JBType CurrentJewel => _pool.First();
        public byte LeftJewels => (byte)_pool.Count;

        internal void Place(byte slot, JBType jewelType)
        {
            if (slot == 12 || slot >= _board.Count)
                return;

            if (_board[slot].Type != JBType.Empty)
                return;

            if (!_abailableJewels.ContainsKey(jewelType) || _abailableJewels[jewelType] == 0)
                return;

            _abailableJewels[jewelType]--;
            _board[slot].Type = jewelType;
        }

        internal void AutoPlace()
        {
            while (AvailableJewels.Sum(x => x) > 0)
            {
                var abilableTypes = _abailableJewels.Where(x => x.Value > 0).Select(x => x.Key);
                var placeType = abilableTypes.ElementAt(Program.RandomProvider(abilableTypes.Count()));

                Place((byte)Program.RandomProvider(25), placeType);
            }
        }

        internal void SelectBox(byte box)
        {
            Box = box;
            _abailableJewels.Clear();
            for (var i = 0; i < 14;)
            {
                var jewelType = (JBType)Program.RandomProvider(6);
                var contains = _abailableJewels.ContainsKey(jewelType);
                if (!contains)
                {
                    _abailableJewels.Add(jewelType, 1);
                    _pool.Add(jewelType);
                    i++;
                }
                else if (contains && _abailableJewels[jewelType] < 4)
                {
                    _abailableJewels[jewelType]++;
                    _pool.Add(jewelType);
                    i++;
                }
            }
        }

        internal byte[] GetMatching()
        {
            //horizontal
            for (var i = 0; i < 5; i++)
            {
                var bingo = true;
                for (var j = 0; j < 5; j++)
                    bingo &= _board[i + j * 5].Selected;

                if (bingo && _matching[i] != 1)
                {
                    _matching[i] = 1;

                    JewelryScore -= 180;
                    if (i == 2)
                    {
                        LuckyScore += 312;
                    }
                    else
                    {
                        NormalScore += 240;
                        JewelryScore -= 45;
                    }
                }
            }

            //Vertical
            for (var i = 0; i < 5; i++)
            {
                var bingo = true;
                for (var j = 0; j < 5; j++)
                    bingo &= _board[i * 5 + j].Selected;

                if (bingo && _matching[i + 5] != 1)
                {
                    _matching[i + 5] = 1;

                    JewelryScore -= 180;
                    if (i == 2)
                    {
                        LuckyScore += 312;
                    }
                    else
                    {
                        NormalScore += 240;
                        JewelryScore -= 45;
                    }
                }
            }

            // Diagonal 1
            {
                var bingo = true;
                for (var j = 0; j < 5; j++)
                    bingo &= _board[j * 5 + j].Selected;

                if (bingo && _matching[10] != 1)
                {
                    _matching[10] = 1;
                    LuckyScore += 312;
                    JewelryScore -= 180;
                }
            }

            // Diagonal 2
            {
                var bingo = true;
                for (var j = 0; j < 5; j++)
                    bingo &= _board[j * 5 + (4 - j)].Selected;

                if (bingo && _matching[11] != 1)
                {
                    _matching[11] = 1;
                    LuckyScore += 312;
                    JewelryScore -= 180;
                }
            }

            return _matching;
        }

        internal void SelectJewel(byte slot, JBType jewelType)
        {
            if (slot >= _board.Count || jewelType != CurrentJewel)
                return;

            _pool.RemoveAt(0);
            JewelryScore += 45;

            _board[slot].Selected = true;
        }

        internal Item GetReward()
        {
            var totalScore = LuckyScore + NormalScore + JewelryScore;
            if (totalScore < 700)
            {
                return new Item(7576);
            }
            else if (totalScore < 900)
            {
                return new Item(7577);
            }
            else if (totalScore < 1000)
            {
                return new Item(7578);
            }
            return new Item(7579);
        }
    }
    internal class JeweldryBingo : MiniGame<JeweldryBingoGame>
    {
        private Dictionary<Player, JeweldryBingoGame> _games = new Dictionary<Player, JeweldryBingoGame>();

        public JeweldryBingo(string file) : base(file)
        { }

        public override BannerType GetBanner() => BannerType.JeweldryBingo;
    }
}
