using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Services
{
    public class NetworkElementService : INetworkElementService
    {
        private readonly AppDbContext _context;
        public NetworkElementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NetworkElementNodeDto>> GetNetworkHierarchyAsync()
        {
            var networkElements = await _context.NetworkElements
                .OrderBy(x => x.NetworkElementName)
                .ToListAsync();

            var roots = networkElements
                .Where(x => x.ParentNetworkElementKey == null)
                .Select(x => BuildNetworkNode(x, networkElements))
                .ToList();
            return roots;
        }

        public async Task<PagedResult<NetworkElementDto>> SearchNetworkElementsAsync(string? searchTerm, int? typeKey, bool? isActive, int page, int pageSize)
        {
            var query = _context.NetworkElements.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.NetworkElementName!.Contains(searchTerm));
            }
            if (typeKey.HasValue)
            {
                query = query.Where(x => x.NetworkElementTypeKey == typeKey.Value);
            }
            // isActive not stored; ignore for now

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.NetworkElementName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NetworkElementDto
                {
                    Key = x.NetworkElementKey,
                    Name = x.NetworkElementName ?? string.Empty,
                    Code = string.Empty,
                    TypeKey = x.NetworkElementTypeKey ?? 0,
                    ParentKey = x.ParentNetworkElementKey,
                    IsActive = true
                })
                .ToListAsync();

            return new PagedResult<NetworkElementDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<NetworkElementDto?> GetNetworkElementAsync(int key)
        {
            return await _context.NetworkElements
                .Where(x => x.NetworkElementKey == key)
                .Select(x => new NetworkElementDto
                {
                    Key = x.NetworkElementKey,
                    Name = x.NetworkElementName ?? string.Empty,
                    Code = string.Empty,
                    TypeKey = x.NetworkElementTypeKey ?? 0,
                    ParentKey = x.ParentNetworkElementKey,
                    IsActive = true
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<NetworkElementDto>> GetChildrenAsync(int parentKey)
        {
            return await _context.NetworkElements
                .Where(x => x.ParentNetworkElementKey == parentKey)
                .OrderBy(x => x.NetworkElementName)
                .Select(x => new NetworkElementDto
                {
                    Key = x.NetworkElementKey,
                    Name = x.NetworkElementName ?? string.Empty,
                    Code = string.Empty,
                    TypeKey = x.NetworkElementTypeKey ?? 0,
                    ParentKey = x.ParentNetworkElementKey,
                    IsActive = true
                })
                .ToListAsync();
        }

        public async Task<PagedResult<NetworkIncidentDto>> GetIncidentsAsync(int networkElementKey, int page, int pageSize)
        {
            var query = _context.CuttingDownDetails
                .Where(x => x.NetworkElementKey == networkElementKey);

            var totalCount = await query.CountAsync();
            var incidents = await query
                .OrderByDescending(x => x.ActualCreateDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NetworkIncidentDto
                {
                    CuttingIncidentId = x.CuttingDownDetailKey.ToString(),
                    NetworkElement = string.Empty,
                    StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                    EndDate = x.ActualEndDate,
                    NumberOfImpactedCustomers = 0,
                    Status = x.ActualEndDate.HasValue ? "Closed" : "Open"
                })
                .ToListAsync();

            return new PagedResult<NetworkIncidentDto>
            {
                Items = incidents,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private NetworkElementNodeDto BuildNetworkNode(NetworkElement element, List<NetworkElement> allElements)
        {
            var node = new NetworkElementNodeDto
            {
                Id = element.NetworkElementKey,
                Name = element.NetworkElementName ?? string.Empty,
                Type = GetNetworkElementTypeName(element.NetworkElementTypeKey ?? 0),
                HasChildren = allElements.Any(x => x.ParentNetworkElementKey == element.NetworkElementKey),
                Children = new List<NetworkElementNodeDto>()
            };

            var children = allElements.Where(x => x.ParentNetworkElementKey == element.NetworkElementKey).ToList();
            foreach (var child in children)
            {
                node.Children.Add(BuildNetworkNode(child, allElements));
            }

            return node;
        }

        private string GetNetworkElementTypeName(int typeKey)
        {
            return typeKey switch
            {
                1 => "Region",
                2 => "Zone",
                3 => "Substation",
                4 => "Feeder",
                5 => "Transformer",
                _ => "Unknown"
            };
        }
    }
}