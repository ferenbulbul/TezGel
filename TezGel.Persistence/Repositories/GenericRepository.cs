using Microsoft.EntityFrameworkCore;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Common;
using TezGel.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly TezGelDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(TezGelDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(); // MUTLAKA veritabanına kaydet
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(); // Update sonrası hemen save
        }

        public async Task DeleteAsync(T entity)
        {
            entity.IsDeleted = true; // Soft delete yap
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(); // Save değişikliği
        }

    }
}
