using System.ComponentModel.DataAnnotations;

namespace SV22T1020146.Shop.Models
{
    public class ChangePasswordViewModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}