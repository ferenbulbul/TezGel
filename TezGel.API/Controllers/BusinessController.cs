using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TezGel.Application.DTOs.ActionReservation;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.DTOs.Product;
using TezGel.Application.Interfaces;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [Route("[controller]")]
    public class BusinessController : BaseController
    {
        private readonly IReservationService _reservationService;
        private readonly IProductService _productService;

        public BusinessController(IReservationService reservationService, IProductService productService)
        {
            _reservationService = reservationService;
            _productService = productService;
        }


        [HttpGet("reservations-list")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ReservationListByBusinessId()
        {
            var businessId = GetUserIdFromToken();
            var reservations = await _reservationService.GetReservationResponseListBusinessAsync(businessId);
            return Ok(ApiResponse<List<RezervationResponseListBusiness>>.SuccessResponse(reservations, "Rezervasyonlar başarıyla getirildi."));
        }
        [HttpGet("reservation")]
        public async Task<IActionResult> ReservationByBusinessId(Guid reservationId)
        {
            var reservations = await _reservationService.GetReservationBusinessAsync(reservationId);
            return Ok(ApiResponse<RezervationResponseListBusiness>.SuccessResponse(reservations, "Rezervasyonlar başarıyla getirildi."));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductCreateRequest request)
        {
            var userId = GetUserIdFromToken();

            await _productService.CreateProductAsync(request, userId);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Ürün başarıyla eklendi."));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("all-product")]
        public async Task<IActionResult> GetAllProduct()
        {
            var userId = GetUserIdFromToken();

            var products=await _productService.GetAllProductsByBusinessUserIdAsync(userId);
            return Ok(ApiResponse<List<BusinessProductListResponse>>.SuccessResponse(products, "Ürünler başarıyla getirildi."));
        }

        [HttpPost("product")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            var product=await _productService.GetProductById(productId);
            return Ok(ApiResponse<BusinessProductListResponse>.SuccessResponse(product, "Ürün başarıyla getirildi."));
        }
    }
}