using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LookupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("sources")]
        public async Task<ActionResult<List<LookupItemDto>>> GetSources()
        {
            try
            {
                var sources = new List<LookupItemDto>
                {
                    new LookupItemDto { Key = 1, Name = "Cabin" },
                    new LookupItemDto { Key = 2, Name = "Cable" }
                };

                return Ok(sources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving sources", error = ex.Message });
            }
        }

        [HttpGet("problem-types")]
        public async Task<ActionResult<List<LookupItemDto>>> GetProblemTypes()
        {
            try
            {
                var problemTypes = await _context.ProblemTypes
                    .Select(x => new LookupItemDto
                    {
                        Key = x.ProblemTypeKey,
                        Name = x.ProblemTypeName ?? ""
                    })
                    .ToListAsync();

                return Ok(problemTypes);
            }   
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving problem types", error = ex.Message });
            }
        }

        [HttpGet("statuses")]
        public async Task<ActionResult<List<LookupItemDto>>> GetStatuses()
        {
            try
            {
                var statuses = new List<LookupItemDto>
                    {
                    new LookupItemDto { Key = 1, Name = "Open" },
                    new LookupItemDto { Key = 2, Name = "Closed" },
                    new LookupItemDto { Key = 3, Name = "In Progress" }
                };

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving statuses", error = ex.Message });
            }
        }

        [HttpGet("search-criteria")]
        public async Task<ActionResult<List<LookupItemDto>>> GetSearchCriteria()
        {
            try
            {
                var criteria = new List<LookupItemDto>
                {
                    new LookupItemDto { Key = 1, Name = "Incident ID" },
                    new LookupItemDto { Key = 2, Name = "Network Element" },
                    new LookupItemDto { Key = 3, Name = "Customer Count" }
                };

                return Ok(criteria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving search criteria", error = ex.Message });
            }
        }

        [HttpGet("network-element-types")]
        public async Task<ActionResult<List<LookupItemDto>>> GetNetworkElementTypes()
        {
            try
            {
                var networkElementTypes = await _context.NetworkElementTypes
                    .Select(x => new LookupItemDto
                    {
                        Key = x.NetworkElementTypeKey,
                        Name = x.NetworkElementTypeName ?? ""
                    })
                    .ToListAsync();

                return Ok(networkElementTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element types", error = ex.Message });
            }
        }
    }

    public class LookupItemDto
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}