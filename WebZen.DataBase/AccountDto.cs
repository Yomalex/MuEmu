using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MU.DataBase
{
    [Table("Account")]
    public class AccountDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Account { get; set; } 
        public string Password { get; set; }

        public int? Character1 { get; set; }
        public int? Character2 { get; set; }
        public int? Character3 { get; set; }
        public int? Character4 { get; set; }
        public int? Character5 { get; set; }

        public string Vault { get; set; }
    }
}
