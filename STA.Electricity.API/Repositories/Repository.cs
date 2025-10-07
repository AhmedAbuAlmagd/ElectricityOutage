using Microsoft.EntityFrameworkCore;
using STA.Electricity.API.Interfaces;
using STA.Electricity.API.Models;

namespace STA.Electricity.API.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetOpenIncidentsAsync(int channelKey)
        {
            // This will be overridden in specific repositories
            throw new NotImplementedException("GetOpenIncidentsAsync must be implemented in derived classes");
        }

        public virtual async Task<int> GetMaxKeyAsync()
        {
            // This will be overridden in specific repositories
            throw new NotImplementedException("GetMaxKeyAsync must be implemented in derived classes");
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<int> UpdateClosedIncidentsAsync(int channelKey)
        {
            // This will be overridden in specific repositories
            throw new NotImplementedException("UpdateClosedIncidentsAsync must be implemented in derived classes");
        }
    }
}
