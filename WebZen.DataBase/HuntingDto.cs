using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Hunting")]
    public class HuntingDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }
        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Map { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Level { get; set; }
        public long Experience { get; set; }
        public float HealingUse { get; set; }
        public long AttackPVM { get; set; }
        public long ElementalAttackPVM { get; set; }
        public int KilledMonsters { get; set; }
        public int Duration { get; set; }

    }
}
