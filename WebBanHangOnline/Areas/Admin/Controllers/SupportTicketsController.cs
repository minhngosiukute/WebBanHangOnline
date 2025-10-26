using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using WebBanHangOnline.Models.ViewModels;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupportTicketsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index(int? page, string status, string keyword)
        {
            var query = _db.SupportTickets.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.TicketCode.Contains(keyword)
                                         || t.FullName.Contains(keyword)
                                         || t.Email.Contains(keyword));
            }

            var pageSize = 5;
            var pageNumber = page ?? 1;

            var model = query
                .OrderByDescending(t => t.CreatedDate)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.StatusList = new SelectList(SupportTicketStatus.All(), status);

            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var ticket = _db.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var model = new SupportTicketDetailViewModel
            {
                Ticket = ticket,
                Messages = ticket.Messages.OrderBy(m => m.CreatedDate),
                Reply = new SupportTicketReplyViewModel { TicketId = ticket.Id }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(SupportTicketReplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id = model.TicketId });
            }

            var ticket = _db.SupportTickets.Find(model.TicketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var message = new SupportTicketMessage
            {
                TicketId = ticket.Id,
                Message = model.Message,
                CreatedDate = DateTime.Now,
                IsFromStaff = true,
                SenderId = User.Identity.Name,
                SenderName = string.IsNullOrWhiteSpace(User.Identity.Name) ? "Hỗ trợ viên" : User.Identity.Name
            };

            _db.SupportTicketMessages.Add(message);

            ticket.Status = ticket.Status == SupportTicketStatus.Pending ? SupportTicketStatus.InProgress : ticket.Status;
            ticket.LastRepliedAt = message.CreatedDate;
            ticket.LastRepliedBy = message.SenderName;
            ticket.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Đã gửi phản hồi tới khách hàng.";

            return RedirectToAction("Details", new { id = ticket.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status) || !SupportTicketStatus.All().Contains(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            var ticket = _db.SupportTickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            ticket.Status = status;
            ticket.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái thành công.";

            return RedirectToAction("Details", new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // Lấy ticket kèm toàn bộ message để xóa gọn gàng
            var ticket = _db.SupportTickets
                            .Include(t => t.Messages)
                            .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                TempData["Error"] = "Yêu cầu hỗ trợ không tồn tại hoặc đã bị xóa.";
                return RedirectToAction("Index");
            }

            try
            {
                // Nếu chưa bật cascade delete ở DB/EF, mình xóa thủ công Messages trước
                if (ticket.Messages != null && ticket.Messages.Any())
                {
                    _db.SupportTicketMessages.RemoveRange(ticket.Messages);
                }

                _db.SupportTickets.Remove(ticket);
                _db.SaveChanges();

                TempData["Success"] = $"Đã xóa yêu cầu #{ticket.TicketCode}.";
            }
            catch (Exception ex)
            {
                // Có thể log ex nếu cần
                TempData["Error"] = "Không thể xóa yêu cầu. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
