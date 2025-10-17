using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(string searchText, int? page)
        {
            // vẫn giữ IQueryable để OrderBy/AsNoTracking thực thi trên SQL
            var query = db.Products
                          .AsNoTracking()
                          .OrderByDescending(x => x.Id);

            IEnumerable<Product> items = query; // giữ nguyên kiểu IEnumerable cho PagedList

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // chuẩn hóa từ khóa: bỏ dấu + thường
                var normalizedSearch = WebBanHangOnline.Models.Common.Filter
                                         .FilterChar(searchText).ToLower();

                // materialize 1 lần để lọc không dấu trong bộ nhớ (không đụng SQL)
                var list = query.ToList();

                items = list.Where(x =>
                    WebBanHangOnline.Models.Common.Filter.FilterChar(x.Title ?? "")
                        .ToLower().Contains(normalizedSearch)
                    || WebBanHangOnline.Models.Common.Filter.FilterChar(x.ProductCategory.Title ?? "")
                        .ToLower().Contains(normalizedSearch)
                    || WebBanHangOnline.Models.Common.Filter.FilterChar(x.Alias ?? "")
                        .ToLower().Contains(normalizedSearch)
                );

                ViewBag.SearchText = searchText; // giữ lại từ khóa cho view
            }

            int pageSize = 5;
            int pageIndex = page ?? 1;

            var model = items.ToPagedList(pageIndex, pageSize); // GIỮ Y NGUYÊN phân trang cũ
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;

            return View(model);
        }


        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                //if (string.IsNullOrEmpty(model.SeoTitle))
                //{
                //    model.SeoTitle = model.Title;
                //}
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            var item = db.Products.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                // Tạo một danh sách tạm thời để lưu các ảnh cần xóa
                var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id).ToList();
                if (checkImg != null)
                {
                    // Lặp qua danh sách tạm thời và xóa từng ảnh
                    foreach (var img in checkImg)
                    {
                        db.ProductImages.Remove(img);
                    }

                    // Lưu thay đổi sau khi đã xóa ảnh
                    db.SaveChanges();
                }

                // Xóa sản phẩm
                db.Products.Remove(item);
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        //[HttpPost]
        //public ActionResult IsSale(int id)
        //{
        //    var item = db.Products.Find(id);
        //    if (item != null)
        //    {
        //        item.IsSale = !item.IsSale;
        //        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        return Json(new { success = true, IsSale = item.IsSale });
        //    }

        //    return Json(new { success = false });
        //}

        public class FavoriteRow
        {
            public int ProductId { get; set; }
            public string Title { get; set; }
            public string CategoryName { get; set; }
            public int ViewCount { get; set; }
            public int LikeCount { get; set; }
            public string Image { get; set; } // thêm dòng này
        }

        public ActionResult FavoriteRanking(int? page)
        {
            var query = db.Products
                .AsNoTracking()
                .Select(p => new FavoriteRow
                {
                    ProductId = p.Id,
                    Title = p.Title,
                    CategoryName = p.ProductCategory.Title,
                    ViewCount = p.ViewCount,
                    LikeCount = p.Wishlists.Count(),
                    Image = p.Image // thêm dòng này
                })
                .OrderByDescending(x => x.LikeCount)
                .ThenByDescending(x => x.ViewCount);

            var top = query.FirstOrDefault();
            ViewBag.TopFavoriteMessage = top == null
                ? null
                : $"Sản phẩm được yêu thích nhất: {top.Title} (Lượt thích: {top.LikeCount}, Lượt xem: {top.ViewCount})";

            int pageSize = 6;
            int pageIndex = page ?? 1;
            var model = query.ToPagedList(pageIndex, pageSize);   // IPagedList<FavoriteRow>

            return View(model);
        }
        // ======================= TOP SẢN PHẨM BÁN CHẠY =======================
        public class BestSellingRow
        {
            public int ProductId { get; set; }
            public string Title { get; set; }
            public string CategoryName { get; set; }
            public string Image { get; set; }
            public int TotalSold { get; set; }
            public decimal Price { get; set; }
        }

        public ActionResult BestSelling(int? page)
        {
            var query = db.OrderDetails
                .Include("Product.ProductCategory") // nạp cả thông tin product và category
                .GroupBy(x => x.ProductId)
                .Select(g => new BestSellingRow
                {
                    ProductId = g.Key,
                    Title = g.FirstOrDefault().Product.Title,
                    CategoryName = g.FirstOrDefault().Product.ProductCategory.Title,
                    Image = g.FirstOrDefault().Product.Image,
                    TotalSold = g.Sum(x => x.Quantity),
                    Price = g.FirstOrDefault().Product.Price
                })
                .OrderByDescending(x => x.TotalSold);

            int pageSize = 8;
            int pageIndex = page ?? 1;
            var model = query.ToPagedList(pageIndex, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;

            var top = query.FirstOrDefault();
            ViewBag.TopBestSelling = top == null
                ? null
                : $"🔥 Sản phẩm bán chạy nhất: {top.Title} ({top.TotalSold} lượt mua)";

            return View(model);
        }

    }
}