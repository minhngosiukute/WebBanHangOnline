// Areas/Admin/Controllers/AdvEmailController.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;


using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using CommonMail = WebBanHangOnline.Common.Common; // ✅ Alias trỏ đến lớp Common trong Common.cs

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdvEmailController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/AdvEmail/Compose
        public ActionResult Compose()
        {
            var vm = new AdvEmailViewModel
            {
                AdvOptions = db.Advs
                    .OrderByDescending(a => a.CreatedDate)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    })
                    .ToList(),
                Subscribers = db.Subscribes
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new SubscriberItem
                    {
                        Id = s.Id,
                        Email = s.Email
                    })
                    .ToList()
            };
            return View(vm);
        }

        // POST: Admin/AdvEmail/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(AdvEmailViewModel vm)
        {
            if (vm.AdvId <= 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn bài quảng cáo.");
            }
            if (vm.SelectedSubscriberIds == null || !vm.SelectedSubscriberIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 email người nhận.");
            }

            if (!ModelState.IsValid)
            {
                // Nạp lại dữ liệu cho View
                vm.AdvOptions = db.Advs
                    .OrderByDescending(a => a.CreatedDate)
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Title })
                    .ToList();
                vm.Subscribers = db.Subscribes
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new SubscriberItem
                    {
                        Id = s.Id,
                        Email = s.Email,
                        Selected = vm.SelectedSubscriberIds.Contains(s.Id)
                    })
                    .ToList();

                return View("Compose", vm);
            }

            var adv = db.Advs.Find(vm.AdvId);
            if (adv == null)
            {
                TempData["ToastrError"] = "Không tìm thấy bài quảng cáo.";
                return RedirectToAction("Compose");
            }

            var recipients = db.Subscribes
                .Where(s => vm.SelectedSubscriberIds.Contains(s.Id))
                .Select(s => s.Email)
                .Distinct()
                .ToList();

            if (!recipients.Any())
            {
                TempData["ToastrError"] = "Danh sách người nhận rỗng.";
                return RedirectToAction("Compose");
            }

            // Tiêu đề & nội dung mail
            var subject = adv.Title;
            var bodyHtml = BuildAdvHtml(adv);

            int ok = 0, fail = 0;
            foreach (var email in recipients)
            {
                // ✅ Gọi hàm gửi mail trong Common.cs
                var sent = CommonMail.SendMail("ShopOnline", subject, bodyHtml, email);
                if (sent) ok++; else fail++;

                // (Tuỳ chọn) tránh throttle nếu dùng SMTP free
                // System.Threading.Thread.Sleep(100);
            }

            if (fail == 0)
                TempData["ToastrSuccess"] = $"Đã gửi '{adv.Title}' đến {ok} email đăng ký.";
            else if (ok == 0)
                TempData["ToastrError"] = "Gửi email thất bại cho tất cả người nhận. Vui lòng kiểm tra cấu hình SMTP/App Password.";
            else
                TempData["ToastrWarning"] = $"Hoàn tất: thành công {ok}, thất bại {fail}. Kiểm tra lại các địa chỉ lỗi hoặc hạn mức SMTP.";

            return RedirectToAction("Compose");
        }

        private string BuildAdvHtml(Adv adv)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='font-family:Poppins,Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.08)'>");

            // Header banner hoặc tiêu đề
            sb.Append("<div style='background:linear-gradient(90deg,#DB7093,#FF9A9E);padding:20px 30px;color:white;text-align:center'>");
            sb.AppendFormat("<h1 style='margin:0;font-size:22px;font-weight:700;letter-spacing:0.5px'>{0}</h1>", System.Web.HttpUtility.HtmlEncode(adv.Title));
            sb.Append("</div>");

            sb.Append("<div style='padding:24px 32px;color:#333'>");
            // Mô tả nội dung
            if (!string.IsNullOrWhiteSpace(adv.Description))
            {
                sb.AppendFormat("<div style='font-size:15px;line-height:1.7;color:#555;margin-bottom:24px'>{0}</div>", adv.Description);
            }

            // Nút CTA (Call to Action)
            if (!string.IsNullOrWhiteSpace(adv.Link))
            {
                sb.AppendFormat("<p style='text-align:center;margin:20px 0'><a href='{0}' style='background:#DB7093;color:#fff;text-decoration:none;padding:12px 24px;border-radius:30px;font-weight:600;display:inline-block;box-shadow:0 3px 6px rgba(219,112,147,0.3);transition:background 0.3s ease'> Truy cập ngay</a></p>", adv.Link);
            }

            sb.Append("</div>");

            // Footer
            sb.Append("<div style='background:#f9f9f9;padding:16px 24px;text-align:center;font-size:13px;color:#888;border-top:1px solid #eee'>");
            sb.Append("<p style='margin:0'>Bạn nhận được email này vì đã đăng ký nhận thông tin từ <strong>MIHGO</strong>.</p>");
            sb.Append("<p style='margin:4px 0 0'>© 2025 MIHGO. Mọi quyền được bảo lưu.</p>");
            sb.Append("</div>");

            sb.Append("</div>");
            return sb.ToString();
        }

    }
}
