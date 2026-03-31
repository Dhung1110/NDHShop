using Microsoft.AspNetCore.Mvc;
using SV22T1020146.BusinessLayers;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.DataLayers.SQLServer;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;

namespace SV22T1020146.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 12;

        /// <summary>
        /// Load danh mục
        /// </summary>
        private async Task LoadCategories()
        {
            IGenericRepository<Category> categoryDB =
                new CategoryRepository(Configuration.ConnectionString);

            var result = await categoryDB.ListAsync(new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = ""
            });

            ViewBag.Categories = result.DataItems;
        }

        /// <summary>
        /// Trang chính
        /// </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        /// <summary>
        /// Tìm kiếm + phân trang
        /// </summary>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            await LoadCategories();

            input.PageSize = PAGE_SIZE;

            
            if (input.MinPrice < 0) input.MinPrice = 0;
            if (input.MaxPrice < 0) input.MaxPrice = 0;

            
            if (input.MinPrice > input.MaxPrice && input.MaxPrice > 0)
            {
                var temp = input.MinPrice;
                input.MinPrice = input.MaxPrice;
                input.MaxPrice = temp;
            }

            var result = await CatalogDataService.ListProductsAsync(input);

            return View(result);
        }

        /// <summary>
        /// Chi tiết sản phẩm
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            // Lấy thông tin sản phẩm
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            // Lấy danh sách ảnh
            var photos = await CatalogDataService.ListPhotosAsync(id);

            // Lấy thuộc tính
            var attributes = await CatalogDataService.ListAttributesAsync(id);

            ViewBag.Photos = photos;
            ViewBag.Attributes = attributes;

            return View(product);
        }
    }
}