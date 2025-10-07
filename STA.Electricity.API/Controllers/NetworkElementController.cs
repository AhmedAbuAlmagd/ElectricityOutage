using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
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
        private readonly AppDbContext _context;

        public NetworkElementController(AppDbContext context)
        {
            _context = context;
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
                var networkElements = await _context.NetworkElements
                    .OrderBy(x => x.NetworkElementName)
                    .ToListAsync();

                // Build hierarchy tree
                var rootElements = networkElements
                    .Where(x => x.ParentNetworkElementKey == null)
                    .Select(x => BuildNetworkNode(x, networkElements))
                    .ToList();

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
                var query = _context.NetworkElements.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.NetworkElementName.Contains(searchTerm));
                }

                if (typeKey.HasValue)
                {
                    query = query.Where(x => x.NetworkElementTypeKey == typeKey.Value);
                }

          

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.NetworkElementName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new NetworkElementDto
                    {
                        Key = x.NetworkElementKey,
                        Name = x.NetworkElementName,
                        Code = "",
                        TypeKey = x.NetworkElementTypeKey ?? 0,
                        ParentKey = x.ParentNetworkElementKey ?? 0,
                        IsActive = true
                    })
                    .ToListAsync();

                return Ok(new PagedResult<NetworkElementDto>
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
                var element = await _context.NetworkElements
                    .Where(x => x.NetworkElementKey == key)
                    .Select(x => new NetworkElementDto
                    {
                        Key = x.NetworkElementKey,
                        Name = x.NetworkElementName,
                        Code = "",
                        TypeKey = x.NetworkElementTypeKey ?? 0,
                        ParentKey = x.ParentNetworkElementKey ?? 0,
                        IsActive = true
                    })
                    .FirstOrDefaultAsync();

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
                var children = await _context.NetworkElements
                    .Where(x => x.ParentNetworkElementKey == parentKey)
                    .OrderBy(x => x.NetworkElementName)
                    .Select(x => new NetworkElementDto
                    {
                        Key = x.NetworkElementKey,
                        Name = x.NetworkElementName,
                        Code = "",
                        TypeKey = x.NetworkElementTypeKey ?? 0,
                        ParentKey = x.ParentNetworkElementKey ?? 0,
                        IsActive = true
                    })
                    .ToListAsync();

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
                var query = _context.CuttingDownDetails
                    .Where(x => x.NetworkElementKey == networkElementKey);

                var totalCount = await query.CountAsync();

                var incidents = await _context.CuttingDownDetails
                    .Where(x => x.NetworkElementKey == networkElementKey)
                    .OrderByDescending(x => x.ActualCreateDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new NetworkIncidentDto
                    {
                        CuttingIncidentId = x.CuttingDownDetailKey.ToString(),
                        NetworkElement = "",
                        StartDate = x.ActualCreateDate ?? DateTime.MinValue,
                        EndDate = x.ActualEndDate,
                        NumberOfImpactedCustomers = 0,
                        Status = x.ActualEndDate.HasValue ? "Closed" : "Open"
                    })
                    .ToListAsync();

                return Ok(new PagedResult<NetworkIncidentDto>
                {
                    Items = incidents,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element incidents", error = ex.Message });
            }
        }

        private NetworkElementNodeDto BuildNetworkNode(NetworkElement element, List<NetworkElement> allElements)
        {
            var node = new NetworkElementNodeDto
            {
                Id = element.NetworkElementKey,
                Name = element.NetworkElementName,
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

    public class NetworkElementDto
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int TypeKey { get; set; }
        public int? ParentKey { get; set; }
        public bool IsActive { get; set; }
    }

    public class NetworkElementNodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<NetworkElementNodeDto> Children { get; set; } = new List<NetworkElementNodeDto>();
    }

    public class NetworkIncidentDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElement { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfImpactedCustomers { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}