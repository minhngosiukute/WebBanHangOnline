using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using PagedList; // Thêm namespace cho PagedList

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult _Review(int productId)
        {
            ViewBag.ProductId = productId;
            var item = new ReviewProduct();
            if (User.Identity.IsAuthenticated)
            {
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(User.Identity.Name);
                if (user != null)
                {
                    item.Email = user.Email;
                    item.FullName = user.FullName;
                    item.UserName = user.UserName;
                }
                return PartialView(item);
            }
            return PartialView();
        }

        public ActionResult LichSuDonHang()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(User.Identity.Name);

                var items = _db.Orders
                    .Where(x => x.CustomerId == user.Id)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList(); // Lấy toàn bộ danh sách đơn hàng, không phân trang

                return PartialView(items);
            }

            return PartialView();
        }

        public ActionResult View(int id)
        {
            var item = _db.Orders.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(User.Identity.Name);
                if (item.CustomerId != user.Id)
                {
                    return HttpNotFound();
                }
            }

            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = _db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [AllowAnonymous]
        public ActionResult _Load_Review(int productId)
        {
            var item = _db.Reviews
                .Where(x => x.ProductId == productId && x.IsActive)
                .OrderByDescending(x => x.Id)
                .ToList();
            ViewBag.Count = item.Count;
            return PartialView(item);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PostReview(ReviewProduct req)
        {
            if (ModelState.IsValid)
            {
                req.CreatedDate = DateTime.Now;
                req.IsActive = false;

                string baseSeed = string.IsNullOrEmpty(req.FullName) ? req.UserName ?? "anonymous" : req.FullName;
                string randomSeed = baseSeed + DateTime.Now.Ticks.ToString();
                req.Avatar = $"https://api.dicebear.com/8.x/avataaars/svg?seed={Uri.EscapeDataString(randomSeed)}";

                _db.Reviews.Add(req);
                _db.SaveChanges();

                var response = new
                {
                    Success = true,
                    Message = "Đánh giá của bạn đã được gửi và đang chờ duyệt.",
                    Review = new
                    {
                        Id = req.Id,
                        FullName = req.FullName,
                        Content = req.Content,
                        Rate = req.Rate,
                        CreatedDate = req.CreatedDate.ToString("dd MM yyyy"),
                        Avatar = req.Avatar
                    }
                };
                return Json(response);
            }
            return Json(new { Success = false, Message = "Dữ liệu không hợp lệ." });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}