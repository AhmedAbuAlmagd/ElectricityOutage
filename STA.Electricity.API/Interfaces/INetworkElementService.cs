using STA.Electricity.API.Dtos;

namespace STA.Electricity.API.Interfaces
{
    public interface INetworkElementService
    {
        Task<List<NetworkElementNodeDto>> GetNetworkHierarchyAsync();
        Task<PagedResult<NetworkElementDto>> SearchNetworkElementsAsync(string? searchTerm, int? typeKey, bool? isActive, int page, int pageSize);
        Task<NetworkElementDto?> GetNetworkElementAsync(int key);
        Task<List<NetworkElementDto>> GetChildrenAsync(int parentKey);
        Task<PagedResult<NetworkIncidentDto>> GetIncidentsAsync(int networkElementKey, int page, int pageSize);
    }
}