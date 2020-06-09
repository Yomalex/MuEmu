using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Util
{
    struct Triangle
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }
        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }

        private float sign(PointF p1, PointF p2, PointF p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public bool Contains(PointF pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, A, B);
            d2 = sign(pt, B, B);
            d3 = sign(pt, C, A);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
    }
}
