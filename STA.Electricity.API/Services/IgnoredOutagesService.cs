using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;

namespace STA.Electricity.API.Services
{
    public class IgnoredOutagesService : IIgnoredOutagesService
    {
        private readonly IIgnoredOutagesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public IgnoredOutagesService(IIgnoredOutagesRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<IgnoredOutageDto>> SearchAsync(
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
            var (items, totalCount) = await _repository.SearchAsync(
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

            return new PagedResult<IgnoredOutageDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<IgnoredOutageDto?> GetByIncidentIdAsync(string id)
            => _repository.GetByIncidentIdAsync(id);

        public Task<List<object>> ExportAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate)
            => _repository.ExportAsync(
                sourceKey,
                problemTypeKey,
                statusKey,
                searchCriteriaKey,
                networkElementTypeKey,
                searchValue,
                fromDate,
                toDate);

        public async Task IgnoreAsync(string cuttingIncidentId, string ignoredBy, string reason)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _repository.IgnoreAsync(cuttingIncidentId, ignoredBy, reason);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UnignoreAsync(string cuttingIncidentId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _repository.UnignoreAsync(cuttingIncidentId);
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}