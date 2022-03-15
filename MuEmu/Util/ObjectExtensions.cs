using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MuEmu.Util
{
    public static class ObjectExtensions
    {
        public static void AssingProperty(this PropertyInfo pinfo, object a, Group g)
        {
            if (pinfo.PropertyType == typeof(string))
            {
                pinfo.SetValue(a, g.Value);
            }
            else if (pinfo.PropertyType == typeof(sbyte))
            {
                pinfo.SetValue(a, sbyte.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(short))
            {
                pinfo.SetValue(a, short.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(int))
            {
                pinfo.SetValue(a, int.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(long))
            {
                pinfo.SetValue(a, long.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(byte))
            {
                pinfo.SetValue(a, byte.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(ushort))
            {
                pinfo.SetValue(a, ushort.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(uint))
            {
                pinfo.SetValue(a, uint.Parse(g.Value));
            }
            else if (pinfo.PropertyType == typeof(ulong))
            {
                pinfo.SetValue(a, ulong.Parse(g.Value));
            }
            else if (pinfo.PropertyType.IsEnum)
            {
                pinfo.SetValue(a, Enum.Parse(pinfo.PropertyType, g.Value));
            }
        }
        public static T AssignRegex<T>(this T a, Group[] g)
        {
            var props = typeof(T).GetProperties();
            for (var i = 0; i < Math.Min(props.Length, g.Length); i++)
            {
                props[i].AssingProperty(a, g[i + 1]);
            }

            return a;
        }

        public static object Get<T>(this T a, string name)
        {
            var type = typeof(T);
            var prop = type.GetProperty(name);
            var get = prop.GetGetMethod();
            return get.Invoke(a, null);
        }

        public static void Set<T>(this T a, string name, object value)
        {
            var type = typeof(T);
            var prop = type.GetProperty(name);
            if(prop != null)
                prop.SetValue(a, value);
        }

        public static void SetBit(this byte a, byte bit)
        {
            a = (byte)(a | (1 << bit));
        }
        public static void ClearBit(this byte a, byte bit)
        {
            a = (byte)(a & ~(1 << bit));
        }
        public static bool GetBit(this byte a, byte bit)
        {
            return (a & (1 << bit)) != 0;
        }
    }
}
