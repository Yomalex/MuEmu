using MU.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.Minigames
{
    internal class BallsAndCowsGame
    {
        private Player _player;
        readonly int[] hrValue = new int[] { 600, 500, 400, 320, 285 };
        public BallsAndCowsGame(Player plr)
        {
            _player = plr;
            Ball = new byte[5];
            Strikes = new byte[5];
            Numbers = new byte[15];
            Hidden = new byte[3];
        }

        public int State { get; internal set; }
        public byte[] Ball { get; internal set; }
        public byte[] Strikes { get; internal set; }
        public byte[] Numbers { get; internal set; }
        public byte[] Hidden { get; internal set; }
        public ushort Score { get; internal set; }

        internal void Start()
        {
            Array.Fill(Ball, (byte)0xff);
            Array.Fill(Strikes, (byte)0xff);
            Array.Fill(Numbers, (byte)0xff);
            FillRow();
            State = 1;
        }

        internal void FillRow()
        {
            var list = new List<byte> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for(var i = 0; i < 3; i++)
            {
                var index = Program.RandomProvider(list.Count);
                Hidden[i] = list[index];
                list.RemoveAt(index);
            }
        }

        internal void SetNumber(byte[] numbers)
        {
            var last = Numbers.Count(x => x != 0xff);
            if (last >= Numbers.Length)
                return;

            Numbers[last] = numbers[last];

            if ((last+1) % 3 == 0)
                Match();

            if (last + 1 >= Numbers.Length)
                State = 2;
        }

        internal void Match()
        {
            for(var i = 0; i < 5; i++)
            {
                var subGroup = Numbers.Skip(i * 3).Take(3);
                if (subGroup.Any(x => x == 0xff))
                    break;

                Strikes[i] = (byte)subGroup.Where((x, i) => Hidden[i] == x).Count();
                Ball[i] = (byte)subGroup.Where((x, i) => Hidden.Contains(x) && Hidden[i] != x).Count();
            }

            var sum = 0;
            sum = Strikes.Where(x => x != 0xff).Sum(x => x) * 40;
            sum = Ball.Where(x => x != 0xff).Sum(x => x) * 10;

            var homeRun = Strikes.ToList().FindIndex(x => x == 3);

            if (homeRun != -1)
            {
                sum += hrValue[homeRun];
                State = 2;
            }

            Score = (ushort)sum;
        }
    }

    internal class BallsAndCows : MiniGame<BallsAndCowsGame>
    {
        public override BannerType GetBanner() => BannerType.BallsAndCows;
        public BallsAndCows(string file) : base(file) { }
    }
}
