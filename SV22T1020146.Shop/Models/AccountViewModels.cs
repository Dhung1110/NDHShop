using System.ComponentModel.DataAnnotations;

namespace SV22T1020146.Shop.Models
{
    
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }

  
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string Password { get; set; } = "";

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";

        public string ContactName { get; set; } = "";
        public string Address { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng chọn tỉnh")]
        public string Province { get; set; }
        public string Phone { get; set; } = "";
    }
}