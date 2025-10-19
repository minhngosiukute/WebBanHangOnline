using System;
using System.Collections.Generic;
using System.Data.Entity; // EF6
using System.Linq;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models.Common
{
    public class StatsSummaryVM
    {
        public long Today { get; set; }
        public long ThisWeek { get; set; }
        public long ThisMonth { get; set; }
    }

    public class DayCountVM
    {
        public DateTime Day { get; set; }
        public long Count { get; set; }
    }

    public static class ThongKeService
    {
        // Gọi trong Session_Start để cộng lượt hôm nay
        public static void IncrementToday(ApplicationDbContext db)
        {
            var today = DateTime.Today;
            var tk = db.AccessStats.FirstOrDefault(x => DbFunctions.TruncateTime(x.Time) == today);
            if (tk == null)
            {
                db.AccessStats.Add(new AccessStat { Time = today, VisitCount = 1 });
            }
            else
            {
                tk.VisitCount += 1;
            }
            db.SaveChanges();
        }

        // Tóm tắt: hôm nay - tuần này - tháng này
        public static StatsSummaryVM GetSummary(ApplicationDbContext db)
        {
            var today = DateTime.Today;

            // Tuần bắt đầu Thứ 2
            var dow = (int)today.DayOfWeek;               // Sun = 0
            var monday = today.AddDays(dow == 0 ? -6 : 1 - dow);
            var weekEnd = monday.AddDays(7);              // tính sẵn ở ngoài

            var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
            var monthEnd = firstDayThisMonth.AddMonths(1); // tính sẵn ở ngoài

            long todayCount = db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) == today)
                .Select(x => (long?)x.VisitCount).FirstOrDefault() ?? 0;

            long weekCount = db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) >= monday
                         && DbFunctions.TruncateTime(x.Time) < weekEnd)
                .Select(x => (long?)x.VisitCount).Sum() ?? 0;

            long monthCount = db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) >= firstDayThisMonth
                         && DbFunctions.TruncateTime(x.Time) < monthEnd)
                .Select(x => (long?)x.VisitCount).Sum() ?? 0;

            return new StatsSummaryVM
            {
                Today = todayCount,
                ThisWeek = weekCount,
                ThisMonth = monthCount
            };
        }

        // Lấy view theo 1 ngày
        public static long GetByDay(ApplicationDbContext db, DateTime day)
        {
            var d = day.Date;
            return db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) == d)
                .Select(x => (long?)x.VisitCount).FirstOrDefault() ?? 0;
        }

        // Tổng view theo tháng (year + month)
        public static long GetByMonth(ApplicationDbContext db, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1); // tính sẵn ở ngoài
            return db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) >= start
                         && DbFunctions.TruncateTime(x.Time) < end)
                .Select(x => (long?)x.VisitCount).Sum() ?? 0;
        }

        // Chuỗi daily cho 1 tháng (để vẽ chart nếu cần)
        public static List<DayCountVM> GetDailySeriesOfMonth(ApplicationDbContext db, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1); // tính sẵn ở ngoài

            var raw = db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) >= start
                         && DbFunctions.TruncateTime(x.Time) < end)
                .Select(x => new { Day = DbFunctions.TruncateTime(x.Time), x.VisitCount })
                .ToList()
                .GroupBy(x => x.Day.Value)
                .Select(g => new DayCountVM { Day = g.Key, Count = g.Sum(z => (long)z.VisitCount) })
                .ToList();

            // fill thiếu ngày
            var days = new List<DayCountVM>();
            for (var d = start; d < end; d = d.AddDays(1))
            {
                var found = raw.FirstOrDefault(x => x.Day.Date == d.Date);
                days.Add(new DayCountVM { Day = d, Count = found?.Count ?? 0 });
            }
            return days;
        }
        public static List<DayCountVM> GetDailySeriesOfWeek(ApplicationDbContext db, DateTime anyDateInWeek)
        {
            var today = anyDateInWeek.Date;
            var dow = (int)today.DayOfWeek; // Sun=0
            var monday = today.AddDays(dow == 0 ? -6 : 1 - dow);
            var weekEnd = monday.AddDays(7);

            var raw = db.AccessStats
                .Where(x => DbFunctions.TruncateTime(x.Time) >= monday
                         && DbFunctions.TruncateTime(x.Time) < weekEnd)
                .Select(x => new { Day = DbFunctions.TruncateTime(x.Time), x.VisitCount })
                .ToList()
                .GroupBy(x => x.Day.Value)
                .Select(g => new DayCountVM { Day = g.Key, Count = g.Sum(z => (long)z.VisitCount) })
                .ToList();

            var days = new List<DayCountVM>();
            for (var d = monday; d < weekEnd; d = d.AddDays(1))
            {
                var found = raw.FirstOrDefault(x => x.Day == d);
                days.Add(new DayCountVM { Day = d, Count = found?.Count ?? 0 });
            }
            return days; // đúng 7 ngày chứa ngày lọc
        }




    }
}
