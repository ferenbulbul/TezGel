using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Product;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<Product>> GetAllWithIncludesAsync();
        Task<List<BusinessProductListResponse>> GetAllProductByBusinessUserIdAsync(Guid businessUserId);
        Task<BusinessProductListResponse?> GetProductByIdAsync(Guid producId);
    }
}