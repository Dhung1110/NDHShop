using Microsoft.AspNetCore.Mvc;
using SV22T1020146.Admin;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.HR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020146.Admin.Controllers
{
    /// <summary>
    /// Quản lý nhân viên
    /// </summary>
    [Authorize]
    public class EmployeeController : Controller
    {
        public const int PAGESIZE = 10;
        public const string SEARCH_EMPLOYEE = "SearchEmployee";
        /// <summary>
        /// Tìm kiếm, hiển thị danh sách loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_EMPLOYEE);
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
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            //Tìm kiếm
            var result = await HRDataService.ListEmployeesAsync(input);

            //Lưu lại điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(SEARCH_EMPLOYEE, input);

            //Trả về kết quả cho view
            return View(result);
        }
        /// <summary>
        /// Tạo mới nhân viên
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                // Kiểm tra dữ liệu đầu vào: FullName và Email
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                // 1️⃣ Xử lý xóa ảnh nếu tick checkbox "DeletePhoto"
                if (Request.Form["DeletePhoto"] == "true")
                {
                    if (!string.IsNullOrEmpty(data.Photo) && data.Photo != "nophoto.png")
                    {
                        var oldFilePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", data.Photo);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }
                    data.Photo = "nophoto.png";
                }

                // 2️⃣ Xử lý upload ảnh mới
                if (uploadPhoto != null)
                {
                    // Xóa ảnh cũ nếu khác "nophoto.png"
                    if (!string.IsNullOrEmpty(data.Photo) && data.Photo != "nophoto.png")
                    {
                        var oldFilePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", data.Photo);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    // Lưu ảnh mới
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                // Tiền xử lý dữ liệu trước khi lưu
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                // Lưu dữ liệu
                if (data.EmployeeID == 0)
                {
                    await HRDataService.AddEmployeeAsync(data);
                    PaginationSearchInput input = new PaginationSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = data.FullName
                    };
                    ApplicationContext.SetSessionData(SEARCH_EMPLOYEE, input);
                }
                else
                {
                    await HRDataService.UpdateEmployeeAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }
        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }

            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool allowDelete = !(await HRDataService.IsUsedEmployeeAsync(id));
            ViewBag.AllowDelete = allowDelete;

            return View(model);
        }
        /// <summary>
        /// Đổi mật khẩu nhân viên 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string password, string confirmPassword)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "Vui lòng nhập mật khẩu mới");

            if (password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
                return View(employee); 

            
            await HRDataService.ChangeEmployeePasswordAsync(employee.Email, CryptHelper.HashMD5(password));

            
            ViewBag.SuccessMessage = "Đã đổi mật khẩu thành công";

            
            return View(employee);
        }

        /// <summary>
        /// phân quyền nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeRole(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            var currentRoles = (employee.RoleNames ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();            
            ViewBag.AllRoles = new List<string> { WebUserRoles.Administrator, WebUserRoles.DataManager, WebUserRoles.Sales };
            ViewBag.CurrentRoles = currentRoles;

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, List<string> roles)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            await HRDataService.ChangeEmployeeRolesAsync(id, roles);

            ViewBag.SuccessMessage = "Đã cập nhật role thành công";

            ViewBag.AllRoles = new List<string> { WebUserRoles.Administrator, WebUserRoles.DataManager, WebUserRoles.Sales };
            ViewBag.CurrentRoles = roles;

            return View(employee);
        }
    }
}