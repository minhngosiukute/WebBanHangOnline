using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_SupportTicketMessage")]
    public class SupportTicketMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }

        [Required]
        public string Message { get; set; }

        [StringLength(256)]
        public string SenderId { get; set; }

        [StringLength(150)]
        public string SenderName { get; set; }

        public bool IsFromStaff { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual SupportTicket Ticket { get; set; }
    }
}
