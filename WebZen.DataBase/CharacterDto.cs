using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
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

        public int CtlCode { get; set; }

        public int? GuildId { get; set; }
        public GuildDto Guild { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public byte Map { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        // Stats Info
        [Column(TypeName = "SMALLINT(5)")]
        public ushort Level { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Life { get; set; }
        [Column(TypeName = "SMALLINT(5)")]
        public ushort MaxLife { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Mana { get; set; }
        [Column(TypeName = "SMALLINT(5)")]
        public ushort MaxMana { get; set; }

        public long Experience { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort LevelUpPoints { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Str { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Agility { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Vitality { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Energy { get; set; }

        [Column(TypeName = "SMALLINT(5)")]
        public ushort Command { get; set; }

        [Column(TypeName = "INT(11)")]
        public uint Money { get; set; }

        public byte ExpandedInventory { get; set; }

        // Inventory
        [NotMapped]
        public List<ItemDto> Items { get; set; }

        // Spells
        public List<SpellDto> Spells { get; set; }

        // Quest
        public List<QuestDto> Quests { get; set; }
        public List<QuestEXDto> QuestEX { get; set; }

        // Friends
        public List<FriendDto> Friends { get; set; }
        public List<MemoDto> Memos { get; set; }

        [NotMapped]
        public List<GremoryCaseDto> GremoryCases { get; set; }

        public SkillKeyDto SkillKey { get; set; } = new SkillKeyDto();

        public MasterInfoDto MasterInfo { get; set; } = new MasterInfoDto { Experience = 0, Level = 1, Points = 0 };

        public int AccountId { get; set; }
        public AccountDto Account { get; set; }

        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Resets { get; set; }

        // Gens
        public GensDto Gens { get; set; } = new GensDto { Class = 14, Contribution = 0, Influence = 0, Ranking = 9999 };
        public BloodCastleDto BloodCastle { get; set; } = new BloodCastleDto { Points = 0 };
    }
}
