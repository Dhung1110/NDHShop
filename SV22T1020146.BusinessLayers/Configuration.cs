using SV22T1020146.BusinessLayers;

namespace SV22T1020146.BusinessLayers
{
    /// <summary>
    /// Khởi tạo và lưu trữ các thông tin cấu hình sử dụng cho BusinessLayer
    /// </summary>
    public static class  Configuration
    {
        private static string _connectionString;
        /// <summary>
        /// Khởi tạo cấu hình cho BusinessLayer (Hàm này phải được gọi trước khi chạy ứng dụng
        /// </summary>
        /// <param name="connectionString">Chuỗi tham số kết nối đến cơ sở dữ liệu</param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Chuỗi tham số kết nối đến cơ sở dữ liệu
        /// </summary>
        public static string ConnectionString => _connectionString;
        public static SecurityDataService SecurityService
        {
            get
            {
                return new SecurityDataService(_connectionString);
            }
        }
    }
}
