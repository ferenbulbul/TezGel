using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.Expection;


namespace TezGel.API.Middlewares
{

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BaseException ex) // Bizim özel exception'larımız
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex.StatusCode;

                var response = ApiResponse<string>.FailResponse(ex.Message, ex.StatusCode);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (FluentValidation.ValidationException validationException) // FluentValidation Exception'u
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;

                var validationErrors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<string>.FailResponse("Doğrulama hataları oluştu.", 400, validationErrors);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex) // Beklenmeyen tüm diğer hatalar
            {
                _logger.LogError(ex, "Unhandled exception occured");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var response = ApiResponse<string>.ExceptionResponse($"Sunucu hatası oluştu.{ex.Message}");
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
