namespace SV22T1020146.DataLayers.Interfaces
{
    public interface IDataDictionaryRepository<T> where T : class
    {
        /// <summary>
        /// Lấy danh sách dữ liệu
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ListAsync();
    }
}
