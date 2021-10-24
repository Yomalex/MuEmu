using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MU.DataBase
{
    [Table("MasterInfo")]
    public class MasterInfoDto
    {
        [Key]
        public int MasterInfoId { get; set; }
        // Stats Info
        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Level { get; set; }

        [Column(TypeName = "BIGINT UNSIGNED")]
        public long Experience { get; set; }
        // Stats Info
        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Points { get; set; }
    }
}
