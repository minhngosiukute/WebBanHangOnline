using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.Common;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Stats/Summary (Partial hoặc Json đều được)
        public ActionResult Index()
        {
            // Có thể load sẵn dữ liệu tổng quan ban đầu (hôm nay / tuần này / tháng này / online)
            var s = ThongKeService.GetSummary(db);

            ViewBag.Today = s.Today.ToString("#,###");
            ViewBag.ThisWeek = s.ThisWeek.ToString("#,###");
            ViewBag.ThisMonth = s.ThisMonth.ToString("#,###");
            ViewBag.Online = Convert.ToInt32(HttpContext.Application["visitors_online"] ?? 0);

            return View(); // → render ra Areas/Admin/Views/Stats/Index.cshtml
        }
        public ActionResult Summary()
        {
            var s = ThongKeService.GetSummary(db);
            var vm = new
            {
                Today = s.Today.ToString("#,###"),
                ThisWeek = s.ThisWeek.ToString("#,###"),
                ThisMonth = s.ThisMonth.ToString("#,###"),
                Online = Convert.ToInt32(HttpContext.Application["visitors_online"] ?? 0)
            };
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Stats/Filter?mode=day&date=2025-10-10
        // GET: Admin/Stats/Filter?mode=month&year=2025&month=10
        public ActionResult Filter(string mode, DateTime? date, int? year, int? month)
        {
            if (string.Equals(mode, "day", StringComparison.OrdinalIgnoreCase))
            {
                var d = (date ?? DateTime.Today).Date;
                var value = ThongKeService.GetByDay(db, d);
                return Json(new
                {
                    Mode = "day",
                    Date = d.ToString("dd-MM-yyyy"),
                    Value = value,
                    ValueText = value.ToString("#,###")
                }, JsonRequestBehavior.AllowGet);
            }

            if (string.Equals(mode, "month", StringComparison.OrdinalIgnoreCase))
            {
                var y = year ?? DateTime.Today.Year;
                var m = month ?? DateTime.Today.Month;
                var value = ThongKeService.GetByMonth(db, y, m);
                var series = ThongKeService.GetDailySeriesOfMonth(db, y, m)
                    .Select(x => new { day = x.Day.ToString("dd-MM-yyyy"), count = x.Count });

                return Json(new
                {
                    Mode = "month",
                    Year = y,
                    Month = m,
                    Value = value,
                    ValueText = value.ToString("#,###"),
                    Series = series
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.Equals(mode, "week", StringComparison.OrdinalIgnoreCase))
            {
                var selected = (date ?? DateTime.Today).Date;
                var list = ThongKeService.GetDailySeriesOfWeek(db, selected);
                var total = list.Sum(x => x.Count);

                // Tính lại mốc tuần để trả về cho UI (hiển thị tiêu đề)
                var dow = (int)selected.DayOfWeek; // Sun=0
                var weekStart = selected.AddDays(dow == 0 ? -6 : 1 - dow);
                var weekEnd = weekStart.AddDays(7).AddSeconds(-1); // để show text

                return Json(new
                {
                    Mode = "week",
                    SelectedDate = selected.ToString("dd-MM-yyyy"),
                    WeekStart = weekStart.ToString("dd-MM-yyyy"),
                    WeekEnd = weekEnd.ToString("dd-MM-yyyy"),
                    Value = total,
                    ValueText = total.ToString("#,###"),
                    Series = list.Select(x => new { day = x.Day.ToString("dd-MM-yyyy"), count = x.Count })
                }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { error = "Mode must be 'day' or 'month'." }, JsonRequestBehavior.AllowGet);
        }
    }
}
