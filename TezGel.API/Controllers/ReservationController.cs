using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TezGel.Application.DTOs.ActionReservation;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : BaseController
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
            => _reservationService = reservationService;

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateActionReserve([FromBody] ReserveProductRequest dto)
        {
            var userId = GetUserIdFromToken();
            await _reservationService.ReserveProductAsync(userId, dto.ProductId);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Ürün rezerve edildi."));
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
                await _reservationService.CompleteReservationAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Rezervasyon tamamlandı ve ürün pasif hale getirildi."));
            
        }
    }
}