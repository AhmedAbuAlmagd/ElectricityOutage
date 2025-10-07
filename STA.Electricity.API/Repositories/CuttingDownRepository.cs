using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace STA.Electricity.API.Repositories
{

    public class CuttingDownARepository : Repository<CuttingDownA>
    {
        public CuttingDownARepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<CuttingDownA>> GetOpenIncidentsAsync(int channelKey)
        {
            var connection = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            return await connection.QueryAsync<CuttingDownA>(@"
                SELECT
                    Cutting_Down_A_Incident_ID,
                    Problem_Type_Key,
                    CreateDate,
                    IsPlanned,
                    IsGlobal,
                    PlannedStartDTS,
                    PlannedEndDTS,
                    IsActive
                FROM STA.Cutting_Down_A
                WHERE EndDate IS NULL
                AND IsActive = 1
                AND Cutting_Down_A_Incident_ID NOT IN (
                    SELECT ISNULL(Cutting_Down_Incident_ID, 0)
                    FROM FTA.Cutting_Down_Header
                    WHERE Channel_Key = @ChannelKey
                )", new { ChannelKey = channelKey }, transaction);
        }
    }


    public class CuttingDownBRepository : Repository<CuttingDownB>
    {
        public CuttingDownBRepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<CuttingDownB>> GetOpenIncidentsAsync(int channelKey)
        {
            var connection = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            return await connection.QueryAsync<CuttingDownB>(@"
                SELECT
                    Cutting_Down_B_Incident_ID,
                    Problem_Type_Key,
                    CreateDate,
                    IsPlanned,
                    IsGlobal,
                    PlannedStartDTS,
                    PlannedEndDTS,
                    IsActive
                FROM STA.Cutting_Down_B
                WHERE EndDate IS NULL
                AND IsActive = 1
                AND Cutting_Down_B_Incident_ID NOT IN (
                    SELECT ISNULL(Cutting_Down_Incident_ID, 0)
                    FROM FTA.Cutting_Down_Header
                    WHERE Channel_Key = @ChannelKey
                )", new { ChannelKey = channelKey }, transaction);
        }
    }

    public class CuttingDownHeaderRepository : Repository<CuttingDownHeader>
    {
        public CuttingDownHeaderRepository(AppDbContext context) : base(context) { }

        public override async Task<int> GetMaxKeyAsync()
        {
            var connection = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            return await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT MAX(Cutting_Down_Key) FROM FTA.Cutting_Down_Header",
                transaction: transaction) ?? 0;
        }

        public override async Task<int> UpdateClosedIncidentsAsync(int channelKey)
        {
            var connection = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            string sourceTable = channelKey == 1 ? "STA.Cutting_Down_A" : "STA.Cutting_Down_B";
            string incidentIdColumn = channelKey == 1 ? "Cutting_Down_A_Incident_ID" : "Cutting_Down_B_Incident_ID";

            return await connection.ExecuteAsync($@"
                UPDATE h SET
                    ActualEndDate = s.EndDate,
                    SynchUpdateDate = GETDATE(),
                    UpdateSystemUserID = @UpdateUserId
                FROM FTA.Cutting_Down_Header h
                INNER JOIN {sourceTable} s ON h.Cutting_Down_Incident_ID = s.{incidentIdColumn}
                WHERE h.Channel_Key = @ChannelKey
                AND s.EndDate IS NOT NULL
                AND h.ActualEndDate IS NULL",
                new {
                    ChannelKey = channelKey,
                    UpdateUserId = channelKey == 1 ? 3 : 4
                }, transaction);
        }
    }
}
