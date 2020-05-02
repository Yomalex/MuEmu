using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Util
{
    public static class IEnumerableExtensions
    {
        public static T PopBack<T>(this IList<T> ts)
        {
            var last = ts.Last();
            ts.Remove(last);
            return last;
        }
        public static T PopFront<T>(this IList<T> ts)
        {
            var first = ts.First();
            ts.Remove(first);
            return first;
        }
    }
}
