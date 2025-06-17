using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TezGel.Application.DTOs.Product;
using TezGel.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using TezGel.Infrastructure.Services;
using TezGel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TezGel.Application.DTOs.Auth.Comman;
using System.Data.Common;
using TezGel.Domain.Entities;

namespace TezGel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;


        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            var response = ApiResponse<List<ProductListResponse>>.SuccessResponse(products, "Ürünler başarıyla getirildi.");
            return Ok(response);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var userId = GetUserIdFromToken();
            var products = await _productService.GetAvailableProductsAsync(userId);
            var response = ApiResponse<List<ProductListResponse>>
                               .SuccessResponse(products, "Mevcut ürünler başarıyla getirildi.");
            return Ok(response);
        }

        [HttpGet("category-list")]
        public async Task<IActionResult> CategoryList()
        {
            var categories = await _productService.GetCategoryList();
            var response = ApiResponse<List<CategoryResponse>>
                               .SuccessResponse(categories, "Mevcut categoryler başarıyla getirildi.");
            return Ok(response);
        }
    }
}
