using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportOrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/ImportOrder
        public ActionResult Index()
        {
            var importOrders = db.ImportOrders
                .Include("Supplier")
                .ToList();
            return View(importOrders);
        }

        // GET: Admin/ImportOrder/Details/5
        public ActionResult Details(int id)
        {
            var importOrder = db.ImportOrders
                .Include("Supplier")
                .Include("ImportOrderDetails.Product")
                .FirstOrDefault(io => io.Id == id);

            if (importOrder == null)
            {
                return HttpNotFound();
            }

            return View(importOrder);
        }

        // GET: Admin/ImportOrder/Create
        public ActionResult Create()
        {
            // Lấy danh sách nhà cung cấp để hiển thị trong dropdown
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "Id", "SupplierName");

            // Lấy danh sách sản phẩm để hiển thị trong form
            ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();

            return View();
        }

        // POST: Admin/ImportOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public ActionResult Create(ImportOrder model, List<int> ProductIds, List<int> Quantities, List<decimal> ImportPrices)
        {
            // ImportCode sinh ở server → bỏ validation field này
            ModelState.Remove("ImportCode");

            // Chuẩn hoá list giá nhập để tránh IndexOutOfRange
            if (ImportPrices == null) ImportPrices = new List<decimal>();
            while (ImportPrices.Count < (ProductIds?.Count ?? 0)) ImportPrices.Add(0m);

            // Kiểm tra có ít nhất 1 dòng hợp lệ
            bool hasValidRow = false;
            if (ProductIds != null && Quantities != null)
            {
                for (int i = 0; i < ProductIds.Count; i++)
                {
                    if (ProductIds[i] > 0 && i < Quantities.Count && Quantities[i] > 0)
                    {
                        hasValidRow = true;
                        break;
                    }
                }
            }
            if (!hasValidRow)
                ModelState.AddModelError("", "Phải thêm ít nhất một sản phẩm hợp lệ (đã chọn và số lượng > 0).");

            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "Id", "SupplierName", model.SupplierId);
                ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();
                return View(model);
            }

            try
            {
                // Sinh mã phiếu nhập (độc nhất theo thời điểm)
                model.ImportCode = "PN" + DateTime.Now.ToString("yyyyMMddHHmmss");
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.ImportDate = DateTime.Now;
                model.IsCompleted = false;

                db.ImportOrders.Add(model);
                db.SaveChanges(); // để có model.Id

                decimal totalAmount = 0m;

                for (int i = 0; i < ProductIds.Count; i++)
                {
                    // Bỏ qua dòng không hợp lệ
                    if (ProductIds[i] <= 0 || i >= Quantities.Count || Quantities[i] <= 0) continue;

                    var product = db.Products.Find(ProductIds[i]);
                    if (product == null) continue;

                    // Lấy giá nhập để LƯU VÀO CHI TIẾT (KHÔNG cập nhật sản phẩm ở bước Create)
                    decimal importPrice = ImportPrices[i] > 0 ? ImportPrices[i] : product.OriginalPrice;

                    var detail = new ImportOrderDetail
                    {
                        ImportOrderId = model.Id,
                        ProductId = ProductIds[i],
                        Quantity = Quantities[i],
                        ImportPrice = importPrice,
                        TotalPrice = Quantities[i] * importPrice,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.ImportOrderDetails.Add(detail);

                    totalAmount += detail.TotalPrice;
                }

                model.TotalAmount = totalAmount;
                db.Entry(model).State = System.Data.Entity.EntityState.Modified; // cập nhật TotalAmount
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo phiếu nhập hàng: " + ex.Message);
                ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "Id", "SupplierName", model.SupplierId);
                ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();
                return View(model);
            }
        }



        // Cập nhật trạng thái đơn hàng (AJAX)
        [HttpPost]
       
        public JsonResult UpdateStatus(int id, bool isCompleted)
        {
            var importOrder = db.ImportOrders
                .Include("ImportOrderDetails")
                .FirstOrDefault(x => x.Id == id);

            if (importOrder == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu nhập hàng." });

            if (importOrder.IsCompleted == isCompleted)
                return Json(new { success = true, message = "Trạng thái không thay đổi." });

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    // Khi chuyển sang Hoàn thành
                    if (!importOrder.IsCompleted && isCompleted)
                    {
                        foreach (var detail in importOrder.ImportOrderDetails)
                        {
                            var product = db.Products.Find(detail.ProductId);
                            if (product == null) continue;

                            // ✅ Cập nhật giá gốc theo giá nhập của phiếu nhập
                            if (detail.ImportPrice > 0)
                            {
                                product.OriginalPrice = detail.ImportPrice;
                            }

                            // ✅ Cộng thêm số lượng nhập vào kho
                            product.Quantity += detail.Quantity;

                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    // Khi hoàn tác (chuyển từ Hoàn thành -> Chưa hoàn thành)
                    else if (importOrder.IsCompleted && !isCompleted)
                    {
                        foreach (var detail in importOrder.ImportOrderDetails)
                        {
                            var product = db.Products.Find(detail.ProductId);
                            if (product == null) continue;

                            // ❗ Kiểm tra tồn kho đủ trừ không
                            if (product.Quantity < detail.Quantity)
                            {
                                tran.Rollback();
                                return Json(new
                                {
                                    success = false,
                                    message = $"Không thể hoàn tác, sản phẩm '{product.Title}' tồn kho hiện tại không đủ ({product.Quantity})."
                                });
                            }

                            // ✅ Trừ lại số lượng
                            product.Quantity -= detail.Quantity;

                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    // Cập nhật trạng thái phiếu nhập
                    importOrder.IsCompleted = isCompleted;
                    importOrder.ModifiedDate = DateTime.Now;
                    db.Entry(importOrder).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    tran.Commit();

                    return Json(new { success = true, message = "Cập nhật trạng thái và kho thành công." });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
                }
            }
        }


        // Xóa phiếu nhập hàng
        [HttpPost]
        public JsonResult DeleteConfirmed(int id)
        {
            var order = db.ImportOrders.Find(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiếu nhập hàng." });
            }

            db.ImportOrders.Remove(order);
            db.SaveChanges();

            return Json(new { success = true, message = "Xóa phiếu nhập hàng thành công!" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}