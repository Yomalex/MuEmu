using MU.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("MasterSkillTree")]
    public class MasterSkillTreeDto
    {
        [XmlElement("Class")]
        public TreeDto[] Trees { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class TreeDto
    {
        [XmlAttribute] public HeroClass ID { get; set; }
        [XmlElement] public SkillTreeDto[] Skill { get; set; }

    }

    [XmlType(AnonymousType = true)]
    public class SubTreeDto
    {
        [XmlAttribute] public int Type { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class SkillTreeDto
    {
        [XmlAttribute] public int Index { get; set; }
        [XmlAttribute] public int ReqMinPoint { get; set; }
        [XmlAttribute] public int MaxPoint { get; set; }
        [XmlAttribute] public int ParentSkill1 { get; set; }
        [XmlAttribute] public int MagicNumber { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string Ecuation { get; set; } = "";

        public float GetValue(short Level)
        {
            if (string.IsNullOrWhiteSpace(Ecuation))
                return 0.0f;

            return (float)Evaluate(Ecuation.Replace("{0}", Level.ToString()));
        }

        private static double Evaluate(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("myExpression", string.Empty.GetType(), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            var result = double.Parse((string)row["myExpression"]);
            return result;
        }
    }
}
