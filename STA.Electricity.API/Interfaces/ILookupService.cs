using STA.Electricity.API.Dtos;
namespace STA.Electricity.API.Interfaces
{
    public interface ILookupService
    {
        Task<List<LookupItemDto>> GetSourcesAsync();
        Task<List<LookupItemDto>> GetProblemTypesAsync();
        Task<List<LookupItemDto>> GetStatusesAsync();
        Task<List<LookupItemDto>> GetSearchCriteriaAsync();
        Task<List<LookupItemDto>> GetNetworkElementTypesAsync();
    }
}