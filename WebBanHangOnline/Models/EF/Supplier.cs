using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Supplier")]
    public class Supplier : CommonAbstract
    {
        public Supplier()
        {
            this.ImportOrders = new HashSet<ImportOrder>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string SupplierName { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<ImportOrder> ImportOrders { get; set; }
    }
}