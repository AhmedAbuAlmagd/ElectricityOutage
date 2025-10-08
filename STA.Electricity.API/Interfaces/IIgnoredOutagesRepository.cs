using STA.Electricity.API.Dtos;

namespace STA.Electricity.API.Interfaces
{
    public interface IIgnoredOutagesRepository
    {
        Task<(IEnumerable<IgnoredOutageDto> Items, int TotalCount)> SearchAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);

        Task<IgnoredOutageDto?> GetByIncidentIdAsync(string id);

        Task<List<object>> ExportAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate);

        Task IgnoreAsync(string cuttingIncidentId, string ignoredBy, string reason);

        Task<bool> UnignoreAsync(string cuttingIncidentId);
    }
}