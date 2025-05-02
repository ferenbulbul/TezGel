using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Product;
using TezGel.Application.Interfaces;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Entities;

namespace TezGel.Application.Services
{

    public class ProductManager : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;

        public ProductManager(IProductRepository productRepository, IFileService fileService)
        {
            _productRepository = productRepository;
            _fileService = fileService;
        }

        public async Task CreateProductAsync(ProductCreateDto dto)
        {
            var imagePath = await _fileService.SaveImageAsync(dto.ImageFile);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                OriginalPrice = dto.OriginalPrice,
                DiscountedPrice = dto.DiscountedPrice,
                CategoryId = dto.CategoryId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImagePath = imagePath,
                ExpireAt = dto.ExpireAt,
                BusinessUserId = dto.BusinessUserId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _productRepository.AddAsync(product);
        }
    }
}