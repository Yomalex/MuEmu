using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MuEmu.Util
{
    public static class ObjectExtensions
    {
        public static T AssignRegex<T>(this T a, Group[] g)
        {
            var props = typeof(T).GetProperties();
            for (var i = 0; i < Math.Min(props.Length, g.Length); i++)
            {
                if (props[i].PropertyType == typeof(string))
                {
                    props[i].SetValue(a, g[i + 1].Value);
                }
                else if (props[i].PropertyType == typeof(sbyte))
                {
                    props[i].SetValue(a, sbyte.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(short))
                {
                    props[i].SetValue(a, short.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(int))
                {
                    props[i].SetValue(a, int.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(long))
                {
                    props[i].SetValue(a, long.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(byte))
                {
                    props[i].SetValue(a, byte.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(ushort))
                {
                    props[i].SetValue(a, ushort.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(uint))
                {
                    props[i].SetValue(a, uint.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType == typeof(ulong))
                {
                    props[i].SetValue(a, ulong.Parse(g[i + 1].Value));
                }
                else if (props[i].PropertyType.IsEnum)
                {
                    props[i].SetValue(a, Enum.Parse(props[i].PropertyType, g[i + 1].Value));
                }
            }

            return a;
        }
    }
}
