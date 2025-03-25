using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo phiếu nhập hàng
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.ImportDate = DateTime.Now; // Gán ngày nhập hàng
                    model.IsCompleted = false; // Mặc định chưa hoàn thành
                    db.ImportOrders.Add(model);
                    db.SaveChanges();

                    // Thêm chi tiết phiếu nhập hàng và cập nhật số lượng sản phẩm
                    decimal totalAmount = 0;
                    if (ProductIds != null && Quantities != null)
                    {
                        for (int i = 0; i < ProductIds.Count; i++)
                        {
                            if (ProductIds[i] > 0 && Quantities[i] > 0)
                            {
                                // Lấy giá nhập (OriginalPrice) từ bảng Product
                                var product = db.Products.Find(ProductIds[i]);
                                if (product == null)
                                {
                                    ModelState.AddModelError("", $"Sản phẩm với ID {ProductIds[i]} không tồn tại.");
                                    continue;
                                }

                                // Xác định giá nhập: nếu người dùng nhập giá mới thì sử dụng, nếu không thì dùng OriginalPrice
                                decimal importPrice;
                                if (ImportPrices != null && i < ImportPrices.Count && ImportPrices[i] > 0)
                                {
                                    importPrice = ImportPrices[i];
                                    // Cập nhật OriginalPrice của sản phẩm
                                    product.OriginalPrice = importPrice;
                                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    importPrice = product.OriginalPrice;
                                }

                                // Tạo chi tiết phiếu nhập hàng
                                var detail = new ImportOrderDetail
                                {
                                    ImportOrderId = model.Id,
                                    ProductId = ProductIds[i],
                                    Quantity = Quantities[i],
                                    ImportPrice = importPrice, // Sử dụng giá nhập đã xác định
                                    TotalPrice = Quantities[i] * importPrice, // Tính TotalPrice
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now
                                };
                                db.ImportOrderDetails.Add(detail);

                                // Cập nhật số lượng sản phẩm trong bảng Product
                                product.Quantity += Quantities[i];
                                db.Entry(product).State = System.Data.Entity.EntityState.Modified;

                                totalAmount += detail.TotalPrice;
                            }
                        }

                        // Cập nhật TotalAmount cho ImportOrder
                        model.TotalAmount = totalAmount;
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo phiếu nhập hàng: " + ex.Message);
                }
            }

            // Nếu có lỗi, trả lại view với dữ liệu đã nhập
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "Id", "SupplierName", model.SupplierId);
            ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();
            return View(model);
        }

        // Cập nhật trạng thái đơn hàng (AJAX)
        [HttpPost]
        public JsonResult UpdateStatus(int id, bool isCompleted)
        {
            System.Diagnostics.Debug.WriteLine($"Nhận dữ liệu: id={id}, isCompleted={isCompleted}");

            var importOrder = db.ImportOrders.Find(id);
            if (importOrder == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiếu nhập hàng." });
            }

            try
            {
                importOrder.IsCompleted = isCompleted;
                importOrder.ModifiedDate = DateTime.Now;
                db.Entry(importOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
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