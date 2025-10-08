using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Services
{
    public class LookupService : ILookupService
    {
        private readonly AppDbContext _context;
        public LookupService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LookupItemDto>> GetSourcesAsync()
        {
            // Sources come from FTA.Channel
            return await _context.Channels
                .OrderBy(c => c.ChannelKey)
                .Select(c => new LookupItemDto { Key = c.ChannelKey, Name = c.ChannelName ?? string.Empty })
                .ToListAsync();
        }

        public async Task<List<LookupItemDto>> GetProblemTypesAsync()
        {
            return await _context.ProblemTypes
                .OrderBy(p => p.ProblemTypeKey)
                .Select(p => new LookupItemDto { Key = p.ProblemTypeKey, Name = p.ProblemTypeName ?? string.Empty })
                .ToListAsync();
        }

        public async Task<List<LookupItemDto>> GetStatusesAsync()
        {
            // No status table found; derive common statuses used across UI
            // Consider moving to configuration or dedicated table later
            var statuses = new List<LookupItemDto>
            {
                new LookupItemDto { Key = 1, Name = "Open" },
                new LookupItemDto { Key = 2, Name = "Closed" }
            };
            return await Task.FromResult(statuses);
        }

        public async Task<List<LookupItemDto>> GetSearchCriteriaAsync()
        {
            // No search criteria table found; keep as static until model exists
            var criteria = new List<LookupItemDto>
            {
                new LookupItemDto { Key = 1, Name = "Incident ID" },
                new LookupItemDto { Key = 2, Name = "Network Element" },
                new LookupItemDto { Key = 3, Name = "Customer Count" }
            };
            return await Task.FromResult(criteria);
        }

        public async Task<List<LookupItemDto>> GetNetworkElementTypesAsync()
        {
            return await _context.NetworkElementTypes
                .OrderBy(n => n.NetworkElementTypeKey)
                .Select(n => new LookupItemDto { Key = n.NetworkElementTypeKey, Name = n.NetworkElementTypeName ?? string.Empty })
                .ToListAsync();
        }
    }
}