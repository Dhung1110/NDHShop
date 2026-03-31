using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Sales;

namespace SV22T1020146.Shop.Controllers
{
    public class CartController : Controller
    {
        /// <summary>
        /// Xem giỏ hàng
        /// </summary>
        public IActionResult Index()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return View(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ
        /// </summary>
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            ShoppingCartHelper.AddItemToCart(new OrderDetailViewInfo()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Quantity = quantity,
                SalePrice = product.Price,
                Photo = product.Photo
            });

            return Ok(); 
        }

        /// <summary>
        /// Xóa 1 sản phẩm
        /// </summary>
        public IActionResult RemoveFromCart(int productId)
        {
            ShoppingCartHelper.RemoveItemFromCart(productId);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa toàn bộ
        /// </summary>
        public IActionResult ClearCart()
        {
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật số lượng
        /// </summary>
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                ShoppingCartHelper.RemoveItemFromCart(productId);
            }
            else
            {
                var item = ShoppingCartHelper.GetShoppingCart()
                    .FirstOrDefault(x => x.ProductID == productId);

                if (item != null)
                {
                    ShoppingCartHelper.UpdateItemInCart(productId, quantity, item.SalePrice);
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult GetCount()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return Content(cart?.Count.ToString() ?? "0");
        }
    }
}