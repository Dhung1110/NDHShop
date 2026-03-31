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
    /// <summary>
    /// Các chức năng liên quan đến người giao hàng
    /// </summary>
    public class ShipperController : Controller
    {
        private const int PAGESIZE = 10;
        public const string SEARCH_SHIPPER = "SearchShipper";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_SHIPPER);
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

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListShippersAsync(input);
            ApplicationContext.SetSessionData(SEARCH_SHIPPER, input);
            return View(result);
        }

        /// <summary>
        /// Thêm mới
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm người giao hàng";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Sửa
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật người giao hàng";
            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu (Thêm + Sửa)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            ViewBag.Title = data.ShipperID == 0 ? "Thêm người giao hàng" : "Cập nhật người giao hàng";

            // ===== VALIDATION =====
            if (string.IsNullOrWhiteSpace(data.ShipperName))
                ModelState.AddModelError(nameof(data.ShipperName), "Vui lòng nhập tên người giao hàng");

            if (string.IsNullOrWhiteSpace(data.Phone))
                ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại");

            // Nếu lỗi -> quay lại form
            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }

            // ===== SAVE =====
            if (data.ShipperID == 0)
            {
                await PartnerDataService.AddShipperAsync(data);
            }
            else
            {
                await PartnerDataService.UpdateShipperAsync(data);
            }

            // reset tìm kiếm
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = data.ShipperName
            };
            ApplicationContext.SetSessionData(SEARCH_SHIPPER, input);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteShipperAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool allowDelete = !await PartnerDataService.IsUsedShipperAsync(id);
            ViewBag.AllowDelete = allowDelete;

            return View(model);
        }
    }
}