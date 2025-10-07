using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace STA.Electricity.API.Services
{
    /// <summary>
    /// Service for synchronization operations between STA and FTA
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly IConfiguration _configuration;

        public SyncService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SyncResult> SynchronizeAsync(string source)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                int channelKey = source.ToUpper() == "A" ? 1 : 2;
                int initialCount = await connection.QuerySingleOrDefaultAsync<int>(@"
                    SELECT COUNT(*) FROM FTA.Cutting_Down_Header
                    WHERE Channel_Key = @ChannelKey
                    AND CAST(SynchCreateDate AS DATE) = CAST(GETDATE() AS DATE)",
                    new { ChannelKey = channelKey });
                // Phase 1: Execute SP_Create to create open incidents
                await connection.ExecuteAsync("FTA.SP_Create", new { ChannelKey = channelKey },
                    commandType: CommandType.StoredProcedure);

                // Phase 2: Execute SP_Close to close completed incidents
                await connection.ExecuteAsync("FTA.SP_Close", new { ChannelKey = channelKey },
                    commandType: CommandType.StoredProcedure);

                // Get counts for response (optional - you can remove this if you don't need exact counts)
                var createdCount = await connection.QuerySingleOrDefaultAsync<int>(@"
                    SELECT COUNT(*) FROM FTA.Cutting_Down_Header
                    WHERE Channel_Key = @ChannelKey
                    AND CAST(SynchCreateDate AS DATE) = CAST(GETDATE() AS DATE)",
                    new { ChannelKey = channelKey });

                var newCreatedCount = createdCount - initialCount;

                var closedCount = await connection.QuerySingleOrDefaultAsync<int>(@"
                    SELECT COUNT(*) FROM FTA.Cutting_Down_Header
                    WHERE Channel_Key = @ChannelKey
                    AND ActualEndDate IS NOT NULL
                    AND CAST(SynchUpdateDate AS DATE) = CAST(GETDATE() AS DATE)",
                    new { ChannelKey = channelKey });

                return new SyncResult
                {
                    Success = true,
                    Message = $"Complete synchronization finished for Source {source} using stored procedures",
                    Source = source,
                    ChannelKey = channelKey,
                    CreatedIncidents = newCreatedCount,
                    ClosedIncidents = closedCount,
                    TotalProcessed = newCreatedCount + closedCount
                };
            }
            catch (Exception ex)
            {
                return new SyncResult
                {
                    Success = false,
                    Source = source,
                    Error = ex.Message
                };
            }
        }
    }
}
