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
            return View(new SupportTicketCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SupportTicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
            {
                return RedirectToAction("Index");
            }

            ViewBag.TicketCode = ticketCode;
            ViewBag.Email = email;

            return View();
        }

        [HttpGet]
        public ActionResult Track(string code, string email)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(email))
            {
                return View();
            }

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
                Messages = ticket.Messages.OrderBy(m => m.CreatedDate)
            };

            return View(model);
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
