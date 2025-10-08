using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Account
        public ActionResult Index()
        {
            var users = db.Users.ToList();
            var roles = db.Roles.ToList();

            // Map UserId → Danh sách role name
            var rolesMap = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var userRoleIds = user.Roles.Select(r => r.RoleId).ToList();
                var userRoleNames = roles
                    .Where(r => userRoleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToList();

                rolesMap[user.Id] = userRoleNames;
            }

            ViewBag.RolesMap = rolesMap;
            return View(users);
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            var allRoles = db.Roles.Select(r => r.Name).ToList();
            ViewBag.AllRoles = allRoles;
            return View(new CreateAccountViewModel());
        }


        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = db.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var selected = model.Roles ?? new List<string>();
                if (selected.Any())
                {
                    await UserManager.AddToRolesAsync(user.Id, selected.ToArray());
                }
                return RedirectToAction("Index");
            }

            AddErrors(result);
            ViewBag.AllRoles = db.Roles.Select(r => r.Name).ToList();
            return View(model);
        }



        public ActionResult Edit(string id)
        {
            var u = UserManager.FindById(id);
            if (u == null) return HttpNotFound();

            var currentRoles = UserManager.GetRoles(id).ToList();
            var allRoles = db.Roles.Select(r => r.Name).ToList();

            var model = new EditAccountViewModel
            {
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Roles = currentRoles
            };

            ViewBag.AllRoles = allRoles;
            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        // [AllowAnonymous]  // <-- BỎ ĐI, để Admin thôi
        public async Task<ActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Role = new MultiSelectList(db.Roles.ToList(), "Name", "Name", model.Roles);
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.UserName); // hoặc FindById nếu bạn gửi Id
            if (user == null) return HttpNotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            var update = await UserManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                AddErrors(update);
                ViewBag.AllRoles = db.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            // Đồng bộ roles
            var currentRoles = await UserManager.GetRolesAsync(user.Id);

            var selected = model.Roles ?? new List<string>();
            var toAdd = selected.Except(currentRoles).ToArray();
            var toRemove = currentRoles.Except(selected).ToArray();

            if (toAdd.Any()) await UserManager.AddToRolesAsync(user.Id, toAdd);
            if (toRemove.Any()) await UserManager.RemoveFromRolesAsync(user.Id, toRemove);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<ActionResult> DeleteAccount(string user, string id)
        {
            var result = new { Success = false, Message = "Không thể xóa tài khoản đang đăng nhập." };
            var userToDelete = UserManager.FindByName(user);

            // Kiểm tra nếu tài khoản muốn xóa là tài khoản đang đăng nhập
            if (userToDelete == null || userToDelete.Id == User.Identity.GetUserId())
            {
                // Trả về lỗi nếu là tài khoản hiện tại hoặc không tìm thấy tài khoản
                return Json(result);
            }

            // Lấy tất cả vai trò của tài khoản cần xóa
            var rolesForUser = UserManager.GetRoles(id).ToList();
            foreach (var role in rolesForUser)
            {
                await UserManager.RemoveFromRoleAsync(id, role);
            }

            // Thực hiện xóa tài khoản
            var deleteResult = await UserManager.DeleteAsync(userToDelete);
            result = new { Success = deleteResult.Succeeded, Message = deleteResult.Succeeded ? "Xóa tài khoản thành công." : "Có lỗi xảy ra khi xóa tài khoản." };
            return Json(result);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}