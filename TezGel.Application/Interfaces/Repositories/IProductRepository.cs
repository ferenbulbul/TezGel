using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<Product>> GetAvailableProductsNearLocationAsync(double lat, double lng, double maxDistanceInKm);
    }
}