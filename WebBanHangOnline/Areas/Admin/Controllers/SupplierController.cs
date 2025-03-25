using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Supplier
        public ActionResult Index()
        {
            var items = db.Suppliers.ToList();
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Supplier model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.IsActive = true;
                db.Suppliers.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.Suppliers.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier model)
        {
            if (ModelState.IsValid)
            {
                db.Suppliers.Attach(model);
                model.ModifiedDate = DateTime.Now;

                // Chỉ định các thuộc tính cần cập nhật
                //db.Entry(model).Property(x => x.SupplierCode).IsModified = true;
                db.Entry(model).Property(x => x.SupplierName).IsModified = true;
                db.Entry(model).Property(x => x.Address).IsModified = true;
                db.Entry(model).Property(x => x.Phone).IsModified = true;
                db.Entry(model).Property(x => x.IsActive).IsModified = true;
                db.Entry(model).Property(x => x.ModifiedDate).IsModified = true;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var item = db.Suppliers.Find(id);
            if (item != null)
            {
                try
                {
                    db.Suppliers.Remove(item);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Không thể xóa nhà cung cấp này." });
                }
            }
            return Json(new { success = false, message = "Nhà cung cấp không tồn tại." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}