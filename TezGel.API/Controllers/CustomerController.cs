using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.DTOs.Customer;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [Route("[controller]")]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("location/update")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationRequest locationRequest)
        {
            var userId = GetUserIdFromToken();
            await _customerService.LocationUpdateAsync(userId, locationRequest);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Location updated successfully."));
        }
    }
}