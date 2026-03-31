using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.Catalog;
using SV22T1020146.Models.Common;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(string connectionString) : base(connectionString) { }

        // ================= PRODUCT =================

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var sqlWhere = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                sqlWhere.Add("ProductName LIKE @SearchValue");
                parameters.Add("SearchValue", $"%{input.SearchValue}%");
            }

            if (input.CategoryID > 0)
            {
                sqlWhere.Add("CategoryID=@CategoryID");
                parameters.Add("CategoryID", input.CategoryID);
            }

            if (input.SupplierID > 0)
            {
                sqlWhere.Add("SupplierID=@SupplierID");
                parameters.Add("SupplierID", input.SupplierID);
            }

            if (input.MinPrice > 0)
            {
                sqlWhere.Add("Price >= @MinPrice");
                parameters.Add("MinPrice", input.MinPrice);
            }

            if (input.MaxPrice > 0)
            {
                sqlWhere.Add("Price <= @MaxPrice");
                parameters.Add("MaxPrice", input.MaxPrice);
            }

            string whereClause = sqlWhere.Count > 0 ? "WHERE " + string.Join(" AND ", sqlWhere) : "";

            // COUNT
            var countSql = $"SELECT COUNT(*) FROM Products {whereClause}";
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // LIST
            string listSql = $@"
                SELECT * FROM Products
                {whereClause}
                ORDER BY ProductName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (input.Page - 1) * input.PageSize);
            parameters.Add("PageSize", input.PageSize);

            var dataItems = (await connection.QueryAsync<Product>(listSql, parameters)).ToList();

            return new PagedResult<Product>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = dataItems
            };
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM Products WHERE ProductID=@ProductID";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var connection = GetConnection();
            string sql = @"
                INSERT INTO Products(ProductName,SupplierID,CategoryID,Unit,Price,Photo)
                VALUES(@ProductName,@SupplierID,@CategoryID,@Unit,@Price,@Photo);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = GetConnection();

            string sql = @"
        UPDATE Products
        SET ProductName=@ProductName,
            SupplierID=@SupplierID,
            CategoryID=@CategoryID,
            Unit=@Unit,
            Price=@Price,
            Photo=@Photo,
            IsSelling=@IsSelling
        WHERE ProductID=@ProductID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Products WHERE ProductID=@ProductID";
            return await connection.ExecuteAsync(sql, new { productID }) > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = GetConnection();
            string sql = "SELECT COUNT(*) FROM OrderDetails WHERE ProductID=@ProductID";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });
            return count > 0;
        }

        // ================= ATTRIBUTE =================

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductAttributes WHERE ProductID=@productID";
            var data = await connection.QueryAsync<ProductAttribute>(sql, new { productID });
            return data.ToList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductAttributes WHERE AttributeID=@attributeID";
            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();
            string sql = @"
                INSERT INTO ProductAttributes(ProductID,AttributeName,AttributeValue,DisplayOrder)
                VALUES(@ProductID,@AttributeName,@AttributeValue,@DisplayOrder);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();
            string sql = @"
                UPDATE ProductAttributes
                SET AttributeName=@AttributeName,
                    AttributeValue=@AttributeValue,
                    DisplayOrder=@DisplayOrder
                WHERE AttributeID=@AttributeID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM ProductAttributes WHERE AttributeID=@attributeID";
            return await connection.ExecuteAsync(sql, new { attributeID }) > 0;
        }

        // ================= PHOTO =================

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductPhotos WHERE ProductID=@productID";
            var data = await connection.QueryAsync<ProductPhoto>(sql, new { productID });
            return data.ToList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductPhotos WHERE PhotoID=@photoID";
            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = GetConnection();
            string sql = @"
                INSERT INTO ProductPhotos(ProductID,Photo,Description,DisplayOrder,IsHidden)
                VALUES(@ProductID,@Photo,@Description,@DisplayOrder,@IsHidden);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = GetConnection();
            string sql = @"
                UPDATE ProductPhotos
                SET Photo=@Photo,
                    Description=@Description,
                    DisplayOrder=@DisplayOrder,
                    IsHidden=@IsHidden
                WHERE PhotoID=@PhotoID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM ProductPhotos WHERE PhotoID=@photoID";
            return await connection.ExecuteAsync(sql, new { photoID }) > 0;
        }
    }
}