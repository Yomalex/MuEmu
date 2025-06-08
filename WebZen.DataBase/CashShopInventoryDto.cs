using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("CashShopInventory")]
    public class CashShopInventoryDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashShopInventoryId { get; set; }
        public int AccountId { get; set; }
        public AccountDto Account { get; set; }
        public int PackageMainIndex { get; set; }
        public int ProductBaseIndex { get; set; }
        public int ProductMainIndex { get; set; }
        public long CoinValue { get; set; }
        public byte ProductType { get; set; }
        public int GiftId { get; set; }
        [MaxLength(200)]
        public string GiftText { get; set; } = string.Empty;
    }
}
