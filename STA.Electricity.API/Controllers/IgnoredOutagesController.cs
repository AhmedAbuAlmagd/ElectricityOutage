using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;
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
        private readonly IIgnoredOutagesService _service;

        public IgnoredOutagesController(IIgnoredOutagesService service)
        {
            _service = service;
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
                var result = await _service.SearchAsync(
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

                result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize);
                return Ok(result);
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
                var outage = await _service.GetByIncidentIdAsync(id);

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
                await _service.IgnoreAsync(request.CuttingIncidentId, request.IgnoredBy, request.Reason);

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
                var success = await _service.UnignoreAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "Ignored outage not found" });
                }

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
                var data = await _service.ExportAsync(
                    sourceKey,
                    problemTypeKey,
                    statusKey,
                    searchCriteriaKey,
                    networkElementTypeKey,
                    searchValue,
                    fromDate,
                    toDate);

                // For now, return JSON data. In a real implementation, you would generate Excel file
                return Ok(new { message = "Export functionality - would generate Excel file", data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting data", error = ex.Message });
            }
        }
    }

    
}