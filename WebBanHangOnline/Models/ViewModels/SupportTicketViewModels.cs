using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models.ViewModels
{
    public class SupportTicketCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")] 
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề hỗ trợ")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả vấn đề bạn gặp phải")]
        [Display(Name = "Nội dung yêu cầu")]
        public string Message { get; set; }
    }
    // ViewModels liệt kê ticket theo email (MyTickets)
    public class MyTicketRowVM
    {
        public int Id { get; set; }
        public string TicketCode { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SupportTicketDetailViewModel
    {
        public SupportTicket Ticket { get; set; }
        public IEnumerable<SupportTicketMessage> Messages { get; set; }
        public SupportTicketReplyViewModel Reply { get; set; }
    }

    public class SupportTicketReplyViewModel
    {
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        [Display(Name = "Nội dung phản hồi")]
        public string Message { get; set; }
    }
}
