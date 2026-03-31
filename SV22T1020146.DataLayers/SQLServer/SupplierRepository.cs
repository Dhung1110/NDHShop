using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Partner;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class SupplierRepository : BaseRepository, IGenericRepository<Supplier>
    {
        public SupplierRepository(string connectionString) : base(connectionString)
        {
        }

        // Thêm nhà cung cấp
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"INSERT INTO Suppliers(SupplierName, ContactName, Phone, Email, Address, Province)
                           VALUES(@SupplierName,@ContactName,@Phone,@Email,@Address,@Province);
                           SELECT SCOPE_IDENTITY();";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@SupplierName", data.SupplierName);
            cmd.Parameters.AddWithValue("@ContactName", data.ContactName);
            cmd.Parameters.AddWithValue("@Phone", data.Phone);
            cmd.Parameters.AddWithValue("@Email", data.Email ?? "");
            cmd.Parameters.AddWithValue("@Address", data.Address);
            cmd.Parameters.AddWithValue("@Province", data.Province);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Xóa nhà cung cấp
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "DELETE FROM Suppliers WHERE SupplierID=@SupplierID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SupplierID", id);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        // Lấy chi tiết nhà cung cấp
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT * FROM Suppliers WHERE SupplierID=@SupplierID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SupplierID", id);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Supplier()
                {
                    SupplierID = Convert.ToInt32(reader["SupplierID"]),
                    SupplierName = reader["SupplierName"].ToString() ?? "",
                    ContactName = reader["ContactName"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",       // <-- đã thêm
                    Address = reader["Address"].ToString() ?? "",
                    Province = reader["Province"].ToString() ?? ""
                };
            }

            return null;
        }

        // Cập nhật nhà cung cấp
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"UPDATE Suppliers
                           SET SupplierName=@SupplierName,
                               ContactName=@ContactName,
                               Phone=@Phone,
                               Email=@Email,             -- <-- thêm Email
                               Address=@Address,
                               Province=@Province
                           WHERE SupplierID=@SupplierID";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@SupplierName", data.SupplierName);
            cmd.Parameters.AddWithValue("@ContactName", data.ContactName);
            cmd.Parameters.AddWithValue("@Phone", data.Phone);
            cmd.Parameters.AddWithValue("@Email", data.Email ?? "");     // <-- thêm Email
            cmd.Parameters.AddWithValue("@Address", data.Address);
            cmd.Parameters.AddWithValue("@Province", data.Province);
            cmd.Parameters.AddWithValue("@SupplierID", data.SupplierID);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        // Kiểm tra nhà cung cấp đang được sử dụng trong sản phẩm hay không
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT COUNT(*) FROM Products WHERE SupplierID=@SupplierID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SupplierID", id);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        // Lấy danh sách nhà cung cấp theo phân trang
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using var connection = GetConnection();
            await connection.OpenAsync();

            // Count tổng số bản ghi
            string countSql = @"SELECT COUNT(*) FROM Suppliers
                                WHERE SupplierName LIKE @Search";

            SqlCommand countCmd = new SqlCommand(countSql, connection);
            countCmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");

            result.RowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            // Lấy dữ liệu theo phân trang
            string sql = @"SELECT *
                           FROM Suppliers
                           WHERE SupplierName LIKE @Search
                           ORDER BY SupplierName
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");
            cmd.Parameters.AddWithValue("@Offset", (input.Page - 1) * input.PageSize);
            cmd.Parameters.AddWithValue("@PageSize", input.PageSize);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<Supplier> data = new List<Supplier>();

            while (await reader.ReadAsync())
            {
                data.Add(new Supplier()
                {
                    SupplierID = Convert.ToInt32(reader["SupplierID"]),
                    SupplierName = reader["SupplierName"].ToString() ?? "",
                    ContactName = reader["ContactName"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",       // <-- thêm Email
                    Address = reader["Address"].ToString() ?? "",
                    Province = reader["Province"].ToString() ?? ""
                });
            }

            result.DataItems = data;
            return result;
        }
    }
}