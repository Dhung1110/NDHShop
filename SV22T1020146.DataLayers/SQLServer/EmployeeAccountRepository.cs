using Dapper;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.DataLayers.SQLServer;
using SV22T1020146.Models.Security;


namespace SV22T1020146.DataLayers.SQLServer
{
    public class EmployeeAccountRepository : BaseRepository, IUserAccountRepository
    {
        public EmployeeAccountRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using var connection = GetConnection();

            string sql = @"SELECT 
                   EmployeeID AS UserId,
                   Email AS UserName,
                   FullName AS DisplayName,
                   Email,
                   Photo,
                   RoleNames,
                   IsWorking   
               FROM Employees
               WHERE Email=@userName
                     AND Password=@password";


            var user = await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql,
                new { userName, password });

            if (user != null)
            {
                
                if (string.IsNullOrEmpty(user.RoleNames))
                    user.RoleNames = "employee";

               
                if (user.RoleNames.Contains(","))
                    user.RoleNames = user.RoleNames.Split(',')[0];
            }

            return user;
        }

        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = GetConnection();

            string sql = @"UPDATE Employees
                           SET Password=@password
                           WHERE Email=@userName";

            return await connection.ExecuteAsync(sql,
                new { userName, password }) > 0;
        }
        public async Task<bool> ChangeRolesAsync(int employeeId, List<string> roles)
        {
            using var connection = GetConnection();

            // Chuyển list role thành string phân tách bằng dấu phẩy
            string rolesStr = string.Join(",", roles);

            string sql = @"UPDATE Employees
                   SET RoleNames=@roles
                   WHERE EmployeeID=@id";

            return await connection.ExecuteAsync(sql, new { roles = rolesStr, id = employeeId }) > 0;
        }
    }
}