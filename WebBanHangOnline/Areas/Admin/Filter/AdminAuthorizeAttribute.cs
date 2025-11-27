using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBanHangOnline.Areas.Admin.Filter
{
    // Attribute này kế thừa từ AuthorizeAttribute mặc định của MVC
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Nếu chưa đăng nhập hoặc không đủ quyền, chuyển hướng về trang đăng nhập của Admin.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            // Nếu người dùng chưa đăng nhập
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Chuyển hướng về trang đăng nhập của admin
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        area = "Admin",
                        controller = "Account",
                        action = "Login",
                        ReturnUrl = request.RawUrl // Giữ lại URL để quay về sau khi đăng nhập
                    })
                );
                return;
            }

            // Nếu đã đăng nhập nhưng không có quyền phù hợp
            if (!string.IsNullOrEmpty(Roles))
            {
                if (!filterContext.HttpContext.User.IsInRole(Roles))
                {
                    // Chuyển đến trang báo lỗi 403 hoặc redirect tùy bạn
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Areas/Admin/Views/Shared/AccessDenied.cshtml"
                    };
                    return;
                }
            }

            // Nếu hợp lệ, tiếp tục xử lý như bình thường
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
