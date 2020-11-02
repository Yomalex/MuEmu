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
        public int AccountId { get; set; }

        [Required]
        [MaxLength(11)]
        public string Account { get; set; }

        [Required]
        [MaxLength(11)]
        public string Password { get; set; }

        public int VaultCount { get; set; }
        public int VaultMoney { get; set; }

        public int ServerCode { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool IsConnected { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastConnection { get; set; } = DateTime.Now;

        [MaxLength(33)]
        public string AuthToken { get; set; }

        public List<CharacterDto> Characters { get; set; }
    }
}
