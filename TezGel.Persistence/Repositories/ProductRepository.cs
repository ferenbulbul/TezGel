using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TezGel.Application.DTOs.Product;
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
                .Where(p => !p.IsDeleted && p.IsActive)
                .ToListAsync();
        }
        public async Task<List<BusinessProductListResponse>> GetAllProductByBusinessUserIdAsync(Guid businessUserId)
        {
            return await _context.Products
                .Where(p => p.BusinessUserId == businessUserId && !p.IsDeleted)
                .Select(p => new BusinessProductListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    ImagePath = p.ImagePath,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive,
                    IsReserved = null
                })
                .ToListAsync();
        }
        public async Task<BusinessProductListResponse?> GetProductByIdAsync(Guid producId)
        {
            return await _context.Products
                .Where(p => p.Id == producId && !p.IsDeleted)
                .Select(p => new BusinessProductListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    ImagePath = p.ImagePath,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive,
                    IsReserved = null
                })
                .FirstOrDefaultAsync();
        }
    }
}