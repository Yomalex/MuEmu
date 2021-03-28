using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("BloodCastle")]
    public class BloodCastleDto
    {
        [Key]
        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }

        public int Points { get; set; }
    }
}
