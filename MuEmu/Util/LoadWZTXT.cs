using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MuEmu.Util
{
    public class LoadWZSectionTXT<T>
    {
        private Regex fileRegex;
        private List<PropertyInfo> propList;
        public LoadWZSectionTXT()
        {
            var t = typeof(T);
            var props = t.GetProperties();
            int i = 0;
            var tmpString = "";
            var posAdd = "";

            propList = new List<PropertyInfo>();
            foreach (var prop in props)
            {
                var res = prop.GetCustomAttribute<XmlAttributeAttribute>();
                var res2 = prop.GetCustomAttribute<XmlTextAttribute>();
                if (res2 != null)
                {
                    posAdd = "(?s)(.*?)\n";
                    propList.Add(prop);
                }
                if (res == null)
                {
                    continue;
                }

                propList.Add(prop);

                if (i > 0)
                {
                    tmpString += @"[^\S\r\n]+";
                }

                if (prop.PropertyType == typeof(string))
                {
                    tmpString += "\"";
                    tmpString += "(.*)";
                    tmpString += "\"";
                }
                else if (prop.PropertyType.IsPrimitive || prop.PropertyType.IsEnum)
                {
                    tmpString += i == 0 ? @"([\/0-9]+)" : @"(\-*[0-9]+)";
                }
                i++;
            }

            tmpString += @"\s*";
            tmpString += posAdd;

            fileRegex = new Regex(tmpString);
        }

        public T[] Load(string section)
        {
            List<T> _res = new List<T>();
            foreach(Match m in fileRegex.Matches(section))
            {
                if (m.Value.StartsWith("//"))
                    continue;

                var res = Activator.CreateInstance<T>();

                var i = 1;
                foreach(var prop in propList)
                {
                    prop.AssingProperty(res, m.Groups[i++]);
                }
                _res.Add(res);
            }

            return _res.ToArray();
        }
    }

    public class LoadWZTXT<T>
    {
        //private Regex sectionRegex = new Regex(@"\n+([0-9]+)\s*(.*?)\n+(?s)(.*?)\nend");//@"\n+([0-9]+)\s*\n+(?s)(.*?)\n+end");
        private Regex sectionRegex = new Regex(@"^([0-9]+)\s+(.+?)\s+(^end+)", RegexOptions.Singleline | RegexOptions.Multiline);
        private List<PropertyInfo> props;
        public LoadWZTXT()
        {
            var t = typeof(T);
            var p = t.GetProperties();
            props = p.Where(x => x.GetCustomAttribute<XmlElementAttribute>() != null && x.PropertyType.IsArray).ToList();
        }
        public T Load(string file)
        {
            var result = Activator.CreateInstance<T>();
            using (var tf = File.OpenText(file))
            {
                var contents = tf.ReadToEnd();
                if(props.Count() > 1)
                {
                    int i = 0;
                    foreach(Match m in sectionRegex.Matches(contents))
                    {
                        var propIndex = int.Parse(m.Groups[1].Value);
                        var prop = props[propIndex];

                        var constructed = typeof(LoadWZSectionTXT<>).MakeGenericType(prop.PropertyType.GetElementType());
                        object o = Activator.CreateInstance(constructed, null);
                        var load = constructed.GetMethod("Load");
                        var ret = load.Invoke(o, new object[] { m.Groups[2].Value });

                        if (prop.GetValue(result) == null)
                        {
                            prop.SetValue(result, ret);
                        }else
                        {
                            var ar = prop.GetValue(result);
                            var lenNew = ((Array)ar).Length + ((Array)ret).Length;
                            var nArr = Array.CreateInstance(prop.PropertyType.GetElementType(), lenNew);
                            ((Array)ar).CopyTo(nArr, 0);
                            ((Array)ret).CopyTo(nArr, ((Array)ar).Length);
                            prop.SetValue(result, nArr);
                        }
                        i++;
                    }
                }else if(props.Count() == 1)
                {
                    var type = typeof(LoadWZSectionTXT<>);
                    var constructed = type.MakeGenericType(props[0].PropertyType.GetElementType());
                    object o = Activator.CreateInstance(constructed, null);
                    var load = constructed.GetMethod("Load");
                    var ret = load.Invoke(o, new object[] { contents });
                    props[0].SetValue(result, ret);
                }
            }

            return result;
        }
    }
}
