using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Product;

namespace TezGel.Application.Interfaces
{
    public interface IProductService
    {
        Task CreateProductAsync(ProductCreateRequest request);
        Task<List<ProductListResponse>> GetAllProductsAsync();
        Task<List<ProductListResponse>> GetAvailableProductsAsync();
    }
}