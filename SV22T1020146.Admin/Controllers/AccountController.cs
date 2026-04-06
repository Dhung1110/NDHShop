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
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <returns></returns>
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
            password = CryptHelper.HashMD5(password);

            var userAccount = await Configuration.SecurityService.AuthorizeAsync(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Sai tài khoản hoặc mật khẩu");
                return View();
            }

            // 🚫 check nghỉ việc
            if (!userAccount.IsWorking)
            {
                ModelState.AddModelError("Error", "Tài khoản đã nghỉ việc, không thể đăng nhập");
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

        /// <summary>
        /// Đổi mật khẩu 
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Đăng xuất 
        /// </summary>
        /// <returns></returns>

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