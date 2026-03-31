using Microsoft.AspNetCore.Mvc;
using SV22T1020146.Admin.Models;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Partner;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020146.Admin.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private const int PAGESIZE = 10;
        public const string SEARCH_SUPPLIER = "SearchSupplier";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_SUPPLIER);
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
            var result = await PartnerDataService.ListSuppliersAsync(input);
            ApplicationContext.SetSessionData(SEARCH_SUPPLIER, input);
            return View(result);
        }
        [Authorize(Roles = "admin,datamanager")]
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhà cung cấp";
            var model = new Supplier()
            {
                SupplierID = 0
            };
            return View("Edit", model);
        }
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data)
        {
            ViewBag.Title = data.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";

            // ===== VALIDATION =====
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Vui lòng nhập tên nhà cung cấp");

            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email");

            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành");

            // Chuẩn hóa dữ liệu
            if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = "";
            if (string.IsNullOrWhiteSpace(data.Address)) data.Address = "";
            if (string.IsNullOrWhiteSpace(data.Phone)) data.Phone = "";

            // Nếu có lỗi → trả về View
            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }

            // ===== SAVE =====
            if (data.SupplierID == 0)
            {
                await PartnerDataService.AddSupplierAsync(data);
            }
            else
            {
                await PartnerDataService.UpdateSupplierAsync(data);
            }

            PaginationSearchInput input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = data.SupplierName
            };
            ApplicationContext.SetSessionData(SEARCH_SUPPLIER, input);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteSupplierAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool allowDelete = !await PartnerDataService.IsUsedSupplierAsync(id);
            ViewBag.AllowDelete = allowDelete;

            return View(model);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}