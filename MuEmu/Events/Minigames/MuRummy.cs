using MU.Network.Event;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Util;

namespace MuEmu.Events.Minigames
{
    public class MuRummyGame
    {
        private Player _player;
        private List<MuRummyCardInfo> _specialCardDeck = new List<MuRummyCardInfo>();
        private List<MuRummyCardInfo> _cardDeck = new List<MuRummyCardInfo>();
        private List<MuRummyCardInfo> _playCard = new List<MuRummyCardInfo>();

        public byte State { get; internal set; }
        public byte CardCount => (byte)_cardDeck.Count;
        public ushort Score { get; internal set; }
        public byte SpecialCardCount => (byte)_specialCardDeck.Count;
        public byte Type { get; internal set; }
        public MuRummyCardInfo SpecialCard => _playCard[8];

        public MuRummyGame(Player player)
        {
            _player = player;
        }

        internal MuRummyCardInfo[] GetCardInfo()
        {
            return _playCard.Take(5).Append(SpecialCard).ToArray();
        }

        internal MuRummyCardInfo[] GetPlayedCard()
        {
            return _playCard.Skip(5).Take(3).Append(_playCard[9]).ToArray();
        }

        internal byte[] GetSlotStatus()
        {
            return new byte[] {
            0, 0, 0, 0, 0,
            0, 0, 0, (byte)(1-Type), (byte)(1-Type),
            };
        }

        internal void Start(byte type)
        {
            State = 1;
            Type = type;
            var list = new List<byte>();
            for(var i = 0; i < 24; i++)
            {
                list.Add((byte)i);
            }

            _cardDeck.Clear();
            while (list.Count > 0)
            {
                var position = Program.RandomProvider(list.Count);
                var element = list[position];
                list.RemoveAt(position);
                _cardDeck.Add(new MuRummyCardInfo
                {
                    Color = (byte)(element / 8 + 1),
                    Number = (byte)(element % 8 + 1),
                    Slot = 0
                });
            }

            if(Type == 1)
            {
                while (_specialCardDeck.Count < 3)
                {
                    _specialCardDeck.Add(new MuRummyCardInfo
                    {
                        Color = 4,
                        Number = (byte)(Program.RandomProvider(11) + 1),
                        Slot = 8,
                    });
                }
            }            
            
            for(var i = 0; i < 10; i++)
            {
                _playCard.Add(new MuRummyCardInfo { Slot = (byte)i });
            }
        }

        internal MuRummyCardInfo MovePlayCard(byte from, byte to)
        {
            var pcf = _playCard[from];
            var pct = _playCard[to];
            var empty = new MuRummyCardInfo { Slot = from, Color = 0xff, Number = 0xff};
            if(Type == 0 && from >= 7)
            {
                return empty;
            }
            if(Type == 1 && pcf.Color == 4 && (pcf.Number > 9 && to != 9))
            {
                return empty;
            }

            _playCard[to] = pcf;
            _playCard[from] = pct;
            return pcf;
        }

        internal MuRummyCardInfo ThrowPlayCard(byte from)
        {
            var pcf = _playCard[from];
            _playCard[from] = new MuRummyCardInfo { Slot = pcf.Slot };

            return _playCard[from];
        }

        internal void SetPlayCard(byte to, MuRummyCardInfo playCard)
        {
            _playCard[to] = playCard;
        }

        internal ushort Match()
        {
            var sort = _playCard
                .Skip(5)
                .Take(3)
                .Append(_playCard[9])
                .Where(x => x.Number != 0)
                .OrderBy(x => x.Number)
                .ToList();

            var lower = sort.First().Number;
            var color = sort.First().Color;
            var sameNumbers = sort.Count(x => x.Number == lower);
            var consecutive = sort.Select(x => x.Number).Distinct().Count(x => x == lower++);
            var sameColors = sort.Count(x => x.Color == color || x.Color == 4);
            var score = 0;

            for (var i = 5; i < 10; i++)
            {
                if (_playCard[i].Slot == i)
                    continue;

                var pcf = _playCard[i];
                var pct = _playCard[pcf.Slot];
                _playCard[pcf.Slot] = pcf;
                _playCard[i] = pct;
            }
            if (sameNumbers>2 || consecutive>2)
            {
                var x2Card = sort.Any(x => x.Number == 10);
                var changeColor = sort.Any(x => x.Number == 11);
                if (sameNumbers>2)
                {
                    var specialCard = sort.Take(sameNumbers).Any(x => x.Color == 4);
                    score = 10 + sort.First().Number * 10 + (specialCard?10:0) + (sameNumbers==4?10:0);
                } else
                {
                    var specialCard = sort.Take(consecutive).Any(x => x.Color == 4);
                    score = (sameColors>2 || changeColor) ? 40 : 0;
                    score += sort.First().Number * 10 + (specialCard ? 10 : 0);
                }
                foreach(var card in sort)
                {
                    ThrowPlayCard(card.Slot);
                }

                score *= x2Card?2:1;
            }

            Score += (ushort)score;

            return (ushort)score;
        }

        internal List<MuRummyCardInfo> Reveal()
        {
            var result = new List<MuRummyCardInfo>();
            for(var i = 0; i < 5 && _cardDeck.Count > 0; i++)
            {
                if(_playCard[i].Color == 0)
                {
                    _playCard[i] = _cardDeck[0];
                    _playCard[i].Slot = (byte)i;
                    result.Add(_playCard[i]);
                    _cardDeck.RemoveAt(0);
                }
            }

            if(Type == 1 && _specialCardDeck.Count > 0 && _playCard[8].Number == 0)
            {
                _playCard[8] = _specialCardDeck[0];
                result.Add(_playCard[8]);
                _specialCardDeck.RemoveAt(0);
            }

            while(result.Count < 6)
            {
                result.Add(new MuRummyCardInfo());
            }

            return result;
        }
    }
    public class MuRummy : MiniGame<MuRummyGame>
    {
        public MuRummy(string file) : base(file)
        { }
        public override BannerType GetBanner() => BannerType.MuRummy;
    }
}
