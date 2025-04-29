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
        private readonly IMailService _mailService;

        public AuthController(IAuthService authService, IMailService mailService)
        {
            _authService = authService;
            _mailService = mailService;
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


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            var (accessToken, refreshToken, emailConfirmed) = await _authService.LoginAsync(request.Username, request.Password);


            var response = ApiResponse<object>.SuccessResponse(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                EmailConfirmed = emailConfirmed
            }, "Giriş başarılı.", 200);

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
                var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);
                var response = new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Token refreshed successfully."));
            
        }

        [HttpPost("send-test-mail")]
        public async Task<IActionResult> SendTestMail()
        {
            await _mailService.SendEmailAsync("ferenbulbul@gmail.com", "Test Başlık", "Bu bir test epostasıdır.");
            return Ok("Test mail gönderildi.");
        }

        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailCodeRequest request)
        {
            try
            {
                await _authService.VerifyEmailCodeAsync(request.Email, request.Code);
                return Ok(ApiResponse<string>.SuccessResponse(null, "E-posta doğrulandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPost("create-email-code")]
        public async Task<IActionResult> CreateEmailCode([FromQuery] string email)
        {

            await _authService.CreateEmailCodeAsync(email);
            return Ok(ApiResponse<string>.SuccessResponse(null, "E-posta kodu gönderildi."));
        }
    }
}
