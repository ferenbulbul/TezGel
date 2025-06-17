using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
        private readonly IBusinessUserRepository _businessUserRepository;
        private readonly UserManager<AppUser> _userManager;


        public ProductManager(IProductRepository productRepository, ICategoryRepository categoryRepository, ILockService lockService, IBusinessUserRepository businessUserRepository, UserManager<AppUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _lockService = lockService;
            _businessUserRepository = businessUserRepository;
            _userManager = userManager;
        }

        public async Task CreateProductAsync(ProductCreateRequest request, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessException("Ürün adı boş olamaz.");

            if (request.OriginalPrice <= 0 || request.DiscountedPrice <= 0)
                throw new BusinessException("Fiyatlar 0'dan büyük olmalıdır.");

            if (request.DiscountedPrice > request.OriginalPrice)
                throw new BusinessException("İndirimli fiyat, orijinal fiyattan yüksek olamaz.");

            var business = await _businessUserRepository.GetByIdAsync(userId);
            var business2 = await _userManager.FindByIdAsync(userId.ToString());
            if (business == null || business2 == null)
                throw new NotFoundException("İşletme bulunamadı.");
            var trZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var todayTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, trZone).Date;
            var localExpire = todayTr.Add(business.ClosingTime);
            var expireDate = TimeZoneInfo.ConvertTimeToUtc(localExpire, trZone);
            Console.WriteLine($"Expire Date: {expireDate}");
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
                Latitude = business2.Latitude,
                Longitude = business2.Longitute,
                ImagePath = request.ImagePath,
                ExpireAt = expireDate,
                BusinessUserId = userId,
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

            if (products == null)
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
                    IsReserved = locked,
                });
            }
            return list;

        }

        public async Task<List<ProductListResponse>> GetAvailableProductsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("Kullanıcı bulunamadı.");
            var all = await GetAllProductsAsync();

            return all
                .Where(p => !p.IsReserved && p.ExpireAt > DateTime.UtcNow)
                .Select(p =>
                {
                    var distance = CalculateDistanceInMeters(user.Latitude, user.Longitute, p.Latitude, p.Longitude);
                    p.DistanceInMeters = distance;
                    return p;
                })
                .Where(p => p.DistanceInMeters <= 9000000)
                .ToList();
            ;

        }

        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadius = 6371000; // metre

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        private static double ToRadians(double angle) => angle * Math.PI / 180.0;

        public async Task<List<CategoryResponse>> GetCategoryList()
        {
            var categoryList = await _categoryRepository.GetAllAsync();
            if (categoryList == null)
                throw new NotFoundException($"Category bulunamadı.");
            var categoryListResponse = new List<CategoryResponse>();
            foreach (var category in categoryList)
            {
                categoryListResponse.Add(new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                });
            }
            return categoryListResponse;
        }
        public async Task<List<BusinessProductListResponse>> GetAllProductsByBusinessUserIdAsync(Guid businessUserId)
        {
            var products = await _productRepository.GetAllProductByBusinessUserIdAsync(businessUserId);
            var businessProduct = new List<BusinessProductListResponse>();
            foreach (var p in products)
            {
                var key = $"product:lock:{p.Id}";
                var locked = await _lockService.IsLockedAsync(key);
                businessProduct.Add(new BusinessProductListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    ImagePath = p.ImagePath,
                    CategoryName = p.CategoryName,
                    IsActive = p.IsActive,
                    IsReserved = locked
                });
            }
            if (products == null)
                throw new NotFoundException("Hiç ürün bulunamadı.");
            return products;
        }
        public async Task<BusinessProductListResponse> GetProductById(Guid productId)
        {
            var p = await _productRepository.GetProductByIdAsync(productId);
            if (p == null)
                throw new NotFoundException("Ürün bulunamadı.");
            var key = $"product:lock:{p.Id}";
            var locked = await _lockService.IsLockedAsync(key);
            var businessProduct=new BusinessProductListResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                OriginalPrice = p.OriginalPrice,
                DiscountedPrice = p.DiscountedPrice,
                ImagePath = p.ImagePath,
                CategoryName = p.CategoryName,
                IsActive = p.IsActive,
                IsReserved = locked
            };
            return businessProduct;
        }
    }

}