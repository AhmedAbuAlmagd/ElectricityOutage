using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using STA.Electricity.API.Interfaces;

namespace STA.Electricity.API.Controllers
{
    /// <summary>
    /// Synchronization Controller for transferring data from STA to FTA
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Synchronize incidents between STA and FTA databases")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>
        /// Synchronize incidents from STA to FTA
        /// </summary>
        /// <param name="source">Source system (A for cabins, B for cables)</param>
        /// <returns>Synchronization result</returns>
        /// <remarks>
        /// Executes the synchronization process to transfer incident data from Staging Tables Area (STA)
        /// to Fact Tables Area (FTA). This involves:
        ///
        /// 1. **SP_Create**: Creates new incidents in FTA from open incidents in STA
        /// 2. **SP_Close**: Closes incidents in FTA that are closed in STA
        ///
        /// **Source Options:**
        /// - **A**: Process cabin incidents (Cutting_Down_A table)
        /// - **B**: Process cable incidents (Cutting_Down_B table)
        ///
        /// **Process Flow:**
        /// - Open incidents (EndDate = null) → Create in FTA
        /// - Closed incidents (EndDate != null) → Close in FTA
        /// - Unmatched network elements → Move to Cutting_Down_Ignored
        /// </remarks>
        /// <response code="200">Synchronization completed successfully</response>
        /// <response code="500">Internal server error during synchronization</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Synchronize STA to FTA",
            Description = "Transfer incident data from Staging Tables Area to Fact Tables Area",
            OperationId = "SynchronizeIncidents",
            Tags = new[] { "Data Synchronization" }
        )]
        [SwaggerResponse(200, "Synchronization completed successfully")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> Run([FromQuery] string source = "A")
        {
            try
            {
                var result = await _syncService.SynchronizeAsync(source);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    Success = false,
                    Error = ex.Message,
                    Source = source
                });
            }
        }
    }
}
