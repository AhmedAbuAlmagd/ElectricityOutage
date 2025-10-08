using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Repositories
{
    public class CuttingDownQueryRepository : ICuttingDownQueryRepository
    {
        private readonly AppDbContext _context;
        public CuttingDownQueryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<CuttingDownHeaderDto> Items, int TotalCount)> SearchHeadersAsync(
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
            var query = _context.CuttingDownHeaders
                .Include(x => x.CuttingDownDetails).ThenInclude(y => y.NetworkElementKeyNavigation)
                .AsQueryable();

            if (sourceKey.HasValue)
            {
                if (sourceKey.Value == 1) query = query.Where(x => x.ChannelKey == 1);
                else if (sourceKey.Value == 2) query = query.Where(x => x.ChannelKey == 2);
            }
            if (problemTypeKey.HasValue)
            {
                query = query.Where(x => x.CuttingDownProblemTypeKey == problemTypeKey.Value);
            }
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.ActualCreateDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(x => x.ActualCreateDate <= toDate.Value);
            }
            if (networkElementTypeKey.HasValue)
            {
                var cuttingdownKeys = _context.CuttingDownDetails
                    .Where(d => d.NetworkElementKey == networkElementTypeKey.Value)
                    .Select(d => d.CuttingDownKey);
                query = query.Where(x => cuttingdownKeys.Contains(x.CuttingDownKey));
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.ActualCreateDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CuttingDownHeaderDto
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString()!,
                    NetworkElementName = "Netowrk Element",
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                    EndDate = x.ActualEndDate,
                    ProblemTypeKey = x.CuttingDownProblemTypeKey ?? 0,
                    Source = x.ChannelKey == 1 ? "Cabin" : x.ChannelKey == 2 ? "Cable" : "System",
                    Status = x.ActualEndDate.HasValue ? "Closed" : "Open"
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<CuttingDownDetailDto?> GetHeaderByIncidentIdAsync(string id)
        {
            var details = await _context.CuttingDownHeaders
                .Where(x => x.CuttingDownIncidentId.ToString() == id)
                .Select(x => new CuttingDownDetailDto
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString()!,
                    NetworkElementName = "Network Element",
                    NetworkElementKey = 0,
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate,
                    EndDate = x.ActualEndDate,
                    ProblemTypeKey = x.CuttingDownProblemTypeKey ?? 0,
                    Source = "System",
                    Status = x.ActualEndDate.HasValue ? "Closed" : "Open",
                    Description = "Cutting down incident",
                    CreatedBy = "System",
                    CreatedDate = x.ActualCreateDate,
                    UpdatedBy = "System",
                    UpdatedDate = x.SynchUpdateDate
                })
                .FirstOrDefaultAsync();
            return details;
        }

        public async Task<List<CuttingDownDetailDto>> GetDetailsByIncidentIdAsync(string cuttingIncidentId)
        {
            var details = await _context.CuttingDownDetails
                .Where(x => x.CuttingDownDetailKey.ToString() == cuttingIncidentId)
                .Select(x => new CuttingDownDetailDto
                {
                    CuttingIncidentId = x.CuttingDownDetailKey.ToString()!,
                    NetworkElementKey = x.NetworkElementKey ?? 0,
                    NetworkElementName = "Network Element",
                    NumberOfImpactedCustomers = x.ImpactedCustomers ?? 0,
                    StartDate = x.ActualCreateDate,
                    EndDate = x.ActualEndDate,
                    ProblemTypeKey = 0,
                    Source = "System",
                    Status = x.ActualEndDate.HasValue ? "Closed" : "Open",
                    Description = "Cutting down detail",
                    CreatedBy = "System",
                    CreatedDate = x.ActualCreateDate,
                    UpdatedBy = "System",
                    UpdatedDate = x.ActualEndDate
                })
                .ToListAsync();
            return details;
        }

        public async Task<List<object>> ExportHeadersAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.CuttingDownHeaders
                .Include(x => x.ChannelKeyNavigation)
                .AsQueryable();

            if (sourceKey.HasValue)
            {
                if (sourceKey.Value == 1)
                {
                    query = query.Where(x => x.ChannelKeyNavigation != null && x.ChannelKeyNavigation.ChannelName == "A");
                }
                else if (sourceKey.Value == 2)
                {
                    query = query.Where(x => x.ChannelKeyNavigation != null && x.ChannelKeyNavigation.ChannelName == "B");
                }
            }
            if (problemTypeKey.HasValue)
            {
                query = query.Where(x => x.CuttingDownProblemTypeKey == problemTypeKey.Value);
            }
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.ActualCreateDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(x => x.ActualCreateDate <= toDate.Value);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue));
            }

            var data = await query
                .OrderByDescending(x => x.ActualCreateDate)
                .Select(x => new
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
                    NetworkElementName = "Network Element",
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate,
                    EndDate = x.ActualEndDate,
                    ProblemTypeKey = x.CuttingDownProblemTypeKey ?? 0,
                    Source = x.ChannelKeyNavigation != null
                        ? (x.ChannelKeyNavigation.ChannelName == "A" ? "Cabin"
                            : x.ChannelKeyNavigation.ChannelName == "B" ? "Cable" : "System")
                        : "System",
                    Status = x.ActualEndDate.HasValue ? "Closed" : "Open"
                })
                .ToListAsync();

            return data.Cast<object>().ToList();
        }
    }
}