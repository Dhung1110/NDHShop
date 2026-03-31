using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020146.Admin.Models;
using SV22T1020146.BusinessLayers;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.DataLayers.SQLServer;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Partner;

namespace SV22T1020146.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 10;
        public const string SEARCH_PRODUCT = "SearchProduct";

        /// <summary>
        /// Hàm bổ trợ nạp danh sách Loại hàng và Nhà cung cấp vào ViewBag
        /// </summary>
        private async Task LoadDataToViewBag()
        {
            var categoryDB = new CategoryRepository(Configuration.ConnectionString);
            var categories = await categoryDB.ListAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = ""
            });
            ViewBag.Categories = categories.DataItems;

            var supplierDB = new SupplierRepository(Configuration.ConnectionString);
            var suppliers = await supplierDB.ListAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = ""
            });
            ViewBag.Suppliers = suppliers.DataItems;
        }

        #region Index + Search

        /// <summary>
        /// Hiển thị danh sách mặt hàng theo phân trang
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SEARCH_PRODUCT);
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }

            // Lấy danh sách Category
            IGenericRepository<Category> categoryDB =
                new CategoryRepository(Configuration.ConnectionString);

            var categoryInput = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = ""
            };

            var categories = await categoryDB.ListAsync(categoryInput);
            ViewBag.Categories = categories.DataItems;

            // Lấy danh sách Supplier
            IGenericRepository<Supplier> supplierDB =
                new SupplierRepository(Configuration.ConnectionString);

            var supplierInput = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = ""
            };

            var suppliers = await supplierDB.ListAsync(supplierInput);
            ViewBag.Suppliers = suppliers.DataItems;

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm + lọc + phân trang
        /// </summary>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            input.PageSize = PAGE_SIZE;

            var result = await CatalogDataService.ListProductsAsync(input);

            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);

            return View(result);
        }
        #endregion

        #region CRUD Product

        /// <summary>
        /// Hiển thị form tạo mới mặt hàng
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            await LoadDataToViewBag();
            return View("Edit", new Product { ProductID = 0 });
        }

        /// <summary>
        /// Hiển thị form cập nhật mặt hàng
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null) return RedirectToAction("Index");

            ViewBag.Title = "Cập nhật mặt hàng";
            await LoadDataToViewBag();

            // QUAN TRỌNG: Lấy danh sách ảnh và thuộc tính để truyền vào Partial View trong Edit.cshtml
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu (Thêm / Sửa)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            // Validate dữ liệu
            if (string.IsNullOrWhiteSpace(data.ProductName))
                ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");
            if (data.CategoryID == 0)
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
            if (data.SupplierID == 0)
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
            if (data.Price <= 0)
                ModelState.AddModelError(nameof(data.Price), "Giá không hợp lệ");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                await LoadDataToViewBag();
                // Nạp lại ảnh/thuộc tính để tránh lỗi khi render lại trang Edit
                ViewBag.Photos = await CatalogDataService.ListPhotosAsync(data.ProductID);
                ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(data.ProductID);
                return View("Edit", data);
            }

            // Upload ảnh
            if (uploadPhoto != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            if (data.Photo == "nophoto.png" && data.ProductID > 0)
            {
                var oldFilePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", data.Photo);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }


            bool result;

            if (data.ProductID == 0)
            {
                var newId = await CatalogDataService.AddProductAsync(data);
                result = newId > 0;
            }
            else
            {
                result = await CatalogDataService.UpdateProductAsync(data);
            }

            if (!result)
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ hoặc thao tác thất bại");

                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                await LoadDataToViewBag();
                ViewBag.Photos = await CatalogDataService.ListPhotosAsync(data.ProductID);
                ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(data.ProductID);

                return View("Edit", data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        [Authorize(Roles = "admin,datamanager")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            // Kiểm tra xem mặt hàng có dữ liệu liên quan không (đơn hàng, v.v...)
            ViewBag.IsUsed = await CatalogDataService.IsUsedProductAsync(id);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            await CatalogDataService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }


        #endregion

        #region Product Attribute

        [Authorize(Roles = "admin,datamanager")]
        public IActionResult CreateAttribute(int id)
        {
            var model = new ProductAttribute() { ProductID = id, AttributeID = 0 };
            return View("EditAttribute", model);
        }

        public async Task<IActionResult> EditAttribute(int id, long attributeId)
        {
            var model = await CatalogDataService.GetAttributeAsync(attributeId);
            if (model == null) return RedirectToAction("Edit", new { id = id });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Vui lòng nhập tên thuộc tính");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Vui lòng nhập giá trị");

            if (!ModelState.IsValid)
                return View("EditAttribute", data);

            if (data.AttributeID == 0)
                await CatalogDataService.AddAttributeAsync(data);
            else
                await CatalogDataService.UpdateAttributeAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        public async Task<IActionResult> DeleteAttribute(int id, long attributeId)
        {
            await CatalogDataService.DeleteAttributeAsync(attributeId);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Product Photo

        [Authorize(Roles = "admin,datamanager")]
        public IActionResult CreatePhoto(int id)
        {
            var model = new ProductPhoto() { ProductID = id, PhotoID = 0, IsHidden = false };
            return View("EditPhoto", model);
        }

        public async Task<IActionResult> EditPhoto(int id, long photoId)
        {
            var model = await CatalogDataService.GetPhotoAsync(photoId);
            if (model == null) return RedirectToAction("Edit", new { id = id });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            if (string.IsNullOrWhiteSpace(data.Description)) data.Description = "";

            // Xử lý upload ảnh
            if (uploadPhoto != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                var path = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            if (string.IsNullOrEmpty(data.Photo))
                ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn ảnh");

            if (!ModelState.IsValid) return View("EditPhoto", data);

            if (data.PhotoID == 0)
                await CatalogDataService.AddPhotoAsync(data);
            else
                await CatalogDataService.UpdatePhotoAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        public async Task<IActionResult> DeletePhoto(int id, long photoId)
        {
            await CatalogDataService.DeletePhotoAsync(photoId);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion
    }
}