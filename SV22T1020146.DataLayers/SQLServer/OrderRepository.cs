using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.Common;
using SV22T1020146.Models.Sales;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(string connectionString) : base(connectionString)
        {
        }

        // ================= ORDER =================

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            var result = new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string where = "WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(input.SearchValue))
                    where += " AND c.CustomerName LIKE @SearchValue";

                if (input.Status != 0)
                    where += " AND o.Status = @Status";

                if (input.DateFrom.HasValue)
                    where += " AND o.OrderTime >= @DateFrom";

                if (input.DateTo.HasValue)
                    where += " AND o.OrderTime <= @DateTo";

                // 👉 COUNT
                string countSql = $@"
        SELECT COUNT(*)
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
        {where}";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(input.SearchValue))
                        cmd.Parameters.AddWithValue("@SearchValue", $"%{input.SearchValue}%");

                    if (input.Status != 0)
                        cmd.Parameters.AddWithValue("@Status", (int)input.Status);

                    if (input.DateFrom.HasValue)
                        cmd.Parameters.AddWithValue("@DateFrom", input.DateFrom.Value);

                    if (input.DateTo.HasValue)
                        cmd.Parameters.AddWithValue("@DateTo", input.DateTo.Value);

                    result.RowCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 👉 DATA
                string sql = $@"
        SELECT 
            o.OrderID,
            o.OrderTime,
            o.AcceptTime,
            o.Status,

            c.CustomerName,
            c.Phone AS CustomerPhone,

            e.FullName AS EmployeeName

        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
        LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID

        {where}

        ORDER BY o.OrderID DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Offset", (input.Page - 1) * input.PageSize);
                    cmd.Parameters.AddWithValue("@PageSize", input.PageSize);

                    if (!string.IsNullOrWhiteSpace(input.SearchValue))
                        cmd.Parameters.AddWithValue("@SearchValue", $"%{input.SearchValue}%");

                    if (input.Status != 0)
                        cmd.Parameters.AddWithValue("@Status", (int)input.Status);

                    if (input.DateFrom.HasValue)
                        cmd.Parameters.AddWithValue("@DateFrom", input.DateFrom.Value);

                    if (input.DateTo.HasValue)
                        cmd.Parameters.AddWithValue("@DateTo", input.DateTo.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.DataItems.Add(new OrderViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                                AcceptTime = reader["AcceptTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["AcceptTime"]),
                                Status = (OrderStatusEnum)Convert.ToInt32(reader["Status"]),

                                CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                CustomerPhone = Convert.ToString(reader["CustomerPhone"]) ?? "",
                                EmployeeName = Convert.ToString(reader["EmployeeName"]) ?? ""
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            OrderViewInfo? result = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
SELECT 
    o.OrderID,
    o.CustomerID,
    o.EmployeeID,
    o.ShipperID,
    o.OrderTime,
    o.AcceptTime,
    o.FinishedTime,
    o.Status,
    o.DeliveryAddress,
    o.DeliveryProvince,

    c.CustomerName,
    c.Address AS CustomerAddress,
    c.Province AS CustomerProvince,

    e.FullName AS EmployeeName,
    s.ShipperName,
    s.Phone AS ShipperPhone

FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.OrderID = @OrderID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new OrderViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CustomerID"]),
                                EmployeeID = reader["EmployeeID"] == DBNull.Value ? null : Convert.ToInt32(reader["EmployeeID"]),
                                ShipperID = reader["ShipperID"] == DBNull.Value ? null : Convert.ToInt32(reader["ShipperID"]),

                                OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                                AcceptTime = reader["AcceptTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["AcceptTime"]),
                                FinishedTime = reader["FinishedTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["FinishedTime"]),
                                Status = (OrderStatusEnum)Convert.ToInt32(reader["Status"]),

                                DeliveryAddress = Convert.ToString(reader["DeliveryAddress"]) ?? "",
                                DeliveryProvince = Convert.ToString(reader["DeliveryProvince"]) ?? "",

                                CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                CustomerAddress = Convert.ToString(reader["CustomerAddress"]) ?? "",
                                CustomerProvince = Convert.ToString(reader["CustomerProvince"]) ?? "",

                                EmployeeName = Convert.ToString(reader["EmployeeName"]) ?? "",
                                ShipperName = Convert.ToString(reader["ShipperName"]) ?? "",
                                ShipperPhone = Convert.ToString(reader["ShipperPhone"]) ?? ""
                            };
                        }
                    }
                }
            }

            return result;
        }

        public async Task<int> AddAsync(Order data)
        {
            int id;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
            INSERT INTO Orders(CustomerID, EmployeeID, OrderTime, Status, DeliveryProvince, DeliveryAddress)
            VALUES(@CustomerID, @EmployeeID, @OrderTime, @Status, @DeliveryProvince, @DeliveryAddress);
            SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", (object?)data.CustomerID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeID", (object?)data.EmployeeID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrderTime", data.OrderTime);
                    cmd.Parameters.AddWithValue("@Status", (int)data.Status);
                    cmd.Parameters.AddWithValue("@DeliveryProvince", data.DeliveryProvince ?? "");
                    cmd.Parameters.AddWithValue("@DeliveryAddress", data.DeliveryAddress ?? "");

                    id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }

            return id;
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
UPDATE Orders
SET EmployeeID=@EmployeeID,
    ShipperID=@ShipperID,
    ShippedTime=@ShippedTime,
    AcceptTime=@AcceptTime,
    FinishedTime=@FinishedTime,
    Status=@Status
WHERE OrderID=@OrderID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", data.OrderID);
                    cmd.Parameters.AddWithValue("@EmployeeID", (object?)data.EmployeeID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShipperID", (object?)data.ShipperID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShippedTime", (object?)data.ShippedTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AcceptTime", (object?)data.AcceptTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FinishedTime", (object?)data.FinishedTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (int)data.Status);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM Orders WHERE OrderID=@OrderID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        // ================= ORDER DETAIL =================

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            var list = new List<OrderDetailViewInfo>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                SELECT d.*, p.ProductName
                FROM OrderDetails d
                JOIN Products p ON d.ProductID = p.ProductID
                WHERE d.OrderID=@OrderID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new OrderDetailViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = Convert.ToString(reader["ProductName"]) ?? "",
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                SalePrice = Convert.ToDecimal(reader["SalePrice"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            OrderDetailViewInfo? result = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                SELECT d.*, p.ProductName
                FROM OrderDetails d
                JOIN Products p ON d.ProductID = p.ProductID
                WHERE d.OrderID=@OrderID AND d.ProductID=@ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    cmd.Parameters.AddWithValue("@ProductID", productID);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new OrderDetailViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = Convert.ToString(reader["ProductName"]) ?? "",
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                SalePrice = Convert.ToDecimal(reader["SalePrice"])
                            };
                        }
                    }
                }
            }

            return result;
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                INSERT INTO OrderDetails(OrderID,ProductID,Quantity,SalePrice)
                VALUES(@OrderID,@ProductID,@Quantity,@SalePrice)";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", data.OrderID);
                    cmd.Parameters.AddWithValue("@ProductID", data.ProductID);
                    cmd.Parameters.AddWithValue("@Quantity", data.Quantity);
                    cmd.Parameters.AddWithValue("@SalePrice", data.SalePrice);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                UPDATE OrderDetails
                SET Quantity=@Quantity,
                    SalePrice=@SalePrice
                WHERE OrderID=@OrderID AND ProductID=@ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", data.OrderID);
                    cmd.Parameters.AddWithValue("@ProductID", data.ProductID);
                    cmd.Parameters.AddWithValue("@Quantity", data.Quantity);
                    cmd.Parameters.AddWithValue("@SalePrice", data.SalePrice);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM OrderDetails WHERE OrderID=@OrderID AND ProductID=@ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    cmd.Parameters.AddWithValue("@ProductID", productID);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}