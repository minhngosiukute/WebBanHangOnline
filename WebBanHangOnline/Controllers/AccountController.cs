using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

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


        public async Task<ActionResult> Profile()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var item = new CreateAccountViewModel();
            item.Email = user.Email;
            item.FullName = user.FullName;
            item.Phone = user.Phone;
            item.UserName = user.UserName;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostProfile(CreateAccountViewModel req)
        {
            var user = await UserManager.FindByEmailAsync(req.Email);
            user.FullName = req.FullName;
            user.Phone = req.Phone;
            var rs = await UserManager.UpdateAsync(user);
            if (rs.Succeeded)
            {
                return RedirectToAction("Profile");
            }
            return View(req);
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
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true
        //    var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            return View(model);
        //    }
        //}
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            // Tìm user theo UserName (vì bạn login bằng username)
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                var confirmed = await UserManager.IsEmailConfirmedAsync(user.Id);
                if (!confirmed)
                {
                    ModelState.AddModelError("", "Tài khoản chưa xác nhận email. Vui lòng kiểm tra hộp thư.");
                    return View(model);
                }
            }

            var result = await SignInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success: return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut: return View("Lockout");
                default:
                    ModelState.AddModelError("", "Đăng nhập không hợp lệ.");
                    return View(model);
            }
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }


        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.ReturnUrl = Url.Action("Index", "Home");
            return View();
        }
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid) return View(model);

        //    // Chuẩn hóa
        //    var inputUserName = model.UserName?.Trim();
        //    var inputEmail = model.Email?.Trim().ToLowerInvariant();

        //    // Kiểm tra trùng
        //    if (await UserManager.FindByNameAsync(inputUserName) != null)
        //    {
        //        ModelState.AddModelError("UserName", "Tài khoản này đã tồn tại.");
        //        return View(model);
        //    }
        //    if (await UserManager.FindByEmailAsync(inputEmail) != null)
        //    {
        //        ModelState.AddModelError("Email", "Email này đã được sử dụng.");
        //        return View(model);
        //    }

        //    // Tạo user: UserName ≠ Email
        //    var user = new ApplicationUser
        //    {
        //        UserName = inputUserName,   // ← dùng username riêng
        //        Email = inputEmail
        //    };

        //    var result = await UserManager.CreateAsync(user, model.Password);
        //    if (result.Succeeded)
        //    {

        //        await UserManager.AddToRoleAsync(user.Id, "Customer");

        //        await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
        //        return RedirectToAction("Index", "Home");
        //    }

        //    foreach (var e in result.Errors) ModelState.AddModelError("", e);
        //    return View(model);
        //}


        //POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Index", "Home");
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Tạo token xác nhận email
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                // Link xác nhận
                var callbackUrl = Url.Action(
                    "ConfirmEmail", "Account",
                    new { userId = user.Id, code = code },
                    protocol: Request.Url.Scheme);

                // Gửi email (dùng hàm có sẵn của bạn)
                WebBanHangOnline.Common.Common.SendMail(
                    "ShopOnline",
                    "Xác nhận tài khoản",
                    $"Chào {model.UserName}, vui lòng xác nhận tài khoản bằng cách bấm vào <a href='{callbackUrl}'>liên kết này</a>.",
                    model.Email
                );

                // Gán role mặc định (nếu bạn muốn gán ngay từ lúc tạo)
                await UserManager.AddToRoleAsync(user.Id, "Customer");

                ViewBag.SuccessMessage = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                ModelState.Clear(); // xóa dữ liệu form
                return View();
            }

            AddErrors(result);
            return View(model);
        }


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ tìm theo Email (vì UserName ≠ Email)
                var user = await UserManager.FindByEmailAsync(model.Email);

                // ✅ kiểm tra null trước khi dùng user.Id
                if (user == null)
                {
                    // Không tiết lộ user có tồn tại hay không
                    return View("ForgotPasswordConfirmation");
                }

                // (tuỳ bạn có dùng xác nhận email hay không)
                var isConfirmed = await UserManager.IsEmailConfirmedAsync(user.Id);
                // nếu không yêu cầu xác nhận email thì có thể bỏ cả 2 dòng trên

                // Gửi link reset mật khẩu
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action(
                    "ResetPassword", "Account",
                    new { userId = user.Id, code = code },
                    protocol: Request.Url.Scheme);

                WebBanHangOnline.Common.Common.SendMail(
                    "ShopOnline", "Quên mật khẩu",
                    "Bạn click vào <a href='" + callbackUrl + "'>link này</a> để reset mật khẩu",
                    model.Email);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Nếu ModelState không hợp lệ
            return View(model);
        }


        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    if (!string.IsNullOrEmpty(loginInfo.Email))
                    {
                        var existingUser = await UserManager.FindByEmailAsync(loginInfo.Email);
                        if (existingUser == null)
                        {
                            var newUser = new ApplicationUser
                            {
                                UserName = loginInfo.Email,
                                Email = loginInfo.Email,
                                EmailConfirmed = true,
                                FullName = loginInfo.ExternalIdentity?.FindFirstValue(ClaimTypes.Name)
                            };

                            var createResult = await UserManager.CreateAsync(newUser);
                            if (createResult.Succeeded)
                            {
                                await UserManager.AddToRoleAsync(newUser.Id, "Customer");
                                var addLoginResult = await UserManager.AddLoginAsync(newUser.Id, loginInfo.Login);
                                if (addLoginResult.Succeeded)
                                {
                                    await SignInManager.SignInAsync(newUser, isPersistent: false, rememberBrowser: false);
                                    return RedirectToLocal(returnUrl);
                                }

                                AddErrors(addLoginResult);
                            }
                            else
                            {
                                AddErrors(createResult);
                            }
                        }
                        else
                        {
                            if (!existingUser.EmailConfirmed)
                            {
                                existingUser.EmailConfirmed = true;
                                await UserManager.UpdateAsync(existingUser);
                            }

                            var logins = await UserManager.GetLoginsAsync(existingUser.Id);
                            var hasLogin = logins.Any(l => l.LoginProvider == loginInfo.Login.LoginProvider && l.ProviderKey == loginInfo.Login.ProviderKey);

                            if (!hasLogin)
                            {
                                var addLoginResult = await UserManager.AddLoginAsync(existingUser.Id, loginInfo.Login);
                                if (!addLoginResult.Succeeded)
                                {
                                    AddErrors(addLoginResult);
                                }
                            }

                            await SignInManager.SignInAsync(existingUser, isPersistent: false, rememberBrowser: false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await UserManager.UpdateAsync(user);
                    await UserManager.AddToRoleAsync(user.Id, "Customer");

                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            
            Session.Remove("Cart");    

            
            Session.Clear();
            Session.Abandon();

            
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("Index", "Home");
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
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

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}