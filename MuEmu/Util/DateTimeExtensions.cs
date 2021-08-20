using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Util
{
    public static class DateTimeExtensions
    {
        public static int ToTimeT(this DateTime dt)
        {
            return (int)Math.Floor((dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }
    }
}
