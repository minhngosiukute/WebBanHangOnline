using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using System.Data.Entity.SqlServer; // để dùng SqlFunctions.DatePart

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }

        
        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate, string period = "day")
        {
            period = (period ?? "day").ToLowerInvariant();

            var q = from o in db.Orders
                    join od in db.OrderDetails on o.Id equals od.OrderId
                    join p in db.Products on od.ProductId equals p.Id
                    select new
                    {
                        o.CreatedDate,
                        od.Quantity,
                        od.Price,
                        p.OriginalPrice
                    };

            // lọc theo ngày (bao trọn ngày toDate)
            if (!string.IsNullOrEmpty(fromDate))
            {
                var start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
                q = q.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= start);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                var end = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date.AddDays(1);
                q = q.Where(x => DbFunctions.TruncateTime(x.CreatedDate) < end);
            }

            if (period == "day")
            {
                var grouped = q.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate))
                               .Select(g => new
                               {
                                   Date = g.Key, // Nullable<DateTime>
                                   TotalBuy = g.Sum(y => (decimal?)(y.Quantity * y.OriginalPrice)) ?? 0m,
                                   TotalSell = g.Sum(y => (decimal?)(y.Quantity * y.Price)) ?? 0m
                               })
                               .OrderBy(g => g.Date)
                               .AsEnumerable()
                               .Select(x => new
                               {
                                   Label = x.Date.HasValue ? x.Date.Value.ToString("dd-MM-yyyy") : "",
                                   DoanhThu = x.TotalSell,
                                   LoiNhuan = x.TotalSell - x.TotalBuy
                               })
                               .ToList();

                return Json(new { Data = grouped }, JsonRequestBehavior.AllowGet);
            }

            if (period == "week")
            {
                // ISO week: dùng DatePart('iso_week'). Năm lấy theo Year thường (đủ dùng).
                var grouped = q.GroupBy(x => new
                {
                    Year = x.CreatedDate.Year,
                    Week = SqlFunctions.DatePart("iso_week", x.CreatedDate)
                })
                               .Select(g => new
                               {
                                   Year = g.Key.Year,
                                   Week = (g.Key.Week ?? 0),
                                   TotalBuy = g.Sum(y => (decimal?)(y.Quantity * y.OriginalPrice)) ?? 0m,
                                   TotalSell = g.Sum(y => (decimal?)(y.Quantity * y.Price)) ?? 0m
                               })
                               .OrderBy(g => g.Year).ThenBy(g => g.Week)
                               .ToList();

                // Trả về Year/Week để JS buildLabel() vẽ khoảng ngày trong tuần
                var data = grouped.Select(x => new
                {
                    x.Year,
                    x.Week,
                    DoanhThu = x.TotalSell,
                    LoiNhuan = x.TotalSell - x.TotalBuy
                });

                return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
            }

            if (period == "month")
            {
                var grouped = q.GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                               .Select(g => new
                               {
                                   g.Key.Year,
                                   g.Key.Month,
                                   TotalBuy = g.Sum(y => (decimal?)(y.Quantity * y.OriginalPrice)) ?? 0m,
                                   TotalSell = g.Sum(y => (decimal?)(y.Quantity * y.Price)) ?? 0m
                               })
                               .OrderBy(g => g.Year).ThenBy(g => g.Month)
                               .AsEnumerable()
                               .Select(x => new
                               {
                                   Label = $"{x.Month:00}/{x.Year}", // JS đang expect item.Label cho "month"
                                   DoanhThu = x.TotalSell,
                                   LoiNhuan = x.TotalSell - x.TotalBuy
                               })
                               .ToList();

                return Json(new { Data = grouped }, JsonRequestBehavior.AllowGet);
            }

            // year
            {
                var grouped = q.GroupBy(x => x.CreatedDate.Year)
                               .Select(g => new
                               {
                                   Year = g.Key,
                                   TotalBuy = g.Sum(y => (decimal?)(y.Quantity * y.OriginalPrice)) ?? 0m,
                                   TotalSell = g.Sum(y => (decimal?)(y.Quantity * y.Price)) ?? 0m
                               })
                               .OrderBy(g => g.Year)
                               .AsEnumerable()
                               .Select(x => new
                               {
                                   Label = x.Year.ToString(), // JS đang expect item.Label cho "year"
                                   DoanhThu = x.TotalSell,
                                   LoiNhuan = x.TotalSell - x.TotalBuy
                               })
                               .ToList();

                return Json(new { Data = grouped }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}