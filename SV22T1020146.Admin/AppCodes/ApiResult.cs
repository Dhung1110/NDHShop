namespace SV22T1020146.Admin
{
    /// <summary>
    /// Trả kết quả về cho lời gọi API
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ApiResult(int code, string message) 
        {
            Code = code;
            Message = message;
        }
        /// <summary>
        /// Trả kết quả (qui ước 1 là thành công, 0 là lỗi)
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Thông báo lỗi 
        /// </summary>
        public string Message { get; set; } = "";
    }
}
