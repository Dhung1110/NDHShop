using SV22T1020146.Models.Common;
namespace SV22T1020146.Admin.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginationSearchViewModel<T> where T : class
    {
        /// <summary>
        /// Dữ liệu phân trang
        /// </summary>
        public required PaginationSearchInput Input { get; set; }
        /// <summary>
        /// Chuỗi tìm kiếm (nếu có)
        /// </summary>
        public required  PagedResult<T> Result  { get; set; } 
    }
}