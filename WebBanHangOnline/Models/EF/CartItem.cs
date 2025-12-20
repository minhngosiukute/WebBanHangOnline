using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_CartItem")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Alias { get; set; }

        public string CategoryName { get; set; }

        public string ProductImg { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }
    }
}