using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using STA.Electricity.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace STA.Electricity.API.Controllers
{
    /// <summary>
    /// Test Data Generation Controller for STA Electricity Outage Management System
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Generate test data for electricity cutting down incidents")]
    public class TestDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();

        public TestDataController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generate test data for Cabin Incidents (Source A)
        /// </summary>
        /// <param name="request">Test data generation request with count and scenario</param>
        /// <returns>Generated cabin incidents data</returns>
        /// <remarks>
        /// Creates realistic test data for Cutting_Down_A table with different business scenarios:
        ///
        /// **Scenarios:**
        /// - **planned**: Scheduled maintenance with planned start/end times (70% closure rate)
        /// - **emergency**: Unplanned outages, 30% chance of being global (40% closure rate)
        /// - **global**: Wide-area outages affecting multiple regions (50% closure rate)
        /// - **mixed**: Random combination of all scenarios (60% closure rate)
        ///
        /// **Sample Request:**
        /// ```json
        /// {
        ///   "count": 5,
        ///   "scenario": "planned"
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">Successfully generated cabin incidents</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("cabin-incidents")]
        [SwaggerOperation(
            Summary = "Generate Cabin Incidents (Source A)",
            Description = "Creates test data for cabin-related electricity outages with various business scenarios",
            OperationId = "GenerateCabinIncidents",
            Tags = new[] { "Test Data Generation" }
        )]
        [SwaggerResponse(200, "Successfully generated cabin incidents", typeof(ApiResponse<List<CuttingDownA>>))]
        [SwaggerResponse(400, "Invalid request parameters")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GenerateCabinIncidents([FromBody] TestDataRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var incidents = new List<CuttingDownA>();
                
                for (int i = 0; i < request.Count; i++)
                {
                    var incident = await GenerateCabinIncidentAsync(connection, request.Scenario);
                    incidents.Add(incident);
                    
                    var sql = @"
                        INSERT INTO STA.Cutting_Down_A 
                        (Cutting_Down_A_Incident_ID, Cutting_Down_Cabin_Name, Problem_Type_Key, 
                         CreateDate, EndDate, IsPlanned, IsGlobal, PlannedStartDTS, PlannedEndDTS, 
                         IsActive, CreatedUser, UpdatedUser)
                        VALUES 
                        (@CuttingDownAIncidentId, @CuttingDownCabinName, @ProblemTypeKey, 
                         @CreateDate, @EndDate, @IsPlanned, @IsGlobal, @PlannedStartDts, @PlannedEndDts, 
                         @IsActive, @CreatedUser, @UpdatedUser)";
                    
                    await connection.ExecuteAsync(sql, incident);
                }

                return Ok(new { 
                    Success = true, 
                    Message = $"Generated {request.Count} cabin incidents for scenario: {request.Scenario}",
                    Data = incidents 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        /// <summary>
        /// Generate test data for Cable Incidents (Source B)
        /// </summary>
        /// <param name="request">Test data generation request with count and scenario</param>
        /// <returns>Generated cable incidents data</returns>
        /// <remarks>
        /// Creates realistic test data for Cutting_Down_B table with different business scenarios:
        ///
        /// **Scenarios:**
        /// - **planned**: Scheduled maintenance with planned start/end times (80% closure rate)
        /// - **emergency**: Unplanned outages, 40% chance of being global (30% closure rate)
        /// - **global**: Wide-area outages affecting multiple regions (60% closure rate)
        /// - **mixed**: Random combination of all scenarios (55% closure rate)
        ///
        /// **Sample Request:**
        /// ```json
        /// {
        ///   "count": 3,
        ///   "scenario": "emergency"
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">Successfully generated cable incidents</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("cable-incidents")]
        [SwaggerOperation(
            Summary = "Generate Cable Incidents (Source B)",
            Description = "Creates test data for cable-related electricity outages with various business scenarios",
            OperationId = "GenerateCableIncidents",
            Tags = new[] { "Test Data Generation" }
        )]
        [SwaggerResponse(200, "Successfully generated cable incidents", typeof(ApiResponse<List<CuttingDownB>>))]
        [SwaggerResponse(400, "Invalid request parameters")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GenerateCableIncidents([FromBody] TestDataRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var incidents = new List<CuttingDownB>();
                
                for (int i = 0; i < request.Count; i++)
                {
                    var incident = await GenerateCableIncidentAsync(connection, request.Scenario);
                    incidents.Add(incident);
                    
                    var sql = @"
                        INSERT INTO STA.Cutting_Down_B 
                        (Cutting_Down_B_Incident_ID, Cutting_Down_Cable_Name, Problem_Type_Key, 
                         CreateDate, EndDate, IsPlanned, IsGlobal, PlannedStartDTS, PlannedEndDTS, 
                         IsActive, CreatedUser, UpdatedUser)
                        VALUES 
                        (@CuttingDownBIncidentId, @CuttingDownCableName, @ProblemTypeKey, 
                         @CreateDate, @EndDate, @IsPlanned, @IsGlobal, @PlannedStartDts, @PlannedEndDts, 
                         @IsActive, @CreatedUser, @UpdatedUser)";
                    
                    await connection.ExecuteAsync(sql, incident);
                }

                return Ok(new { 
                    Success = true, 
                    Message = $"Generated {request.Count} cable incidents for scenario: {request.Scenario}",
                    Data = incidents 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        private async Task<CuttingDownA> GenerateCabinIncidentAsync(SqlConnection connection, string scenario)
        {
            // Get actual cabin names from database (Network_Element_Type_Key = 6 for Cabin)
            var cabinName = await GetRandomNetworkElementAsync(connection, 6);

            var incident = new CuttingDownA
            {
                CuttingDownAIncidentId = _random.Next(100000, 999999),
                CuttingDownCabinName = cabinName,
                ProblemTypeKey = _random.Next(1, 13), 
                CreateDate = DateTime.Now,
                IsActive = true,
                CreatedUser = "SourceA", 
                UpdatedUser = "SourceA"
            };

            switch (scenario.ToLower())
            {
                case "planned":
                    incident.IsPlanned = true;
                    incident.IsGlobal = false;
                    incident.PlannedStartDts = incident.CreateDate?.AddHours(2);
                    incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(4);
                    if (_random.NextDouble() < 0.7)
                    {
                        incident.EndDate = incident.PlannedEndDts?.AddMinutes(_random.Next(-30, 60));
                    }
                    break;

                case "emergency":
                    incident.IsPlanned = false;
                    incident.IsGlobal = _random.NextDouble() < 0.3; 
                    if (_random.NextDouble() < 0.4)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(1, 12));
                    }
                    break;

                case "global":
                    incident.IsPlanned = _random.NextDouble() < 0.5;
                    incident.IsGlobal = true;
                    if (incident.IsPlanned == true)
                    {
                        incident.PlannedStartDts = incident.CreateDate?.AddHours(1);
                        incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(6);
                    }
                    if (_random.NextDouble() < 0.5)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(2, 24));
                    }
                    break;

                default: 
                    incident.IsPlanned = _random.NextDouble() < 0.6;
                    incident.IsGlobal = _random.NextDouble() < 0.2;
                    if (incident.IsPlanned == true)
                    {
                        incident.PlannedStartDts = incident.CreateDate?.AddHours(_random.Next(1, 48));
                        incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(_random.Next(2, 8));
                    }
                    if (_random.NextDouble() < 0.6)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(1, 48));
                    }
                    break;
            }

            return incident;
        }

        private async Task<CuttingDownB> GenerateCableIncidentAsync(SqlConnection connection, string scenario)
        {
            var cableName = await GetRandomNetworkElementAsync(connection, 7);

            var incident = new CuttingDownB
            {
                CuttingDownBIncidentId = _random.Next(100000, 999999),
                CuttingDownCableName = "cab-1-1",
                ProblemTypeKey = _random.Next(1, 13), 
                CreateDate = DateTime.Now,
                IsActive = true,
                CreatedUser = "SourceB",
                UpdatedUser = "SourceB"
            };

            switch (scenario.ToLower())
            {
                case "planned":
                    incident.IsPlanned = true;
                    incident.IsGlobal = false;
                    incident.PlannedStartDts = incident.CreateDate?.AddHours(2);
                    incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(3);
                    if (_random.NextDouble() < 0.8)
                    {
                        incident.EndDate = incident.PlannedEndDts?.AddMinutes(_random.Next(-15, 30));
                    }
                    break;

                case "emergency":
                    incident.IsPlanned = false;
                    incident.IsGlobal = _random.NextDouble() < 0.4;
                    if (_random.NextDouble() < 0.3)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(1, 8));
                    }
                    break;

                case "global":
                    incident.IsPlanned = _random.NextDouble() < 0.4;
                    incident.IsGlobal = true;
                    if (incident.IsPlanned == true)
                    {
                        incident.PlannedStartDts = incident.CreateDate?.AddHours(1);
                        incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(4);
                    }
                    if (_random.NextDouble() < 0.6)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(2, 18));
                    }
                    break;

                default: 
                    incident.IsPlanned = _random.NextDouble() < 0.5;
                    incident.IsGlobal = _random.NextDouble() < 0.25;
                    if (incident.IsPlanned == true)
                    {
                        incident.PlannedStartDts = incident.CreateDate?.AddHours(_random.Next(1, 24));
                        incident.PlannedEndDts = incident.PlannedStartDts?.AddHours(_random.Next(1, 6));
                    }
                    if (_random.NextDouble() < 0.55)
                    {
                        incident.EndDate = incident.CreateDate?.AddHours(_random.Next(1, 36));
                    }
                    break;
            }

            return incident;
        }

        /// <summary>
        /// Get a random network element name from the database
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="networkElementTypeKey">Network element type key (6=Cabin, 7=Cable)</param>
        /// <returns>Random network element name</returns>
        private async Task<string> GetRandomNetworkElementAsync(SqlConnection connection, int networkElementTypeKey)
        {
            try
            {
                var elements = await connection.QueryAsync<string>(@"
                    SELECT TOP 100 Network_Element_Name
                    FROM FTA.Network_Element
                    WHERE Network_Element_Type_Key = @TypeKey
                    AND IsActive = 1
                    ORDER BY NEWID()", new { TypeKey = networkElementTypeKey });

                var elementList = elements.ToList();
                if (elementList.Any())
                {
                    return elementList[_random.Next(elementList.Count)];
                }
                else
                {
                    // Fallback to generated names if no elements found
                    return networkElementTypeKey == 6 ? $"Cabin_{_random.Next(1000, 9999)}" : $"Cable_{_random.Next(1000, 9999)}";
                }
            }
            catch
            {
                // Fallback to generated names on error
                return networkElementTypeKey == 6 ? $"Cabin_{_random.Next(1000, 9999)}" : $"Cable_{_random.Next(1000, 9999)}";
            }
        }
    }

    /// <summary>
    /// Request model for test data generation
    /// </summary>
    public class TestDataRequest
    {
        /// <summary>
        /// Number of incidents to generate (1-100)
        /// </summary>
        /// <example>5</example>
        [Range(1, 100, ErrorMessage = "Count must be between 1 and 100")]
        [SwaggerSchema("Number of incidents to generate")]
        public int Count { get; set; } = 10;

        /// <summary>
        /// Business scenario for incident generation
        /// </summary>
        /// <example>planned</example>
        [SwaggerSchema("Business scenario: planned, emergency, global, or mixed")]
        public string Scenario { get; set; } = "mixed";
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details (if any)
        /// </summary>
        public string? Error { get; set; }
    }
}
