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
        public int CharacterId { get; set; }

        public string Name { get; set; }

        public int Class { get; set; }

        public int? GuildId { get; set; }
        public GuildDto Guild { get; set; }

        public byte Map { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }

        // Stats Info
        public ushort Level { get; set; }

        public ushort Life { get; set; }
        public ushort MaxLife { get; set; }

        public ushort Mana { get; set; }
        public ushort MaxMana { get; set; }

        public long Experience { get; set; }

        public ushort LevelUpPoints { get; set; }

        public ushort Str { get; set; }

        public ushort Agility { get; set; }

        public ushort Vitality { get; set; }

        public ushort Energy { get; set; }

        public ushort Command { get; set; }
        
        // Inventory
        public List<ItemDto> Items { get; set; }

        // Quest
        public List<QuestDto> Quests { get; set; }

        public int AccountId { get; set; }
        public AccountDto Account { get; set; }
    }
}
