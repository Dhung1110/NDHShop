namespace SV22T1020146.Shop.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Thuộc tính tính toán tổng tiền của 1 dòng hàng
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}