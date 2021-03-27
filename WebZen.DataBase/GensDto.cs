using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Gens")]
    public class GensDto
    {
        [Key]
        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }

        public int Influence { get; set; }

        public int Class { get; set; }
        public int Ranking { get; set; }
        public int Contribution { get; set; }
    }
}
