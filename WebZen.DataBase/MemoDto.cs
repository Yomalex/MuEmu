using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Letters")]
    public class MemoDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemoId { get; set; }

        public int CharacterId { get; set; }

        public int SenderId { get; set; }
        public CharacterDto Sender { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Date { get; set; }

        [MaxLength(32)]
        public string Subject { get; set; }
    }
}
