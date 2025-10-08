using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Dtos;
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
        private readonly ICuttingDownService _service;

        public CuttingDownController(ICuttingDownService service)
        {
            _service = service;
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
                var result = await _service.SearchHeadersAsync(
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
                var details = await _service.GetHeaderByIncidentIdAsync(id);
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
                var details = await _service.GetDetailsByIncidentIdAsync(cuttingIncidentId);
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
                var data = await _service.ExportHeadersAsync(
                    sourceKey,
                    problemTypeKey,
                    statusKey,
                    searchCriteriaKey,
                    networkElementTypeKey,
                    searchValue,
                    fromDate,
                    toDate);

                return Ok(new { message = "Export functionality - would generate Excel file", data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting data", error = ex.Message });
            }
        }
    }

    
}