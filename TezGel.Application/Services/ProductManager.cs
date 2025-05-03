using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TezGel.Application.DTOs.Product;
using TezGel.Application.Expection;
using TezGel.Application.Expections;
using TezGel.Application.Interfaces;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Entities;

namespace TezGel.Application.Services
{

    public class ProductManager : IProductService
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILockService _lockService;

        public ProductManager(IProductRepository productRepository, ICategoryRepository categoryRepository, ILockService lockService)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _lockService = lockService;
        }

        public async Task CreateProductAsync(ProductCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessException("Ürün adı boş olamaz.");

            if (request.OriginalPrice <= 0 || request.DiscountedPrice <= 0)
                throw new BusinessException("Fiyatlar 0'dan büyük olmalıdır.");

            if (request.DiscountedPrice > request.OriginalPrice)
                throw new BusinessException("İndirimli fiyat, orijinal fiyattan yüksek olamaz.");

            if (request.ExpireAt <= DateTime.UtcNow)
                throw new BusinessException("Geçerlilik tarihi geçmiş olamaz.");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                throw new NotFoundException("Kategori bulunamadı.");
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                OriginalPrice = request.OriginalPrice,
                DiscountedPrice = request.DiscountedPrice,
                CategoryId = request.CategoryId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ImagePath = request.ImagePath,
                ExpireAt = request.ExpireAt,
                BusinessUserId = request.BusinessUserId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            try
            {
                await _productRepository.AddAsync(product);
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException("Ürün kaydedilemedi. Lütfen geçerli veriler gönderin.", ex);
            }
        }

        public async Task<List<ProductListResponse>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithIncludesAsync();

            if (products == null || !products.Any())
                throw new NotFoundException("Hiç ürün bulunamadı.");
            var list = new List<ProductListResponse>(products.Count);

            foreach (var p in products)
            {
                var key = $"product:lock:{p.Id}";
                var locked = await _lockService.IsLockedAsync(key);
                list.Add(new ProductListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    ImagePath = p.ImagePath,
                    ExpireAt = p.ExpireAt,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    CategoryName = p.Category?.Name ?? "-",
                    IsReserved = locked
                });
            }
            return list;

        }

        public async Task<List<ProductListResponse>> GetAvailableProductsAsync()
        {
            var all = await GetAllProductsAsync();
            return all.Where(p => !p.IsReserved).ToList();
        }

    }
}