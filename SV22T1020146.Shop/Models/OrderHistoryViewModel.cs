using SV22T1020146.Models;
using SV22T1020146.Models.Sales;

namespace SV22T1020146.Shop.Models
{
    public class OrderHistoryViewModel
    {
        // Danh sách các đơn hàng đã mua
        public List<Order> Orders { get; set; } = new List<Order>();

        // Hoặc thông tin chi tiết để theo dõi trạng thái (Chức năng 9)
        public Order? CurrentOrder { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}