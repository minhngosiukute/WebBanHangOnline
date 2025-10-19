using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("AccessStats")] // Tạo bảng mới
    public class AccessStat
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int VisitCount { get; set; }
    }
}