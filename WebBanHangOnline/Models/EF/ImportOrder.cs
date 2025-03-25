using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_ImportOrder")]
    public class ImportOrder : CommonAbstract
    {
        public ImportOrder()
        {
            this.ImportOrderDetails = new HashSet<ImportOrderDetail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ImportCode { get; set; }

        [Required]
        public DateTime ImportDate { get; set; }

        public int SupplierId { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsCompleted { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; }
    }
}