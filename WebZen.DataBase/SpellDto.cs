using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Spells")]
    public class SpellDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpellId { get; set; }

        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }

        public short Magic { get; set; }
        public short Level { get; set; }
    }
}
