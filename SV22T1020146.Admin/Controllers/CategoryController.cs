using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020146.Admin.Models;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;

namespace SV22T1020146.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        public const int PAGESIZE = 10;
        public const string SEARCH_CATEGORY = "SearchCategory";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_CATEGORY);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả phân trang
        /// </summary>
        /// <param name="intput"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData(SEARCH_CATEGORY, input);
            return View(result);
        }

        /// <summary>
        /// /// Tạo loại hàng mới
        /// /// </summary>
        /// /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin loại hàng";
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu loại hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            ViewBag.Title = data.CategoryID == 0 ? "Thêm loại hàng" : "Cập nhật loại hàng";

            // Validate
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Vui lòng nhập tên loại hàng");

            if (!ModelState.IsValid)
                return View("Edit", data);

            bool result;
            if (data.CategoryID == 0)
            {
                var id = await CatalogDataService.AddCategoryAsync(data);
                result = id > 0;

                if (!result)
                    ModelState.AddModelError("", "Thêm loại hàng thất bại");
            }
            else
            {
                result = await CatalogDataService.UpdateCategoryAsync(data);

                if (!result)
                    ModelState.AddModelError("", "Cập nhật loại hàng thất bại");
            }

            if (!result)
                return View("Edit", data);

            // Lưu search
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = data.CategoryName
            };
            ApplicationContext.SetSessionData(SEARCH_CATEGORY, input);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa loại hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool allowDelete = !await CatalogDataService.IsUsedCategoryAsync(id);
            ViewBag.AllowDelete = allowDelete;

            if (Request.Method == "POST")
            {
                if (!allowDelete)
                    return RedirectToAction("Delete", new { id });

                await CatalogDataService.DeleteCategoryAsync(id);
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}