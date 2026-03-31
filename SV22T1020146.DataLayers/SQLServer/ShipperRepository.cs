using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Partner;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class ShipperRepository : BaseRepository, IGenericRepository<Shipper>
    {
        public ShipperRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"INSERT INTO Shippers(ShipperName, Phone)
                           VALUES(@ShipperName,@Phone);
                           SELECT SCOPE_IDENTITY();";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@ShipperName", data.ShipperName);
            cmd.Parameters.AddWithValue("@Phone", data.Phone);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "DELETE FROM Shippers WHERE ShipperID=@ShipperID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ShipperID", id);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT * FROM Shippers WHERE ShipperID=@ShipperID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ShipperID", id);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Shipper()
                {
                    ShipperID = Convert.ToInt32(reader["ShipperID"]),
                    ShipperName = reader["ShipperName"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? ""
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"UPDATE Shippers
                           SET ShipperName=@ShipperName,
                               Phone=@Phone
                           WHERE ShipperID=@ShipperID";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@ShipperName", data.ShipperName);
            cmd.Parameters.AddWithValue("@Phone", data.Phone);
            cmd.Parameters.AddWithValue("@ShipperID", data.ShipperID);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = "SELECT COUNT(*) FROM Orders WHERE ShipperID=@ShipperID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ShipperID", id);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using var connection = GetConnection();
            await connection.OpenAsync();

            string countSql = @"SELECT COUNT(*) FROM Shippers
                                WHERE ShipperName LIKE @Search";

            SqlCommand countCmd = new SqlCommand(countSql, connection);
            countCmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");

            result.RowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            string sql = @"SELECT *
                           FROM Shippers
                           WHERE ShipperName LIKE @Search
                           ORDER BY ShipperName
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Search", $"%{input.SearchValue}%");
            cmd.Parameters.AddWithValue("@Offset", (input.Page - 1) * input.PageSize);
            cmd.Parameters.AddWithValue("@PageSize", input.PageSize);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<Shipper> data = new List<Shipper>();

            while (await reader.ReadAsync())
            {
                data.Add(new Shipper()
                {
                    ShipperID = Convert.ToInt32(reader["ShipperID"]),
                    ShipperName = reader["ShipperName"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? ""
                });
            }

            result.DataItems = data;
            return result;
        }
    }
}