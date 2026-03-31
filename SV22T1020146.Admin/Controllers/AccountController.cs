using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Security;

namespace SV22T1020146.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đủ thông tin");
                return View();
            }

            // ❗ Nếu DB không hash thì comment dòng này
            password = CryptHelper.HashMD5(password);

            var userAccount = await Configuration.SecurityService.AuthorizeAsync(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Sai tài khoản hoặc mật khẩu");
                return View();
            }

            var webUserData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                webUserData.CreatePrincipal()
            );

            return RedirectToAction("Index", "Home");
        }

        // ================= CHANGE PASSWORD =================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không đúng");
                return View();
            }

            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            // ❗ nếu dùng MD5 thì bật lại
             newPassword = CryptHelper.HashMD5(newPassword);

            bool result = await Configuration.SecurityService
                                .ChangePasswordAsync(user.UserName, newPassword);

            if (!result)
            {
                ModelState.AddModelError("", "Đổi mật khẩu thất bại");
                return View();
            }

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

        // ================= LOGOUT =================

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}