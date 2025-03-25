using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using PagedList;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReviewController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Admin/Review
        public ActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var reviews = _db.Reviews
                .Include("Product")
                .OrderByDescending(x => x.CreatedDate)
                .ToPagedList(pageNumber, pageSize);

            return View(reviews);
        }

        // POST: Admin/Review/ToggleStatus/5
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            try
            {
                var review = _db.Reviews.Find(id);
                if (review == null)
                {
                    return Json(new { success = false, message = "Đánh giá không tồn tại." });
                }

                // Toggle trạng thái IsActive
                review.IsActive = !review.IsActive;
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = review.IsActive ? "Duyệt đánh giá thành công." : "Hủy duyệt đánh giá thành công.",
                    isActive = review.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thay đổi trạng thái: " + ex.Message });
            }
        }

        // POST: Admin/Review/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var review = _db.Reviews.Find(id);
                if (review == null)
                {
                    return Json(new { success = false, message = "Đánh giá không tồn tại." });
                }

                _db.Reviews.Remove(review);
                _db.SaveChanges();

                return Json(new { success = true, message = "Xóa đánh giá thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
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