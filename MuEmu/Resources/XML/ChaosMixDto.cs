using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MuEmu.Resources.XML
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("ChaosBox")]
    public class ChaosMixDto
    {
        [XmlElement("Jewel")]
        public JewelInfoDto[] Jewels { get; set; }

        [XmlElement("Mix")]
        public MixInfoDto[] Mixes { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class JewelInfoDto
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlAttribute("index")]
        public int Index { get; set; }

        [XmlAttribute("success")]
        public int Success { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MixInfoDto
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("success")]
        public int Success { get; set; }

        [XmlAttribute("value")]
        public int Value { get; set; }

        [XmlElement("Ingredient")]
        public IngredientInfoDto[] Ingredients { get; set; }

        [XmlElement("RewardSuccess")]
        public IngredientInfoDto[] RewardSuccess { get; set; }

        public IngredientInfoDto RewardFail { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class IngredientInfoDto
    {
        [XmlAttribute("iid")]
        public int IID { get; set; }

        [XmlAttribute("type")]
        public int Type { get; set; } = -1;

        [XmlAttribute("index")]
        public int Index { get; set; } = -1;

        [XmlAttribute("level")]
        public string Level { get; set; } = "255";

        [XmlAttribute("luck")]
        public int Luck { get; set; } = -1;

        [XmlAttribute("skill")]
        public int Skill { get; set; } = -1;

        [XmlAttribute("option")]
        public int Option { get; set; } = -1;

        [XmlAttribute("count")]
        public int Count { get; set; } = 1;

        [XmlAttribute("success")]
        public int Success { get; set; } = 100;
    }
}
