using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace STA.Electricity.API.Controllers
{
    /// <summary>
    /// Ignored Outages Management Controller for managing excluded outage incidents
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Manage ignored outage incidents and exclusion rules")]
    public class IgnoredOutagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IgnoredOutagesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Search ignored outages with filtering
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
        /// <returns>Paginated list of ignored outages</returns>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Search ignored outages",
            Description = "Search and filter ignored outage incidents with pagination"
        )]
        public async Task<ActionResult<PagedResult<IgnoredOutageDto>>> SearchIgnoredOutages(
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
                var query = _context.CuttingDownIgnoreds.AsQueryable();

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
                    query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue) ||
                                           x.CabelName.Contains(searchValue) ||
                                           x.CabinName.Contains(searchValue));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.ActualCreateDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new IgnoredOutageDto
                    {
                        CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
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

                return Ok(new PagedResult<IgnoredOutageDto>
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
                return StatusCode(500, new { message = "An error occurred while searching ignored outages", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ignored outage by ID
        /// </summary>
        /// <param name="id">Cutting incident ID</param>
        /// <returns>Ignored outage details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get ignored outage by ID",
            Description = "Retrieve detailed information about a specific ignored outage"
        )]
        public async Task<ActionResult<IgnoredOutageDto>> GetIgnoredOutage(string id)
        {
            try
            {
                var outage = await _context.CuttingDownIgnoreds
                    .Where(x => x.CuttingDownIncidentId.ToString() == id)
                    .Select(x => new IgnoredOutageDto
                    {
                        CuttingIncidentId = x.CuttingDownIncidentId.ToString(),
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

                if (outage == null)
                {
                    return NotFound(new { message = "Ignored outage not found" });
                }

                return Ok(outage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving ignored outage", error = ex.Message });
            }
        }

        /// <summary>
        /// Add outage to ignored list
        /// </summary>
        /// <param name="request">Ignore outage request</param>
        /// <returns>Success response</returns>
        [HttpPost("ignore")]
        [SwaggerOperation(
            Summary = "Ignore an outage",
            Description = "Add an outage incident to the ignored list with reason"
        )]
        public async Task<ActionResult> IgnoreOutage([FromBody] IgnoreOutageRequest request)
        {
            try
            {
                var ignoredOutage = new CuttingDownIgnored
                {
                    CuttingDownIncidentId = int.Parse(request.CuttingIncidentId),
                    ActualCreateDate = DateTime.UtcNow,
                    SynchCreateDate = DateTime.UtcNow,
                    CreatedUser = request.IgnoredBy,
                    CabelName = request.Reason
                };

                _context.CuttingDownIgnoreds.Add(ignoredOutage);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Outage successfully ignored" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while ignoring outage", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove outage from ignored list
        /// </summary>
        /// <param name="id">Cutting incident ID</param>
        /// <returns>Success response</returns>
        [HttpPost("{id}/unignore")]
        [SwaggerOperation(
            Summary = "Unignore an outage",
            Description = "Remove an outage incident from the ignored list"
        )]
        public async Task<ActionResult> UnignoreOutage(string id)
        {
            try
            {
                var ignoredOutage = await _context.CuttingDownIgnoreds
                    .FirstOrDefaultAsync(x => x.CuttingDownIncidentId.ToString() == id);

                if (ignoredOutage == null)
                {
                    return NotFound(new { message = "Ignored outage not found" });
                }

                _context.CuttingDownIgnoreds.Remove(ignoredOutage);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Outage successfully unignored" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while unignoring outage", error = ex.Message });
            }
        }

        /// <summary>
        /// Export ignored outages data to Excel
        /// </summary>
        /// <param name="sourceKey">Source filter key</param>
        /// <param name="problemTypeKey">Problem type filter key</param>
        /// <param name="statusKey">Status filter key</param>
        /// <param name="searchCriteriaKey">Search criteria filter key</param>
        /// <param name="networkElementTypeKey">Network element type filter key</param>
        /// <param name="searchValue">Search value</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <returns>Excel file with ignored outages data</returns>
        [HttpGet("export")]
        [SwaggerOperation(
            Summary = "Export ignored outages data",
            Description = "Export filtered ignored outages to Excel format"
        )]
        public async Task<ActionResult> ExportIgnoredOutages(
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
                var query = _context.CuttingDownIgnoreds.AsQueryable();

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
                    query = query.Where(x => x.CuttingDownIncidentId.ToString().Contains(searchValue) ||
                                           x.CabelName.Contains(searchValue) ||
                                           x.CabinName.Contains(searchValue));
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
    public class IgnoredOutageDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? IgnoredDate { get; set; }
        public string? IgnoredBy { get; set; }
        public string? IgnoreReason { get; set; }
    }

    public class IgnoreOutageRequest
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string IgnoredBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}