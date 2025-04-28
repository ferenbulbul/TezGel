using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using TezGel.Application.DTOs.Auth.Comman;


namespace TezGel.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // normal istek akışı
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // hata varsa yakala
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var responseModel = ApiResponse<string>.FailResponse(exception.Message);

            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            // Burada exception türüne göre statusCode değiştirebilirsin
            if (exception is ArgumentException) 
                statusCode = HttpStatusCode.BadRequest;
            if (exception is InvalidOperationException)
                statusCode = HttpStatusCode.BadRequest;
            // FluentValidation veya kendi özel hata türlerin olursa buraya ekleriz.

            context.Response.StatusCode = (int)statusCode;

            var responseJson = JsonSerializer.Serialize(responseModel);
            await context.Response.WriteAsync(responseJson);
        }
    }
}
