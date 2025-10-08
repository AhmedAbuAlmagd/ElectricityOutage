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
                // Phase 1: Execute SP_Create to create open incidents (guard if SP exists)
                var spCreateExists = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM sys.procedures WHERE name = 'SP_Create' AND SCHEMA_NAME(schema_id) = 'FTA'");
                if (spCreateExists > 0)
                {
                    await connection.ExecuteAsync("FTA.SP_Create", new { ChannelKey = channelKey }, commandType: CommandType.StoredProcedure);
                }

                // Phase 2: Execute SP_Close to close completed incidents (guard if SP exists)
                var spCloseExists = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM sys.procedures WHERE name = 'SP_Close' AND SCHEMA_NAME(schema_id) = 'FTA'");
                if (spCloseExists > 0)
                {
                    await connection.ExecuteAsync("FTA.SP_Close", new { ChannelKey = channelKey }, commandType: CommandType.StoredProcedure);
                }

                var insertedDetails = await InsertMissingDetailsAsync(connection, channelKey);

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
                    TotalProcessed = newCreatedCount + closedCount,
                    InsertedDetails = insertedDetails
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

        private async Task<int> InsertMissingDetailsAsync(SqlConnection connection, int channelKey)
        {
            // Map STA table and column names based on channel
            string staTable = channelKey == 1 ? "STA.Cutting_Down_A" : "STA.Cutting_Down_B";
            string staIdColumn = channelKey == 1 ? "Cutting_Down_A_Incident_ID" : "Cutting_Down_B_Incident_ID";
            string staNameColumn = channelKey == 1 ? "Cutting_Down_Cabin_Name" : "Cutting_Down_Cable_Name";
            int networkElementTypeKey = channelKey == 1 ? 6 : 7; // 6: Cabin, 7: Cable

            var sql = $@"
                SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
                ;WITH NewHeaders AS (
                    SELECT h.Cutting_Down_Key, h.Cutting_Down_Incident_ID, h.ActualCreateDate, h.ActualEndDate
                    FROM FTA.Cutting_Down_Header h
                    WHERE h.Channel_Key = @ChannelKey
                      AND NOT EXISTS (
                          SELECT 1 FROM FTA.Cutting_Down_Detail d WHERE d.Cutting_Down_Key = h.Cutting_Down_Key
                      )
                ), SourceRows AS (
                    SELECT s.{staIdColumn} AS IncidentId, RTRIM(LTRIM(s.{staNameColumn})) AS ElementName
                    FROM {staTable} s
                ), Matches AS (
                    SELECT nh.Cutting_Down_Key,
                           ne.Network_Element_Key,
                           nh.ActualCreateDate,
                           nh.ActualEndDate
                    FROM NewHeaders nh
                    LEFT JOIN SourceRows sr ON sr.IncidentId = nh.Cutting_Down_Incident_ID
                    OUTER APPLY (
                        SELECT TOP 1 x.Network_Element_Key
                        FROM FTA.Network_Element x
                        WHERE RTRIM(LTRIM(x.Network_Element_Name)) = sr.ElementName
                          AND x.Network_Element_Type_Key = @NetworkElementTypeKey
                        ORDER BY x.Network_Element_Key
                    ) ne
                ), Seed AS (
                    SELECT ISNULL(MAX(Cutting_Down_Detail_Key), 0) AS MaxKey FROM FTA.Cutting_Down_Detail WITH (UPDLOCK, HOLDLOCK)
                ), Numbered AS (
                    SELECT m.*, ROW_NUMBER() OVER (ORDER BY m.Cutting_Down_Key, ISNULL(m.Network_Element_Key, 0)) AS rn
                    FROM Matches m
                )
                INSERT INTO FTA.Cutting_Down_Detail (
                    Cutting_Down_Detail_Key,
                    Cutting_Down_Key,
                    Network_Element_Key,
                    ActualCreateDate,
                    ActualEndDate,
                    ImpactedCustomers
                )
                SELECT s.MaxKey + n.rn,
                       n.Cutting_Down_Key,
                       n.Network_Element_Key,
                       ISNULL(n.ActualCreateDate, GETDATE()),
                       n.ActualEndDate,
                       0
                FROM Numbered n CROSS JOIN Seed s;";

            // Ensure concurrency safety by running in a transaction
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var rows = await connection.ExecuteAsync(sql, new { ChannelKey = channelKey, NetworkElementTypeKey = networkElementTypeKey }, transaction);
                transaction.Commit();
                return rows;
            }
            catch
            {
                try { transaction.Rollback(); } catch { }
                throw;
            }
        }
    }
}
