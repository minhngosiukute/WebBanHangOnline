using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_ImportOrderDetail")]
    public class ImportOrderDetail : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ImportOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ImportPrice { get; set; }

        public decimal TotalPrice { get; set; }

        [ForeignKey("ImportOrderId")]
        public virtual ImportOrder ImportOrder { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}