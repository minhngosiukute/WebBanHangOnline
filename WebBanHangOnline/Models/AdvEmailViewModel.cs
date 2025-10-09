using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHangOnline.Models
{
    public class AdvEmailViewModel
    {
        public int AdvId { get; set; }
        public List<int> SelectedSubscriberIds { get; set; } = new List<int>();

        // Dùng để render UI
        public IEnumerable<SelectListItem> AdvOptions { get; set; }
        public List<SubscriberItem> Subscribers { get; set; } = new List<SubscriberItem>();
    }

    public class SubscriberItem
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool Selected { get; set; }
    }
}