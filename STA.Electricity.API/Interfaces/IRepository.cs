namespace STA.Electricity.API.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        // Query operations (used in sync)
        Task<IEnumerable<T>> GetOpenIncidentsAsync(int channelKey);
        Task<int> GetMaxKeyAsync();

        // Command operations (used in sync)
        Task<T> AddAsync(T entity);
        Task<int> UpdateClosedIncidentsAsync(int channelKey);
    }
}
