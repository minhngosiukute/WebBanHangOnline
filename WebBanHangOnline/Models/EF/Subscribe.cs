using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Subscribe")]
    public class Subscribe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, EmailAddress]
        [StringLength(256)]
        [Index("IX_tb_Subscribe_Email", IsUnique = true)] // <- CHỐT: unique
        public string Email { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}