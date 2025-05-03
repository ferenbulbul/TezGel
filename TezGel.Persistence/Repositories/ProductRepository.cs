using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Entities;
using TezGel.Persistence.Context;

namespace TezGel.Persistence.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(TezGelDbContext context) : base(context)
        {
        }

        public async Task<List<Product>> GetAllWithIncludesAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive && p.ExpireAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}