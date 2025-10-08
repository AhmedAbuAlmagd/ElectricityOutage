using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;

namespace STA.Electricity.API.Services
{
    public class CuttingDownService : ICuttingDownService
    {
        private readonly ICuttingDownQueryRepository _repository;

        public CuttingDownService(ICuttingDownQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<CuttingDownHeaderDto>> SearchHeadersAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var (items, totalCount) = await _repository.SearchHeadersAsync(
                sourceKey,
                problemTypeKey,
                statusKey,
                searchCriteriaKey,
                networkElementTypeKey,
                searchValue,
                fromDate,
                toDate,
                page,
                pageSize);

            return new PagedResult<CuttingDownHeaderDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<CuttingDownDetailDto?> GetHeaderByIncidentIdAsync(string id)
            => _repository.GetHeaderByIncidentIdAsync(id);

        public Task<List<CuttingDownDetailDto>> GetDetailsByIncidentIdAsync(string cuttingIncidentId)
            => _repository.GetDetailsByIncidentIdAsync(cuttingIncidentId);

        public Task<List<object>> ExportHeadersAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate)
            => _repository.ExportHeadersAsync(
                sourceKey,
                problemTypeKey,
                statusKey,
                searchCriteriaKey,
                networkElementTypeKey,
                searchValue,
                fromDate,
                toDate);
    }
}