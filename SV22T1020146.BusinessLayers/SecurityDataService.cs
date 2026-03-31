using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.DataLayers.SQLServer;
using SV22T1020146.Models.Security;

namespace SV22T1020146.BusinessLayers
{
    public class SecurityDataService
    {
        private readonly IUserAccountRepository _employeeDB;
        private readonly IUserAccountRepository _customerDB;

        public SecurityDataService(string connectionString)
        {
            _employeeDB = new EmployeeAccountRepository(connectionString);
            _customerDB = new CustomerAccountRepository(connectionString);
        }

        public async Task<UserAccount?> AuthorizeAsync(string username, string password)
        {
            
            var user = await _employeeDB.Authorize(username, password);
            if (user != null)
                return user;

           
            user = await _customerDB.Authorize(username, password);
            if (user != null)
                return user;

            return null;
        }

        public async Task<bool> ChangePasswordAsync(string username, string password)
        {
            var r1 = await _employeeDB.ChangePasswordAsync(username, password);
            var r2 = await _customerDB.ChangePasswordAsync(username, password);
            return r1 || r2;
        }
    }
}