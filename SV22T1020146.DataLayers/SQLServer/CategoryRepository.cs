using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class CategoryRepository : BaseRepository, IGenericRepository<Category>
    {
        public CategoryRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Category data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"INSERT INTO Categories(CategoryName, Description)
                           VALUES(@CategoryName,@Description);
                           SELECT SCOPE_IDENTITY();";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CategoryName", data.CategoryName);
            cmd.Parameters.AddWithValue("@Description", (object?)data.Description ?? DBNull.Value);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "DELETE FROM Categories WHERE CategoryID=@CategoryID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CategoryID", id);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<Category?> GetAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT * FROM Categories WHERE CategoryID=@CategoryID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CategoryID", id);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Category()
                {
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    CategoryName = reader["CategoryName"].ToString() ?? "",
                    Description = reader["Description"].ToString() ?? ""
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"UPDATE Categories
                           SET CategoryName=@CategoryName,
                               Description=@Description
                           WHERE CategoryID=@CategoryID";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@CategoryName", data.CategoryName);
            cmd.Parameters.AddWithValue("@Description", (object?)data.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryID", data.CategoryID);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT COUNT(*) FROM Products WHERE CategoryID=@CategoryID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CategoryID", id);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using var connection = GetConnection();
            await connection.OpenAsync();

            string countSql = @"SELECT COUNT(*) FROM Categories
                                WHERE CategoryName LIKE @Search";

            SqlCommand countCmd = new SqlCommand(countSql, connection);
            countCmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");

            result.RowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            string sql = @"SELECT *
                           FROM Categories
                           WHERE CategoryName LIKE @Search
                           ORDER BY CategoryName
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");
            cmd.Parameters.AddWithValue("@Offset", (input.Page - 1) * input.PageSize);
            cmd.Parameters.AddWithValue("@PageSize", input.PageSize);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<Category> data = new List<Category>();

            while (await reader.ReadAsync())
            {
                data.Add(new Category()
                {
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    CategoryName = reader["CategoryName"].ToString() ?? "",
                    Description = reader["Description"].ToString() ?? ""
                });
            }

            result.DataItems = data;
            return result;
        }
    }
}