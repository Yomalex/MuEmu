using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebZen.DataBase
{
    [Table("account")]
    public class AccountDto
    {
        public int Account { get; set; }
    }
}
