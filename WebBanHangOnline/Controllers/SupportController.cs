// SupportController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using WebBanHangOnline.Models.ViewModels;

namespace WebBanHangOnline.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult Index()
        {
            var vm = new SupportTicketCreateViewModel();

            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    // Thử tìm user theo Email trước, nếu không có dùng UserName
                    var login = User.Identity.Name;
                    var appUser = _db.Users.FirstOrDefault(u =>
                        u.Email == login || u.UserName == login);

                    if (appUser != null)
                    {
                        vm.Email = appUser.Email;

                        // Nếu bạn có trường họ tên trong bảng AspNetUsers, dùng nó.
                        // Một số project lưu tên trong appUser.FullName hoặc appUser.DisplayName
                        // Thay tên trường nếu khác:
                        vm.FullName = (appUser.FullName ?? appUser.FullName ?? appUser.UserName) ?? "";

                        // Nếu có lưu SĐT trong profile:
                        vm.PhoneNumber = appUser.Phone;
                    }
                }
                catch
                {
                    // không bắt lỗi to, chỉ không prefill nếu có vấn đề
                }
            }

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SupportTicketCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var ticket = new SupportTicket
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Subject = model.Subject,
                CreatedDate = DateTime.Now,
                Status = SupportTicketStatus.Pending
            };

            ticket.Messages.Add(new SupportTicketMessage
            {
                Message = model.Message,
                SenderName = model.FullName,
                SenderId = model.Email,
                IsFromStaff = false,
                CreatedDate = DateTime.Now
            });

            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();

            TempData["SupportTicketCode"] = ticket.TicketCode;
            TempData["SupportTicketEmail"] = ticket.Email;

            return RedirectToAction("Submitted");
        }

        [HttpGet]
        public ActionResult Submitted()
        {
            var ticketCode = TempData["SupportTicketCode"] as string;
            var email = TempData["SupportTicketEmail"] as string;
            if (string.IsNullOrEmpty(ticketCode) || string.IsNullOrEmpty(email))
                return RedirectToAction("Index");

            ViewBag.TicketCode = ticketCode;
            ViewBag.Email = email;
            return View();
        }

        // ======== NEW: Liệt kê ticket theo email ========
        [HttpGet]
        public ActionResult MyTickets(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Email = "";
                // Trả về view với Model rỗng (khỏi dùng IEnumerable để null-safe)
                return View(Enumerable.Empty<MyTicketRowVM>());
            }

            var list = _db.SupportTickets
                          .Where(t => t.Email == email)
                          .OrderByDescending(t => t.CreatedDate)
                          .Select(t => new MyTicketRowVM
                          {
                              Id = t.Id,
                              TicketCode = t.TicketCode,
                              Subject = t.Subject,
                              Status = t.Status,
                              CreatedDate = t.CreatedDate,
                              UpdatedDate = t.UpdatedDate
                          })
                          .ToList();

            ViewBag.Email = email;
            return View(list);
        }


        [HttpGet]
        public ActionResult Track(string code, string email)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(email))
                return View();

            var ticket = _db.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefault(t => t.TicketCode == code && t.Email == email);

            if (ticket == null)
            {
                ViewBag.NotFound = true;
                return View();
            }

            var model = new SupportTicketDetailViewModel
            {
                Ticket = ticket,
                Messages = ticket.Messages.OrderBy(m => m.CreatedDate),
                Reply = new SupportTicketReplyViewModel { TicketId = ticket.Id } // NEW: để bind form trả lời
            };

            ViewBag.Code = code;   // giữ lại để post
            ViewBag.Email = email; // giữ lại để post
            return View(model);
        }

        // ======== NEW: Khách gửi phản hồi ========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply([Bind(Prefix = "Reply")] SupportTicketReplyViewModel model, string code, string email)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập nội dung phản hồi.";
                return RedirectToAction("Track", new { code, email });
            }

            var ticket = _db.SupportTickets.Find(model.TicketId);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu hỗ trợ.";
                return RedirectToAction("Index");
            }

            // Xác thực: code & email phải khớp ticket hiện có
            if (!string.Equals(ticket.Email, email, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(ticket.TicketCode, code, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Thông tin xác thực không đúng.";
                return RedirectToAction("Track", new { code, email });
            }

            var msg = new SupportTicketMessage
            {
                TicketId = ticket.Id,
                Message = model.Message,
                IsFromStaff = false,
                SenderId = ticket.Email,
                SenderName = ticket.FullName,
                CreatedDate = DateTime.Now
            };

            _db.SupportTicketMessages.Add(msg);

            // cập nhật trạng thái nếu đang Pending
            if (ticket.Status == SupportTicketStatus.Pending)
                ticket.Status = SupportTicketStatus.InProgress;

            ticket.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            TempData["Success"] = "Đã gửi phản hồi!";
            return RedirectToAction("Track", new { code, email });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
