using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

namespace TezGel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;


        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            var businessUserId =Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Kullanıcı ID bulunamadı"));
            dto.BusinessUserId = businessUserId;

            await _productService.CreateProductAsync(dto);
            return Ok("Ürün başarıyla eklendi.");
        }
    }
}
