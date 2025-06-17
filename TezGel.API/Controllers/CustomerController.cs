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
using TezGel.Application.DTOs.Customer;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [Route("[controller]")]
    public class CustomerController : BaseController
    {
        private readonly IReservationService _reservationService;
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService, IReservationService reservationService)
        {
            _customerService = customerService;
            _reservationService = reservationService;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("location-update")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationRequest locationRequest)
        {
            var userId = GetUserIdFromToken();
            await _customerService.LocationUpdateAsync(userId, locationRequest);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Location updated successfully."));
        }
        [HttpGet("reservation-list")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ReservationListByid()
        {
            var userId = GetUserIdFromToken();
            var reservations = await _reservationService.GetReservationByUserIdAsync(userId);
            return Ok(ApiResponse<List<RezervationResponseList>>.SuccessResponse(reservations, "Rezervasyon listesi getirildi."));
        }

        [HttpGet("reservation")]
        public async Task<IActionResult> Reservation(Guid reservationId)
        {
            var reservations = await _reservationService.GetReservationByIdAsync(reservationId);
            return Ok(ApiResponse<RezervationResponseList>.SuccessResponse(reservations, "Rezervasyon  getirildi."));
        }
        [HttpGet("reservation-status")]
        public async Task<IActionResult> ReservationStatus(Guid reservationId)
        {
            var reservations = await _reservationService.GetReservationStatusAsync(reservationId);
            return Ok(ApiResponse<string>.SuccessResponse(reservations, "Rezervasyon  getirildi."));
        }

    }
}