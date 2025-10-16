using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

public class SearchController : Controller
{
    private readonly ApplicationDbContext db = new ApplicationDbContext();

    [HttpPost]
    public async Task<ActionResult> Image(HttpPostedFileBase file)
    {
        if (file == null || file.ContentLength == 0)
            return RedirectToAction("Index", "Products");

        using (var http = new HttpClient())
        using (var content = new MultipartFormDataContent())
        {
            // Gửi ảnh lên API Python
            var stream = new StreamContent(file.InputStream);
            stream.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(stream, "file", "query.jpg");

            // Gọi API tìm kiếm hình ảnh (lấy nhiều hơn, rồi cắt top 4)
            var res = await http.PostAsync("http://localhost:8000/search-image?top_k=20", content);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<SearchApiResponse>(json)
                       ?? new SearchApiResponse { results = new List<SearchApiItem>() };

            // 🔍 Tăng ngưỡng độ tương tự lên 0.85 (hoặc 0.8)
            //    và chỉ hiển thị nếu có ít nhất 1 kết quả đạt ngưỡng
            const double THRESHOLD = 0.7;

            var filtered = (data.results ?? new List<SearchApiItem>())
                .Where(r => r.score >= THRESHOLD)
                .OrderByDescending(r => r.score)
                .Take(4)
                .ToList();

            // ⚠️ Nếu không có kết quả nào đủ giống thì trả view trống
            if (!filtered.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm phù hợp với hình đã chọn.";
                return View("~/Views/Products/Index.cshtml", new List<Product>());
            }


            if (filtered.Count == 0)
                return View("~/Views/Products/Index.cshtml", new List<Product>());

            // Lấy danh sách ID theo thứ tự độ giống
            var orderedIds = filtered
                .Select(r => { int id; return int.TryParse(r.id, out id) ? (int?)id : null; })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            // Truy vấn từ DB và sắp xếp lại đúng thứ tự
            var products = db.Products.Where(p => orderedIds.Contains(p.Id)).ToList();
            var orderMap = orderedIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            products = products
                .OrderBy(p => orderMap.ContainsKey(p.Id) ? orderMap[p.Id] : int.MaxValue)
                .ToList();

            // ✅ Trả về view danh sách sản phẩm (Index)
            return View("~/Views/Products/Index.cshtml", products);
        }
    }

    // Models phụ trợ
    class SearchApiResponse { public List<SearchApiItem> results { get; set; } }

    class SearchApiItem
    {
        public string id, name, category, price, local_url, image_url, tags;
        public double score; // 0..1
    }
}
