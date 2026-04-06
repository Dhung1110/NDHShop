using LiteCommerce.BusinessLayers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Partner;
using SV22T1020146.Models.Sales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020146.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const int PAGE_SIZE = 5;
        public const string SEARCH_ORDER = "SearchOrder";
        public const string SEARCH_PRODUCT = "SearchProduct";

        #region Danh sách đơn hàng

        /// <summary>
        /// Hiển thị trang chính + load điều kiện tìm kiếm từ Session
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(SEARCH_ORDER);

            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0
                };
            }

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm + phân trang danh sách đơn hàng
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            input.PageSize = PAGE_SIZE;

            var result = await SalesDataService.ListOrdersAsync(input);

            ApplicationContext.SetSessionData(SEARCH_ORDER, input);

            return View(result);
        }

        #endregion

        #region Tạo đơn hàng

        /// <summary>
        /// Hiển thị form tạo đơn hàng và giỏ hàng tạm
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        public async Task<IActionResult> Create()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_PRODUCT)
                        ?? new PaginationSearchInput()
                        {
                            Page = 1,
                            PageSize = PAGE_SIZE,
                            SearchValue = ""
                        };

            ViewBag.Details = ShoppingCartHelper.GetShoppingCart();
            ViewBag.OrderID = 0;

            List<Customer> allCustomers = new List<Customer>();
            int page = 1;
            int pageSize = 100;

            PaginationSearchInput pageInput;

            while (true)
            {
                pageInput = new PaginationSearchInput()
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = ""
                };

                var result = await PartnerDataService.ListCustomersAsync(pageInput);
                if (result == null || result.DataItems.Count == 0)
                    break;

                allCustomers.AddRange(result.DataItems);

                if (result.DataItems.Count < pageSize)
                    break; 

                page++; 
            }

            ViewBag.Customers = allCustomers;
            // Load tỉnh/thành
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            ViewBag.SelectedCustomerID = HttpContext.Session.GetInt32("order_customerID") ?? 0;
            ViewBag.SelectedProvince = HttpContext.Session.GetString("order_province") ?? "";
            ViewBag.SelectedAddress = HttpContext.Session.GetString("order_address") ?? "";

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm để thêm vào đơn hàng (AJAX)
        /// </summary>
        public async Task<IActionResult> SearchProduct(PaginationSearchInput input, int page = 1)
        {
            input.Page = page <= 0 ? 1 : page;
            input.PageSize = PAGE_SIZE;

            var result = await CatalogDataService.ListProductsAsync(new ProductSearchInput()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                CategoryID = 0
            });

            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);

            return View(result);
        }

        #endregion

        #region Giỏ hàng (Shopping Cart)
        public static OrderDetailViewInfo? GetCartItem(int productId)
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return cart.FirstOrDefault(x => x.ProductID == productId);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng tạm (OrderID = 0)
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, decimal salePrice = 0)
        {
            if (productId <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ";
                return RedirectToAction("Create");
            }

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Create");
            }

            ShoppingCartHelper.AddItemToCart(new OrderDetailViewInfo()
            {
                ProductID = productId,
                ProductName = product.ProductName,
                Quantity = quantity,
                SalePrice = product.Price,
                Photo = product.Photo
            });

            return RedirectToAction("Create");
        }

        /// <summary>
        /// Hiển thị form cập nhật mặt hàng trong giỏ
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        public IActionResult EditCartItem(int productId)
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);

            if (item == null)
                return RedirectToAction("Create");

            return PartialView(item);
        }

        /// <summary>
        /// Cập nhật thông tin mặt hàng trong giỏ
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        [HttpPost]
        public IActionResult EditCartItem(int productId, int quantity, decimal salePrice)
        {
            ShoppingCartHelper.UpdateItemInCart(productId, quantity, salePrice);
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi giỏ
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        public IActionResult DeleteCartItem(int productId)
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);

            if (item == null)
                return RedirectToAction("Create");

            return PartialView(item);
        }

        /// <summary>
        /// Xác nhận xóa mặt hàng khỏi giỏ
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        [HttpPost]
        public IActionResult DeleteCartItem(int productId, string confirm)
        {
            ShoppingCartHelper.RemoveItemFromCart(productId);
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        public IActionResult ClearCart()
        {
            return PartialView();
        }

        [HttpPost]
        [Authorize(Roles = "admin,sales")]
        public IActionResult ClearCart(string confirm)
        {
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Create");
        }
        /// <summary>
        /// Lập đơn hàng
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="province"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string province = "", string address = "")
        {
            var cart = ShoppingCartHelper.GetShoppingCart();

            if (cart == null || cart.Count == 0)
                return Json(new { code = 0, message = "Giỏ hàng đang trống!" });

            // Xử lý logic không bắt buộc
            int? customerIdValue = (customerID > 0) ? customerID : null;
            string deliveryProvince = province ?? "";
            string deliveryAddress = address ?? "";

            // 1. Lưu đơn hàng (Sử dụng Service của bạn)
            int orderID = await SalesDataService.AddOrderAsync(customerIdValue, deliveryProvince, deliveryAddress);

            // 2. Lưu chi tiết
            foreach (var item in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });
            }

            ShoppingCartHelper.ClearCart();
            HttpContext.Session.Remove("order_customerID");
            HttpContext.Session.Remove("order_province");
            HttpContext.Session.Remove("order_address");

            return Json(new { code = orderID, message = "Thành công" });
        }
        
        [HttpPost]
        public IActionResult SaveOrderInfo(int customerID = 0, string province = "", string address = "")
        {
            HttpContext.Session.SetInt32("order_customerID", customerID);
            HttpContext.Session.SetString("order_province", province ?? "");
            HttpContext.Session.SetString("order_address", address ?? "");

            return Ok();
        }
        #endregion

        #region ===== EDIT ORDER ITEM =====

        /// <summary>
        /// Hiển thị form sửa mặt hàng
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        [HttpGet]
        public async Task<IActionResult> EditOrderItem(int orderId, int productId)
        {
            var order = await SalesDataService.GetOrderAsync(orderId);
            if (order == null)
                return Content("Đơn hàng không tồn tại");

            // ✅ CHỈ CHO SỬA khi New hoặc Accepted
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
            {
                return Content("Không được phép sửa đơn hàng ở trạng thái này");
            }

            var item = await SalesDataService.GetDetailAsync(orderId, productId);
            if (item == null)
                return Content("Không tìm thấy mặt hàng");

            ViewBag.OrderID = orderId;
            return PartialView(item);
        }

        /// <summary>
        /// Lưu cập nhật mặt hàng
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        [HttpPost]
        public async Task<IActionResult> EditOrderItem(int orderId, int productId, int quantity)
        {
            var order = await SalesDataService.GetOrderAsync(orderId);
            if (order == null)
                return Content("Đơn hàng không tồn tại");

            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
            {
                return Content("Không được phép sửa");
            }

            if (quantity <= 0)
                return Content("Số lượng không hợp lệ");

            var oldItem = await SalesDataService.GetDetailAsync(orderId, productId);
            if (oldItem == null)
                return Content("Không tìm thấy mặt hàng");

            await SalesDataService.UpdateDetailAsync(new OrderDetail
            {
                OrderID = orderId,
                ProductID = productId,
                Quantity = quantity,
                SalePrice = oldItem.SalePrice 
            });

            return RedirectToAction("Detail", new { id = orderId });
        }

        #endregion

        #region ===== DELETE ORDER ITEM =====

        /// <summary>
        /// Hiển thị xác nhận xóa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteOrderItem(int orderId, int productId)
        {
            var order = await SalesDataService.GetOrderAsync(orderId);
            if (order == null)
                return Content("Đơn hàng không tồn tại");

            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
            {
                return Content("Không được phép xóa");
            }

            var item = await SalesDataService.GetDetailAsync(orderId, productId);
            if (item == null)
                return Content("Không tìm thấy mặt hàng");

            ViewBag.OrderID = orderId;
            ViewBag.ProductID = productId; // 🔥 THÊM DÒNG NÀY

            return PartialView(item);
        }

        /// <summary>
        /// Xác nhận xóa
        /// </summary>
        [Authorize(Roles = "admin,sales")]
        [HttpPost]
        [ActionName("DeleteOrderItem")]   
        public async Task<IActionResult> DeleteOrderItemConfirmed(int orderId, int productId)
        {
            var order = await SalesDataService.GetOrderAsync(orderId);
            if (order == null)
                return Content("Đơn hàng không tồn tại");

            // ✅ CHECK TRẠNG THÁI
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
            {
                return Content("Không được phép xóa");
            }

            await SalesDataService.DeleteDetailAsync(orderId, productId);

            return RedirectToAction("Detail", new { id = orderId });
        }

        #endregion

        #region Chi tiết đơn hàng

        /// <summary>
        /// Hiển thị chi tiết đơn hàng và danh sách mặt hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            // 🔥 THÊM: LẤY DANH SÁCH SHIPPER
            var shippers = await PartnerDataService.ListShippersAsync(
                new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 1000,
                    SearchValue = ""
                });

            ViewBag.Shippers = shippers.DataItems;

            // LẤY CUSTOMER
            if (order.CustomerID.HasValue)
            {
                var customer = await PartnerDataService.GetCustomerAsync(order.CustomerID.Value);
                ViewBag.Customer = customer;
            }

            // LẤY CHI TIẾT
            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Details = details;

            return View(order);
        }

        #endregion

        #region Trạng thái đơn hàng
        /// <summary>
        /// Duyệt đơn hàng (chỉ khi đang ở trạng thái New)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [Authorize(Roles = "admin,sales")]
        [HttpGet]
        public IActionResult Accept(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptConfirm(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return NotFound();

            if (order.Status != OrderStatusEnum.New)
            {
                TempData["Error"] = "Không thể duyệt!";
                return RedirectToAction("Detail", new { id });
            }

            await SalesDataService.AcceptOrderAsync(id, 1);

            TempData["Success"] = $"Đã duyệt đơn #{id}";
            return RedirectToAction("Detail", new { id });
        }
        /// <summary>
        /// Giao hàng (chỉ khi đang ở trạng thái Accepted)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,sales")]
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            ViewBag.OrderID = id;

            // Lấy danh sách shipper từ DB
            var shippers = await PartnerDataService.ListShippersAsync(
                new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 1000,
                    SearchValue = ""
                });

            ViewBag.Shippers = shippers.DataItems;

            return PartialView();
        }
        [HttpPost]  // Phải POST để cập nhật DB
        public async Task<IActionResult> Shipping(int id, int shipperId)
        {
            if (shipperId == 0)
            {
                TempData["Error"] = "Bạn phải chọn người giao hàng.";
                return RedirectToAction("Detail", new { id });
            }

            // Gọi service để cập nhật đơn hàng
            await SalesDataService.ShipOrderAsync(id, shipperId);

            return RedirectToAction("Detail", new { id });
        }
        /// <summary>
        /// Hoàn tất đơn hàng (chỉ khi đang ở trạng thái Shipping)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,sales")]
        public async Task<IActionResult> Finish(int id)
        {
            var result = await SalesDataService.CompleteOrderAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Hoàn tất đơn hàng thất bại. Kiểm tra trạng thái đơn hàng.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đơn hàng đã hoàn tất thành công!";
            }

            return RedirectToAction("Detail", new { id });
        }
        /// <summary>
        /// Từ chối đơn hàng (chỉ khi đang ở trạng thái New)
        /// </summary>
        /// <param name="id">ID đơn hàng</param>
        /// <returns></returns>
        [Authorize(Roles = "admin,sales")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status != OrderStatusEnum.New)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối đơn hàng đang ở trạng thái mới.";
                return RedirectToAction("Detail", new { id });
            }

            bool result = await SalesDataService.RejectOrderAsync(id, 1); 
            if (result)
                TempData["SuccessMessage"] = "Đơn hàng đã bị từ chối thành công.";
            else
                TempData["ErrorMessage"] = "Từ chối đơn hàng thất bại.";

            return RedirectToAction("Detail", new { id });
        }
        /// <summary>
        /// Hủy đơn hàng (chỉ khi đang ở trạng thái New hoặc Accepted)
        /// </summary>
        /// <param name="id">ID đơn hàng</param>
        /// <returns></returns>
        [HttpPost] // POST để tránh lỗi CSRF và vô tình click
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,sales")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await SalesDataService.CancelOrderAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Hủy đơn hàng thất bại. Chỉ có thể hủy khi trạng thái New hoặc Accepted.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công!";
            }

            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa đơn hàng (chỉ khi chưa duyệt)
        /// </summary>

        // Action hiển thị giao diện xác nhận xóa (Ajax gọi vào đây)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return NotFound();

            // Nếu không phải trạng thái New → báo lỗi
            if (order.Status != OrderStatusEnum.New)
            {
                ViewBag.Error = "Không thể xóa đơn hàng vì đã được xử lý!";
            }

            return PartialView("Delete", id);
        }

        // Action xử lý xóa thực sự
        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);
            if (result)
                TempData["Success"] = $"Đã xóa đơn hàng #{id} thành công.";
            else
                TempData["Error"] = "Xóa thất bại! Đơn hàng không tồn tại hoặc đã được duyệt.";

            return RedirectToAction("Index");
        }
        #endregion
    }
}