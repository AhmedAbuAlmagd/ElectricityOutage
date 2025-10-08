using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace STA.Electricity.API.Controllers
{
    /// <summary>
    /// Cutting Down Management Controller for electricity outage incidents
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Manage electricity cutting down incidents and outages")]
    public class CuttingDownController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CuttingDownController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Search cutting down headers with advanced filtering
        /// </summary>
        /// <param name="sourceKey">Source filter key</param>
        /// <param name="problemTypeKey">Problem type filter key</param>
        /// <param name="statusKey">Status filter key</param>
        /// <param name="searchCriteriaKey">Search criteria filter key</param>
        /// <param name="networkElementTypeKey">Network element type filter key</param>
        /// <param name="searchValue">Search value</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated list of cutting down headers</returns>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Search cutting down incidents",
            Description = "Search and filter electricity cutting down incidents with pagination"
        )]
        public async Task<ActionResult<PagedResult<CuttingDownHeaderDto>>> SearchCuttingDownHeaders(
            [FromQuery] int? sourceKey,
            [FromQuery] int? problemTypeKey,
            [FromQuery] int? statusKey,
            [FromQuery] int? searchCriteriaKey,
            [FromQuery] int? networkElementTypeKey,
            [FromQuery] string? searchValue,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.CuttingDownHeaders
                    .Include(x => x.CuttingDownDetails).ThenInclude(y => y.NetworkElementKeyNavigation)
                    .AsQueryable();

                if (sourceKey.HasValue)
                {
                    if (sourceKey.Value == 1)
                    {
                        query = query.Where(x => x.ChannelKey == 1);
                    }
                    else if (sourceKey.Value == 2)
                    {
                        query = query.Where(x=> x.ChannelKey == 2);
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

                        CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
                        NetworkElementName = "Netowrk Element",
                        NumberOfImpactedCustomers = 0,
                        StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                        EndDate = x.ActualEndDate,
                        ProblemTypeKey = x.CuttingDownProblemTypeKey ?? 0,
                        Source = x.ChannelKey == 1 ? "Cabin" : x.ChannelKey == 2 ? "Cable" : "System",
                        Status = x.ActualEndDate.HasValue ? "Closed" : "Open"
                    })
                    .ToListAsync();

                return Ok(new PagedResult<CuttingDownHeaderDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching cutting down headers", error = ex.Message });
            }
        }

        /// <summary>
        /// Get cutting down header by ID
        /// </summary>
        /// <param name="id">Cutting incident ID</param>
        /// <returns>Cutting down header details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get cutting down header by ID",
            Description = "Retrieve detailed information about a specific cutting down incident"
        )]
        public async Task<ActionResult<CuttingDownHeaderDto>> GetCuttingDownHeader(string id)
        {
            try
            {
                var details = await _context.CuttingDownHeaders
                    .Where(x => x.CuttingDownIncidentId.ToString() == id)
                    .Select(x => new CuttingDownDetailDto
                    {
                        CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
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

                if (details == null)
                {
                    return NotFound(new { message = "Cutting down incident not found" });
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cutting down header", error = ex.Message });
            }
        }

        /// <summary>
        /// Get cutting down details for a specific header
        /// </summary>
        /// <param name="cuttingIncidentId">Cutting incident ID</param>
        /// <returns>List of cutting down details</returns>
        [HttpGet("{cuttingIncidentId}/details")]
        [SwaggerOperation(
            Summary = "Get cutting down details",
            Description = "Retrieve detailed breakdown of a cutting down incident"
        )]
        public async Task<ActionResult<List<CuttingDownDetailDto>>> GetCuttingDownDetails(string cuttingIncidentId)
        {
            try
            {
                var details = await _context.CuttingDownDetails
                    .Where(x => x.CuttingDownDetailKey.ToString() == cuttingIncidentId)
                    .Select(x => new CuttingDownDetailDto
                    {
                        CuttingIncidentId = x.CuttingDownDetailKey.ToString(),
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

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cutting down details", error = ex.Message });
            }
        }

        /// <summary>
        /// Export cutting down data to Excel
        /// </summary>
        /// <param name="sourceKey">Source filter key</param>
        /// <param name="problemTypeKey">Problem type filter key</param>
        /// <param name="statusKey">Status filter key</param>
        /// <param name="searchCriteriaKey">Search criteria filter key</param>
        /// <param name="networkElementTypeKey">Network element type filter key</param>
        /// <param name="searchValue">Search value</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <returns>Excel file with cutting down data</returns>
        [HttpGet("export")]
        [SwaggerOperation(
            Summary = "Export cutting down data",
            Description = "Export filtered cutting down incidents to Excel format"
        )]
        public async Task<ActionResult> ExportCuttingDownData(
            [FromQuery] int? sourceKey,
            [FromQuery] int? problemTypeKey,
            [FromQuery] int? statusKey,
            [FromQuery] int? searchCriteriaKey,
            [FromQuery] int? networkElementTypeKey,
            [FromQuery] string? searchValue,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.CuttingDownHeaders
                    .Include(x => x.ChannelKeyNavigation)
                    .AsQueryable();

                // Apply same filters as search
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

                // For now, return JSON data. In a real implementation, you would generate Excel file
                return Ok(new { message = "Export functionality - would generate Excel file", data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting data", error = ex.Message });
            }
        }
    }

    // DTOs
    public class CuttingDownHeaderDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CuttingDownDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public int NetworkElementKey { get; set; }
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CuttingDownDetailDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public int NetworkElementKey { get; set; }
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}