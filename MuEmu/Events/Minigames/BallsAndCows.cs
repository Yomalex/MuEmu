using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events.Minigames
{
    internal class BallsAndCowsGame
    {
        public BallsAndCowsGame(Player plr)
        {

        }
    }

    internal class BallsAndCows : MiniGame<BallsAndCowsGame>
    {
        public override BannerType GetBanner() => BannerType.BallsAndCows;
        public BallsAndCows(string file) : base(file) { }
    }
}
