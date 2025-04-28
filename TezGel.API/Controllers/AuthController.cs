using Microsoft.AspNetCore.Mvc;
using TezGel.Application.DTOs.Auth;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequest request)
        {
            await _authService.RegisterCustomerAsync(request);
            var response = ApiResponse<string>.SuccessResponse(null, "Customer registered successfully.");
            return Ok(response);
        }

        [HttpPost("register-business")]
        public async Task<IActionResult> RegisterBusiness([FromBody] BusinessRegisterRequest request)
        {
            await _authService.RegisterBusinessAsync(request);
            var response = ApiResponse<string>.SuccessResponse(null, "Business registered successfully.");
            return Ok(response);
        }
    }
}
