using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Character")]
    public class CharacterDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        public string Name { get; set; }

        public int Class { get; set; }

        public int? Guild { get; set; }

        public byte Map { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }

        // Stats Info
        public ushort Level { get; set; }

        public long Experience { get; set; }

        public ushort LevelUpPoints { get; set; }

        public ushort Str { get; set; }

        public ushort Agility { get; set; }

        public ushort Vitality { get; set; }

        public ushort Energy { get; set; }

        public ushort Command { get; set; }

        // Equipament
        public int? LeftHand { get; set; }

        public int? RightHand { get; set; }

        public int? Helm { get; set; }

        public int? Armor { get; set; }

        public int? Pants { get; set; }

        public int? Gloves { get; set; }

        public int? Boots { get; set; }

        public int? Wings { get; set; }

        public int? Pet { get; set; }

        // Inventory
        public string Inventory { get; set; }

        public string PersonalShop { get; set; }

        // Quest
        IList<QuestDto> Quests { get; set; } = new List<QuestDto>();
    }
}
