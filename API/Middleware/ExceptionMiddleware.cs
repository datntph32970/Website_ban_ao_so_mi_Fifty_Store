using API.Exceptions;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new { error = new { message = exception.Message } };

            if (exception is AppException appException)
            {
                context.Response.StatusCode = appException.StatusCode;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new { error = new { message = "Đã xảy ra lỗi. Vui lòng thử lại sau." } };
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
