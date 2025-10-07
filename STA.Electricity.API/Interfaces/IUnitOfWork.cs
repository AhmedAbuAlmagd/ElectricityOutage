using STA.Electricity.API.Models;

namespace STA.Electricity.API.Interfaces
{
    /// <summary>
    /// Unit of Work interface for managing database transactions and repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Only repositories used in sync
        IRepository<CuttingDownA> CuttingDownARepository { get; }
        IRepository<CuttingDownB> CuttingDownBRepository { get; }
        IRepository<CuttingDownHeader> CuttingDownHeaderRepository { get; }

        // Transaction management (used in sync)
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
