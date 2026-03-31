using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.HR;


namespace SV22T1020146.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhân viên mới
        /// Nếu id <> 0: Kiểm tra email của nhân viên có mã là id
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int employeeID);

        Task<bool> ChangePasswordAsync(string email, string password);
        Task<bool> VerifyPasswordAsync(string email, string password);
        Task<bool> ChangeRolesAsync(int employeeId, List<string> roles);
    }
}
