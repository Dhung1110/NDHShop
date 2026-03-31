using Microsoft.Data.SqlClient;
namespace SV22T1020146.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp cơ sở cho các lớp cài đăyj cho phép xử lý dữ liệu trên CSDL SQL Server
    /// </summary>
    public abstract class BaseRepository
    {
        private string _connectionString;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected SqlConnection GetConnection() 
        {
            return new SqlConnection(_connectionString);
        }
    }
}
