using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly RoleManager<IdentityRole> roleManager;

        public RoleController()
        {
            // Khởi tạo RoleManager
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        // GET: Admin/Role
        //public ActionResult Index()
        //{
        //    var items = roleManager.Roles.ToList();
        //    return View(items);
        //}

        public ActionResult Index(int? page)
        {
            int pageSize = 6; // Số lượng vai trò trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var roles = db.Roles.OrderBy(r => r.Name).ToPagedList(pageNumber, pageSize);

            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(roles);
        }


        // GET: Admin/Role/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem vai trò đã tồn tại chưa
                if (roleManager.RoleExists(model.Name))
                {
                    ModelState.AddModelError("", "Vai trò đã tồn tại.");
                    return View(model);
                }

                var result = roleManager.Create(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: Admin/Role/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var item = roleManager.FindById(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        // POST: Admin/Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var result = roleManager.Update(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var role = roleManager.FindById(id);
            if (role != null)
            {
                // Kiểm tra nếu có người dùng nào đang sử dụng vai trò này
                var usersInRole = db.Users.Any(u => u.Roles.Any(r => r.RoleId == role.Id));
                if (usersInRole)
                {
                    return Json(new { success = false, message = "Vai trò này đang được sử dụng bởi một hoặc nhiều người dùng và không thể xóa." });
                }

                // Nếu không có người dùng nào đang sử dụng, tiến hành xóa vai trò
                var result = roleManager.Delete(role);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa vai trò." });
                }
            }
            return Json(new { success = false, message = "Vai trò không tồn tại." });
        }


        // Phương thức hỗ trợ để thêm lỗi vào ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}