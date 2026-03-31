## Trong file layout:
- @RenderBody() : Đặt tại vị trí mà nội dung của các trang web sẽ được "ghi" vào đó
- @{
     await Html.RenderPartialAsync("PartialView");
   }
   hoặc:
   @await Html.PartialAsync("PartialView")
   Dùng để lấy nội dung của một PartialView (phần code HTML được tách ra ở 1 file view) và "ghi/chèn" vào một vị trí nào đó.
- @await RenderSectionAsync("SectionName", required: false)
  


## Phác thảo chức năng cho SV22T1020146.Admin

- Trang chủ: Home/Index
- Tài khoản:
    - Account/Login
    - Account/Logout
    - Account/ChangPassword
- Supplier :
    - Supplier/Index
    - Supplier/Create
    - Supplier/Edit/{id}
	- Supperlier/Delete/{id}
- Customer :
    - Customer/Index
	- Customer/Create
	- Customer/Edit/{id}
	- Customer/Delete/{id}
	- Customer/ChangePassword/{id}
- Shipper :
    - Shipper/Index
    - Shipper/Create
    - Shipper/Edit/{id}
	- Shipper/Delete/{id}
_ Employee :
	- Employee/Index
	- Empoyee/Create
    - Employee/edit/{id}
	- Employee/Delete/{id}
	- Employee/ChangePassword/{id}
	- Employee/ChangeRoles/{id}
_ Cagegory :
	- Category/Index
	- Category/Create
	- Category/Edit/{id}
	- Category/Delete/{id}
- Product :
	- Product/Index
	    - Tìm kiếm, lọc mặt hàng theo nhà cung cấp, phân loại, khoảng giá, tên
	    - Hiển thị danh sách dưới dạng phân trang
	- Product/Create
	- Product/Edit/{id}
	- Product/Delete/{id}
	- Product/Detail/{id}
	- Product/ListAtrributes/{id}
	- Product/AddAtrribute/{id}
	- Product/EditAttribute/{id}?attributeId={attributeId}
	- Product/DeleteAttribute/{id}?attributeId={attributeId}
	- Product/ListPhotos/{id}
	- Product/AddPhoto/{id}
	- Product/DeletePhoto/{id}?photoId={photoId}
	- Product/DeletePhoto/{id}?photoId={photoId}
- Order :
	- Order/Index
    - Order/Search
    - Order/Create
    - Order/Detail/{id}
	- Order/EditCartItem/{id}?productId={productId}
	- Order/DeleteCartItem/{id}?productId={productId}
	- Order/ClearCart
	- Order/Accept/{id}
	- Order/Shipping/{id}
	- Order/Finish/{id}
	- Order/Reject/{id}
	- Order/Cancel/{id}
	- Order/Delete/{id}
  

## Models chia theo Domain:
	- Data dictionary: Province
	- Partner: Supplier, Customer, Shipper
	- HR(Human Resource): Employee
	- Catalog: Category, Product, ProductAttribute, ProductPhoto
	- Sales: Order, OrderStatus, OrderDetail
	- Security: UserAccount
	- Common: ...


## Thiết kế interfaces:
    - Tìm kiếm phân trang: Đầu vào tím kiếm, phân trang: Page, PageSize, SearchValue (nhà cc, khách hàng, shipper, category, employee)
	- Lấy thông tin 1 đối tượng theo id: GetById(id)
	- Bổ sung 1 doois tượng vào CSDL
	- Cập nhật thông tin 1 đối tượng trong CSDL


## Admin:
    - Cài package NewtonSoft.Json
	- Tạo lớp ApplicationContext trong thư mục AppCodes
	- Cập nhật code của Program.cs theo mẫu

## 3 nguyên tắc khi Action trả về dữ liệu cho View là model:
    - Khi Action có trả dữ liệu về cho View thì phải biết kiểu dữ liệu là gì
    - Trong View (trên cùng của View), phải có chỉ thị khái báo dữ kiểu dữ liệu mà Action trả về  @model Kiểu__Dữ__Liệu
    - Trong View, dữ liệu mà Action trả về lưu trong thuộc tính có tên là Model (trong view thống qua thuộc tính này để lấy dữ liệu)

## Security
	- Người dùng cung cấp thông tin để kiểm tra xem có được phép vào hệ thống hay không?
	- Hệ thống kiểm tra, Nếu hợp lệ thì cấp cho một Cookie(giấy chứng nhận)
	- Phía client xuất trình Cookie mỗi khi thực hiện các Request (kèm cookie trong header của lời gọi)
	- Phía server dựa vào cookie để kiểm tra

	2 thuật ngữ:
	- Authentication: Xác thực, quá trình xác định xem người dùng có phải là ai đó đã đăng ký trong hệ thống hay không?
    - Authorization: Phân quyền, quá trình xác định xem người dùng đã xác thực có được phép truy cập vào tài nguyên nào đó hay không?