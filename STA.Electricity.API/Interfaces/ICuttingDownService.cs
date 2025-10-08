using STA.Electricity.API.Dtos;

namespace STA.Electricity.API.Interfaces
{
    public interface ICuttingDownService
    {
        Task<PagedResult<CuttingDownHeaderDto>> SearchHeadersAsync(
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

        Task<CuttingDownDetailDto?> GetHeaderByIncidentIdAsync(string id);

        Task<List<CuttingDownDetailDto>> GetDetailsByIncidentIdAsync(string cuttingIncidentId);

        Task<List<object>> ExportHeadersAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate);
    }
}