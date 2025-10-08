using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace STA.Electricity.API.Controllers
{
    /// <summary>
    /// Network Element Management Controller for electricity network hierarchy
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Manage network elements and hierarchy structure")]
    public class NetworkElementController : ControllerBase
    {
        private readonly INetworkElementService _service;

        public NetworkElementController(INetworkElementService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get network hierarchy tree structure
        /// </summary>
        /// <returns>Hierarchical network element tree</returns>
        [HttpGet("hierarchy")]
        [SwaggerOperation(
            Summary = "Get network hierarchy",
            Description = "Retrieve the complete network element hierarchy tree structure"
        )]
        public async Task<ActionResult<List<NetworkElementNodeDto>>> GetNetworkHierarchy()
        {
            try
            {
                var rootElements = await _service.GetNetworkHierarchyAsync();
                return Ok(rootElements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network hierarchy", error = ex.Message });
            }
        }

        /// <summary>
        /// Search network elements with filtering
        /// </summary>
        /// <param name="searchTerm">Search term for name or code</param>
        /// <param name="typeKey">Network element type filter</param>
        /// <param name="isActive">Active status filter</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated list of network elements</returns>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Search network elements",
            Description = "Search and filter network elements with pagination"
        )]
        public async Task<ActionResult<PagedResult<NetworkElementDto>>> SearchNetworkElements(
            [FromQuery] string? searchTerm,
            [FromQuery] int? typeKey,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.SearchNetworkElementsAsync(searchTerm, typeKey, isActive, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching network elements", error = ex.Message });
            }
        }

        /// <summary>
        /// Get network element by key
        /// </summary>
        /// <param name="key">Network element key</param>
        /// <returns>Network element details</returns>
        [HttpGet("{key}")]
        [SwaggerOperation(
            Summary = "Get network element by key",
            Description = "Retrieve detailed information about a specific network element"
        )]
        public async Task<ActionResult<NetworkElementDto>> GetNetworkElement(int key)
        {
            try
            {
                var element = await _service.GetNetworkElementAsync(key);
                if (element == null)
                {
                    return NotFound(new { message = "Network element not found" });
                }
                return Ok(element);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element", error = ex.Message });
            }
        }

        /// <summary>
        /// Get children of a network element
        /// </summary>
        /// <param name="parentKey">Parent network element key</param>
        /// <returns>List of child network elements</returns>
        [HttpGet("{parentKey}/children")]
        [SwaggerOperation(
            Summary = "Get network element children",
            Description = "Retrieve child network elements for a specific parent"
        )]
        public async Task<ActionResult<List<NetworkElementDto>>> GetNetworkElementChildren(int parentKey)
        {
            try
            {
                var children = await _service.GetChildrenAsync(parentKey);
                return Ok(children);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element children", error = ex.Message });
            }
        }

        /// <summary>
        /// Get network incidents for a specific network element
        /// </summary>
        /// <param name="networkElementKey">Network element key</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of network incidents</returns>
        [HttpGet("{networkElementKey}/incidents")]
        [SwaggerOperation(
            Summary = "Get network element incidents",
            Description = "Retrieve incidents associated with a specific network element"
        )]
        public async Task<ActionResult<PagedResult<NetworkIncidentDto>>> GetNetworkElementIncidents(
            int networkElementKey,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetIncidentsAsync(networkElementKey, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element incidents", error = ex.Message });
            }
        }
    }
}