using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.DataDictionary;
using SV22T1020146.Models.Partner;
using SV22T1020146.Models.Security;
using SV22T1020146.Shop.Models;
using System.Security.Claims;

namespace SV22T1020146.Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly SecurityDataService _securityService;

        public AccountController()
        {
            _securityService = new SecurityDataService(Configuration.ConnectionString);
        }
        /// <summary>
        /// Đăng kí tài khoản khách hàng mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Register()
        {
            var provinces = await DictionaryDataService.ListProvincesAsync();
            ViewBag.Provinces = provinces;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(model);
            }

            bool isValid = await PartnerDataService.ValidateCustomerEmailAsync(model.Email);
            if (!isValid)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(model);
            }

            Customer customer = new Customer()
            {
                CustomerName = model.CustomerName,
                ContactName = model.ContactName,
                Address = model.Address,
                Province = model.Province,
                Phone = model.Phone,
                Email = model.Email,
                Password = CryptHelper.HashMD5(model.Password),
                IsLocked = false
            };

            int id = await PartnerDataService.AddCustomerAsync(customer);

            if (id <= 0)
            {
                ModelState.AddModelError("", "Đăng ký thất bại");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(model);
            }

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Đăng Nhập 
        /// </summary>
        /// <returns></returns>
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string password = CryptHelper.HashMD5(model.Password);
            UserAccount? user = await _securityService.AuthorizeAsync(model.Email, password);

            if (user == null)
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleNames),
                new Claim("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Thông tin cá nhân của khách hàng đã đăng nhập
        /// </summary>
        /// <returns></returns>

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = await PartnerDataService.GetCustomerAsync(userId);
            if (customer == null) return NotFound();

            var provinces = await DictionaryDataService.ListProvincesAsync() ?? new List<Province>();
            ViewBag.Provinces = new SelectList(provinces, "ProvinceName", "ProvinceName", customer.Province);

            var model = new ProfileViewModel
            {
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Province = customer.Province
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = await PartnerDataService.GetCustomerAsync(userId);
            if (customer == null) return NotFound();

            
            bool isChanged =
                model.CustomerName != customer.CustomerName ||
                model.ContactName != customer.ContactName ||
                model.Phone != customer.Phone ||
                model.Address != customer.Address ||
                model.Province != customer.Province ||
                model.Email != customer.Email; 

            var provinces = await DictionaryDataService.ListProvincesAsync() ?? new List<Province>();
            ViewBag.Provinces = new SelectList(provinces, "ProvinceName", "ProvinceName", model.Province);

            if (!isChanged)
            {
                ViewBag.Message = "Thông tin không có thay đổi";
               
                model.Email = customer.Email;
                return View(model);
            }

            
            customer.CustomerName = model.CustomerName;
            customer.ContactName = model.ContactName;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.Province = model.Province;
            customer.Email = model.Email; 

            bool result = await PartnerDataService.UpdateCustomerAsync(customer);

            if (!result)
            {
                ModelState.AddModelError("", "Cập nhật thông tin thất bại");
                return View(model);
            }

            ViewBag.Message = "Cập nhật thông tin thành công";
            // luôn hiển thị email mới
            model.Email = customer.Email;
            return View(model);
        }
        /// <summary>
        /// Đổi mật khẩu 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Kiểm tra nhập liệu
            if (string.IsNullOrEmpty(model.OldPassword))
                ModelState.AddModelError("OldPassword", "Vui lòng nhập mật khẩu hiện tại");

            if (string.IsNullOrEmpty(model.NewPassword))
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới");

            if (string.IsNullOrEmpty(model.ConfirmPassword))
                ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu mới");

            if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.ConfirmPassword)
                && model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Xác nhận mật khẩu không khớp");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Lấy thông tin user
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = await PartnerDataService.GetCustomerAsync(userId);

            if (customer == null || CryptHelper.HashMD5(model.OldPassword) != customer.Password)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            // Thay đổi mật khẩu
            customer.Password = CryptHelper.HashMD5(model.NewPassword);
            bool result = await PartnerDataService.ChangeCustomerPasswordAsync(customer.Email, customer.Password);

            if (!result)
            {
                ModelState.AddModelError("", "Đổi mật khẩu thất bại, vui lòng thử lại");
                return View(model);
            }

            ViewBag.Message = "Đổi mật khẩu thành công";
            return View(new ChangePasswordViewModel()); // reset form
        }


    }
}