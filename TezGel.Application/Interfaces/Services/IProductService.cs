using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Product;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces
{
    public interface IProductService
    {
        Task CreateProductAsync(ProductCreateRequest request, Guid userId);
        Task<List<ProductListResponse>> GetAllProductsAsync();
        Task<List<ProductListResponse>> GetAvailableProductsAsync(Guid userId);
        Task<List<CategoryResponse>> GetCategoryList();
        Task<List<BusinessProductListResponse>> GetAllProductsByBusinessUserIdAsync(Guid businessUserId);
        Task<BusinessProductListResponse> GetProductById(Guid productId);

    }
}