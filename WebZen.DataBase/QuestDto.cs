using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Quest")]
    public class QuestDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestId { get; set; }

        public int Quest { get; set; }
        public byte State { get; set; }

        public string Details { get; set; }

        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }
    }
}
