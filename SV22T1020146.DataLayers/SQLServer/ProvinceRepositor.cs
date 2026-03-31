using Microsoft.Data.SqlClient;
using SV22T1020146.DataLayers.Interfaces;
using SV22T1020146.Models.DataDictionary;

namespace SV22T1020146.DataLayers.SQLServer
{
    public class ProvinceRepository : BaseRepository, IDataDictionaryRepository<Province>
    {
        public ProvinceRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Province>> ListAsync()
        {
            var list = new List<Province>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "SELECT ProvinceName FROM Provinces ORDER BY ProvinceName";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Province()
                            {
                                ProvinceName = Convert.ToString(reader["ProvinceName"])!
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}