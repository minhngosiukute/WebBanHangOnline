using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Cart")]
    public class Cart : CommonAbstract
    {
        public Cart()
        {
            Items = new HashSet<CartItem>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ICollection<CartItem> Items { get; set; }
    }
}