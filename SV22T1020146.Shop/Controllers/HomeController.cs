using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Catalog;
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

        public async Task<IActionResult> Index()
        {
            var input = new ProductSearchInput()
            {
                Page = 1,
                PageSize = 1000, 
                SearchValue = ""
            };

            var result = await CatalogDataService.ListProductsAsync(input);

            
            var randomProducts = result.DataItems
                                       .OrderBy(x => Guid.NewGuid())
                                       .Take(8)
                                       .ToList();

            return View(randomProducts); 
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
