using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Repositories
{
    public class IgnoredOutagesRepository : IIgnoredOutagesRepository
    {
        private readonly AppDbContext _context;
        public IgnoredOutagesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<IgnoredOutageDto> Items, int TotalCount)> SearchAsync(
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
            var query = _context.CuttingDownIgnoreds.AsQueryable();

            if (fromDate.HasValue) query = query.Where(x => x.ActualCreateDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.ActualCreateDate <= toDate.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue) ||
                                         (x.CabelName ?? "").Contains(searchValue) ||
                                         (x.CabinName ?? "").Contains(searchValue));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.ActualCreateDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IgnoredOutageDto
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString()!,
                    NetworkElementName = x.CabelName ?? x.CabinName ?? "",
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                    EndDate = null,
                    ProblemTypeKey = 0,
                    Source = "System",
                    Status = "Ignored",
                    IgnoredDate = x.SynchCreateDate,
                    IgnoredBy = x.CreatedUser,
                    IgnoreReason = "Automatically ignored"
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IgnoredOutageDto?> GetByIncidentIdAsync(string id)
        {
            var outage = await _context.CuttingDownIgnoreds
                .Where(x => x.CuttingDownIncidentId.ToString() == id)
                .Select(x => new IgnoredOutageDto
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString()!,
                    NetworkElementName = x.CabelName ?? x.CabinName ?? "",
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                    EndDate = null,
                    ProblemTypeKey = 0,
                    Source = "System",
                    Status = "Ignored",
                    IgnoredDate = x.SynchCreateDate,
                    IgnoredBy = x.CreatedUser,
                    IgnoreReason = "Automatically ignored"
                })
                .FirstOrDefaultAsync();
            return outage;
        }

        public async Task<List<object>> ExportAsync(
            int? sourceKey,
            int? problemTypeKey,
            int? statusKey,
            int? searchCriteriaKey,
            int? networkElementTypeKey,
            string? searchValue,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.CuttingDownIgnoreds.AsQueryable();

            if (fromDate.HasValue) query = query.Where(x => x.ActualCreateDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.ActualCreateDate <= toDate.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue) ||
                                         (x.CabelName ?? "").Contains(searchValue) ||
                                         (x.CabinName ?? "").Contains(searchValue));
            }

            var data = await query
                .OrderByDescending(x => x.ActualCreateDate)
                .Select(x => new
                {
                    CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
                    NetworkElementName = x.CabelName ?? x.CabinName ?? "",
                    NumberOfImpactedCustomers = 0,
                    StartDate = x.ActualCreateDate,
                    EndDate = (DateTime?)null,
                    ProblemTypeKey = 0,
                    Source = "System",
                    Status = "Ignored",
                    IgnoredDate = x.SynchCreateDate,
                    IgnoredBy = x.CreatedUser,
                    IgnoreReason = "Automatically ignored"
                })
                .ToListAsync();

            return data.Cast<object>().ToList();
        }

        public async Task IgnoreAsync(string cuttingIncidentId, string ignoredBy, string reason)
        {
            var ignoredOutage = new CuttingDownIgnored
            {
                CuttingDownIncidentId = int.Parse(cuttingIncidentId),
                ActualCreateDate = DateTime.UtcNow,
                SynchCreateDate = DateTime.UtcNow,
                CreatedUser = ignoredBy,
                CabelName = reason
            };

            _context.CuttingDownIgnoreds.Add(ignoredOutage);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UnignoreAsync(string cuttingIncidentId)
        {
            var ignoredOutage = await _context.CuttingDownIgnoreds
                .FirstOrDefaultAsync(x => x.CuttingDownIncidentId.ToString() == cuttingIncidentId);

            if (ignoredOutage == null) return false;

            _context.CuttingDownIgnoreds.Remove(ignoredOutage);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}