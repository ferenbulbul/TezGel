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
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]  ProductCreateRequest request)
        {
            var userId = GetUserIdFromToken();
            request.BusinessUserId = userId;

            await _productService.CreateProductAsync(request);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Ürün başarıyla eklendi."));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            var response = ApiResponse<List<ProductListResponse>>.SuccessResponse(products, "Ürünler başarıyla getirildi.");
            return Ok(response);
        }
    }
}
