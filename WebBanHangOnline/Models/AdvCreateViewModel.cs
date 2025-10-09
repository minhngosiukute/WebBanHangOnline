using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHangOnline.Models
{
    public class AdvCreateViewModel
    {
        [Required, StringLength(150)]
        public string Title { get; set; }
        [AllowHtml]
     
        public string Description { get; set; }

        //[StringLength(500)]
        //public string Image { get; set; }

        //[StringLength(500)]
        public string Link { get; set; }

        //public int Type { get; set; }
    }
}