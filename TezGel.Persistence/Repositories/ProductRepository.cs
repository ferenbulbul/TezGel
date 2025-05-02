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

        public async Task<List<Product>> GetAvailableProductsNearLocationAsync(double lat, double lng, double maxDistanceInKm)
        {
            // Basit mesafe filtresi (haversine yerine düz kare mesafe örneği)
            return await _context.Products
                .Where(p => p.IsActive && !p.IsDeleted && p.ExpireAt > DateTime.UtcNow)
                .Where(p =>
                    Math.Abs(p.Latitude - lat) < 0.1 &&  // basit kare kutu
                    Math.Abs(p.Longitude - lng) < 0.1)
                .ToListAsync();
        }
    }
}