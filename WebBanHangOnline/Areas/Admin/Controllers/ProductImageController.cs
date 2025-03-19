using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImage
        public ActionResult Index(int id)
        {
            ViewBag.ProductId = id;
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int productId, string url)
        {
            db.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                Image = url,
                IsDefault = false
            });
            db.SaveChanges();
            return Json(new { Success = true });
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult SetDefault(int id)
        {
            // Tìm ảnh theo Id
            var selectedImage = db.ProductImages.Find(id);
            if (selectedImage == null)
            {
                return Json(new { success = false });
            }

            // Đặt tất cả các ảnh khác của sản phẩm này thành IsDefault = false
            var productImages = db.ProductImages.Where(pi => pi.ProductId == selectedImage.ProductId).ToList();
            foreach (var img in productImages)
            {
                img.IsDefault = false;
            }

            // Đặt ảnh hiện tại thành mặc định
            selectedImage.IsDefault = true;
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}