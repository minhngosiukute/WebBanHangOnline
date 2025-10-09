using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdvController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Adv
        public ActionResult Index()
        {
            var items = db.Advs.OrderByDescending(x => x.CreatedDate).ToList();
            return View(items);
        }

        // GET: Admin/Adv/Create
        public ActionResult Create()
        {
            return View(new AdvCreateViewModel());
        }

        // POST: Admin/Adv/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // chấp nhận HTML
        public ActionResult Create(AdvCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var adv = new Adv
            {
                Title = model.Title,
                Description = model.Description,
                Link = model.Link,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            db.Advs.Add(adv);
            db.SaveChanges();
            TempData["ToastrSuccess"] = "Tạo bài quảng cáo thành công!";
            return RedirectToAction("Index");
        }

        // GET: Admin/Adv/Edit/5
        public ActionResult Edit(int id)
        {
            var item = db.Advs.Find(id);
            if (item == null) return HttpNotFound();

            var vm = new AdvCreateViewModel
            {
                Title = item.Title,
                Description = item.Description,
                //Image = item.Image,
                Link = item.Link
                //Type = item.Type
            };
            ViewBag.AdvId = id;
            return View(vm);
        }

        // POST: Admin/Adv/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AdvCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errs = string.Join(" | ",
                    ModelState.Where(k => k.Value.Errors.Count > 0)
                              .Select(k => $"{k.Key}: {string.Join(", ", k.Value.Errors.Select(e => e.ErrorMessage))}"));
                TempData["ToastrError"] = string.IsNullOrWhiteSpace(errs) ? "Cập nhật thất bại." : errs;
                ViewBag.AdvId = id;
                return View(vm);
            }

            var item = db.Advs.Find(id);
            if (item == null) return HttpNotFound();

            item.Title = vm.Title?.Trim();
            item.Description = vm.Description;
            //item.Image = vm.Image;
            item.Link = vm.Link;
            //item.Type = vm.Type;
            item.ModifiedDate = DateTime.Now;
            // item.ModifiedBy = User?.Identity?.Name ?? "system"; // nếu có

            db.SaveChanges();
            TempData["ToastrSuccess"] = "Cập nhật bài quảng cáo thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Advs.Find(id);
            if (item != null)
            {
                db.Advs.Remove(item);
                db.SaveChanges();
                TempData["ToastrSuccess"] = "Đã xoá bài quảng cáo!";
            }
            else
            {
                TempData["ToastrError"] = "Không tìm thấy bài quảng cáo.";
            }
            return RedirectToAction("Index");
        }
    }
}
