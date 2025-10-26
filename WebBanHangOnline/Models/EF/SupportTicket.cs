using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_SupportTicket")]
    public class SupportTicket
    {
        public SupportTicket()
        {
            TicketCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            Status = SupportTicketStatus.Pending;
            Messages = new HashSet<SupportTicketMessage>();
            CreatedDate = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(16)]
        public string TicketCode { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(256)]
        public string AssignedToUserId { get; set; }

        [StringLength(150)]
        public string AssignedToName { get; set; }

        public DateTime? LastRepliedAt { get; set; }

        [StringLength(256)]
        public string LastRepliedBy { get; set; }

        public virtual ICollection<SupportTicketMessage> Messages { get; set; }
    }

    public static class SupportTicketStatus
    {
        public const string Pending = "Chờ xử lý";
        public const string InProgress = "Đang xử lý";
        public const string Resolved = "Đã giải quyết";
        public const string Closed = "Đã đóng";

        public static IEnumerable<string> All()
        {
            return new[] { Pending, InProgress, Resolved, Closed };
        }
    }
}
