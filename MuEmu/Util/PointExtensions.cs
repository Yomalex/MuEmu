using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Util
{
    public static class PointExtensions
    {
        public static Point Substract(this Point a, Point b)
        {
            var x = b.X - a.X;
            var y = b.Y - a.Y;
            return new Point(x, y);
        }
        public static int Length(this Point a)
        {
            return a.X*a.X + a.Y*a.Y;
        }
        public static double LengthSquared(this Point a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y);
        }
    }
}
