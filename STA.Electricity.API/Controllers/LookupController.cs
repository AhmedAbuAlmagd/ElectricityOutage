using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Dtos;
using STA.Electricity.API.Interfaces;

namespace STA.Electricity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("sources")]
        public async Task<ActionResult<List<LookupItemDto>>> GetSources()
        {
            try
            {
                var sources = await _lookupService.GetSourcesAsync();
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
                var problemTypes = await _lookupService.GetProblemTypesAsync();
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
                var statuses = await _lookupService.GetStatusesAsync();
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
                var criteria = await _lookupService.GetSearchCriteriaAsync();
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
                var networkElementTypes = await _lookupService.GetNetworkElementTypesAsync();
                return Ok(networkElementTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving network element types", error = ex.Message });
            }
        }
    }
}