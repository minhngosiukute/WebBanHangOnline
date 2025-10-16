using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models
{
    public class SearchItem
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string LocalUrl { get; set; }
        public string ImageUrl { get; set; }
        public double Score { get; set; }
    }

}