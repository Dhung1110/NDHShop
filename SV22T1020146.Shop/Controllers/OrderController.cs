using LiteCommerce.BusinessLayers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Sales;
using SV22T1020146.Shop.Models;
using System.Linq;

namespace SV22T1020146.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private int GetCustomerID()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        /// <summary>
        /// Giao diện xác nhận đặt hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            int customerID = GetCustomerID();
            var customer = await PartnerDataService.GetCustomerAsync(customerID);
            ViewBag.Customer = customer;

            return View(cart);
        }

        /// <summary>
        /// Xử lý tạo đơn hàng khi bấm nút "Đặt hàng"
        /// </summary>
        [HttpPost] // 🔥 QUAN TRỌNG: Phải có HttpPost để phân biệt với hàm Get bên trên
        public async Task<IActionResult> Checkout(string province = "", string address = "")
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index", "Cart");

            if (string.IsNullOrEmpty(province) || string.IsNullOrEmpty(address))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin giao hàng";
                return RedirectToAction("Checkout");
            }

            int customerID = GetCustomerID();

            // 1. Tạo đơn hàng và lấy OrderID
            int orderID = await SalesDataService.AddOrderAsync(customerID, province, address);

            // 2. Lưu chi tiết đơn hàng
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

            // 3. Xóa giỏ hàng và chuyển đến trang theo dõi
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Tracking", new { orderId = orderID });
        }

        /// <summary>
        /// Lịch sử mua hàng (Hỗ trợ lọc theo trạng thái từ Tab)
        /// </summary>
        public async Task<IActionResult> History(int status = 0)
        {
            int customerID = GetCustomerID();

            var input = new OrderSearchInput()
            {
                Page = 1,
                PageSize = 100, // Lấy danh sách dài để người dùng cuộn xem
                CustomerID = customerID,
                Status = (OrderStatusEnum)status // Ép kiểu để khớp với Enum Status
            };

            var result = await SalesDataService.ListOrdersAsync(input);

            var model = new OrderHistoryViewModel()
            {
                // Dùng .Cast<Order>() để chuyển đổi từ OrderViewInfo về Order an toàn
                Orders = result.DataItems.Cast<Order>().ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Theo dõi chi tiết 1 đơn hàng cụ thể
        /// </summary>
        public async Task<IActionResult> Tracking(int orderId)
        {
            int customerID = GetCustomerID();
            var order = await SalesDataService.GetOrderAsync(orderId);

            // Bảo mật: Không cho khách hàng xem đơn của người khác
            if (order == null || order.CustomerID != customerID)
                return RedirectToAction("History");

            var details = await SalesDataService.ListDetailsAsync(orderId);

            // Lấy Product info cho từng chi tiết
            var detailList = new List<OrderDetailViewInfo>();
            foreach (var d in details)
            {
                var product = await CatalogDataService.GetProductAsync(d.ProductID); 
                detailList.Add(new OrderDetailViewInfo
                {
                    ProductID = d.ProductID,
                    Quantity = d.Quantity,
                    SalePrice = d.SalePrice,
                    ProductName = product?.ProductName,
                    Photo = product?.Photo, 
                    Unit = product?.Unit
                });
            }

            var model = new OrderHistoryViewModel()
            {
                CurrentOrder = order,
                OrderDetails = detailList
            };

            return View(model);
        }
    }
}