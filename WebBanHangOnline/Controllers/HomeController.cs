using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {

            //WebBanHangOnline.Common.Common.SendMail("ABC", "AAAA", "AAAA", "tinhhuynh41238@gmail.com");
            return View();
        }

        public ActionResult Partial_Subcrice()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Subscribe(Subscribe req)
        {
            // Chuẩn hóa email (tránh A@B.com ≠ a@b.com trên một số collation)
            var email = (req?.Email ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(email))
                return Json(new { Success = false, Message = "Vui lòng nhập email hợp lệ." });

            // Không cần tự kiểm tra trùng (DB đã có UNIQUE).
            db.Subscribes.Add(new Subscribe
            {
                Email = email,
                CreatedDate = DateTime.Now
            });

            try
            {
                db.SaveChanges();
                return Json(new { Success = true, Message = "Đăng ký thành công! Cảm ơn bạn đã theo dõi." });
            }
            catch (DbUpdateException)
            {
                // Vi phạm UNIQUE index => email đã tồn tại
                return Json(new { Success = false, Message = "Email này đã đăng ký trước đó rồi." });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Refresh()
        {
            var item = new ThongKeModel();

            ViewBag.Visitors_online = HttpContext.Application["visitors_online"];
            var hn = HttpContext.Application["HomNay"];
            item.HomNay = HttpContext.Application["HomNay"].ToString();
            item.HomQua = HttpContext.Application["HomQua"].ToString();
            item.TuanNay = HttpContext.Application["TuanNay"].ToString();
            item.TuanTruoc = HttpContext.Application["TuanTruoc"].ToString();
            item.ThangNay = HttpContext.Application["ThangNay"].ToString();
            item.ThangTruoc = HttpContext.Application["ThangTruoc"].ToString();
            item.TatCa = HttpContext.Application["TatCa"].ToString();
            return PartialView(item);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}