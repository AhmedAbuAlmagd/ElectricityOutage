using Microsoft.EntityFrameworkCore.Storage;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Repositories
{
    /// <summary>
    /// Unit of Work implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositories
        private IRepository<CuttingDownA>? _cuttingDownARepository;
        private IRepository<CuttingDownB>? _cuttingDownBRepository;
        private IRepository<CuttingDownHeader>? _cuttingDownHeaderRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<CuttingDownA> CuttingDownARepository =>
            _cuttingDownARepository ??= new CuttingDownARepository(_context);

        public IRepository<CuttingDownB> CuttingDownBRepository =>
            _cuttingDownBRepository ??= new CuttingDownBRepository(_context);

        public IRepository<CuttingDownHeader> CuttingDownHeaderRepository =>
            _cuttingDownHeaderRepository ??= new CuttingDownHeaderRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
