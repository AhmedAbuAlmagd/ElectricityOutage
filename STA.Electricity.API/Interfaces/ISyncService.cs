namespace STA.Electricity.API.Interfaces
{
    /// <summary>
    /// Service interface for synchronization operations between STA and FTA
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Synchronize incidents from STA to FTA for a specific source
        /// </summary>
        /// <param name="source">Source system (A for cabins, B for cables)</param>
        /// <returns>Synchronization result</returns>
        Task<SyncResult> SynchronizeAsync(string source);
    }

    /// <summary>
    /// Result of synchronization operation
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int ChannelKey { get; set; }
        public int CreatedIncidents { get; set; }
        public int ClosedIncidents { get; set; }
        public int TotalProcessed { get; set; }
        public string? Error { get; set; }
    }
}
