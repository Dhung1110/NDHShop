using Microsoft.AspNetCore.Mvc;
using SV22T1020146.Shop.Models;
using System.Diagnostics;

namespace SV22T1020146.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Tạo một danh sách sản phẩm giả để hiển thị thử
            var products = new List<string> { "iPhone 15", "Samsung S24", "MacBook M3", "iPad Pro" };

            // Gửi danh sách này sang View
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
