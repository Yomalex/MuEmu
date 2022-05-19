using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("GuildMatching")]
    public class GuildMatchingDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int GuildId { get; set; }
        public GuildDto Guild { get; set; }

        public string Title { get; set; }
        public short InterestType { get; set; }
        public short LevelRange { get; set; }
        [Column(TypeName = "SMALLINT(5)")]
        public ushort Class { get; set; }
    }
    [Table("GuildMatchingJoin")]
    public class GuildMatchingJoinDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int GuildMatchingId { get; set; }
        public GuildMatchingDto GuildMatching { get; set; }
        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }
        public byte State { get; set; }
    }
}
