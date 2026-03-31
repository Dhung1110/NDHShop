using SV22T1020146.Models.Sales;

namespace SV22T1020146.Admin
{
    /// <summary>
    /// Lớp cung cấp các hàm tiện ích/chức năng liên quan đến giỏ hàng
    /// (giỏ hàng lưu trong sesion)
    /// </summary>
    public static class ShoppingCartHelper
    {
        /// <summary>
        /// Tên biến để lưu giỏ hàng trong session
        /// </summary>
        private const string CART = "ShoppingCart";

        /// <summary>
        /// Lấy giỏ hàng từ session (nếu giỏ hàng chưa có thì tạo giỏ hàng rỗng)
        /// </summary>
        /// <returns></returns>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }

        /// <summary>
        /// Thêm hàng vào giỏ 
        /// </summary>
        /// <param name="data"></param>
        public static void AddItemToCart(OrderDetailViewInfo data)
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == data.ProductID);
            if (existItem == null)
            {
                cart.Add(data);
            }
            else
            {
                existItem.Quantity += data.Quantity;
                existItem.SalePrice = data.SalePrice;
            }

            ApplicationContext.SetSessionData(CART, cart);
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong giỏ hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        public static void UpdateItemInCart(int productID, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == productID);
            if (existItem != null)
            {
                existItem.Quantity = quantity;
                existItem.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        //Xóa một mặt hàng khỏi giỏ
        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        /// <summary>
        /// Xóa trống giỏ hàng 
        /// </summary>
        public static void ClearCart()
        {
            var cart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CART, cart);
        }
    }
}
