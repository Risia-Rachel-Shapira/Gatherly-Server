
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
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
                if (context.Response.HasStarted)
                    throw;

                var statusCode = ex switch
                {
                    UnauthorizedAccessException => 403,
                    KeyNotFoundException => 404,
                    ArgumentException => 400,
                    DbUpdateConcurrencyException => 409,
                    InvalidOperationException => 409,
                    _ => 500
                };

                if (statusCode == 500)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception. CorrelationId: {CorrelationId}",
                        context.TraceIdentifier);
                }
                else if (statusCode == 409)
                {
                    _logger.LogWarning(
                        ex,
                        "Concurrency or conflict error. CorrelationId: {CorrelationId}",
                        context.TraceIdentifier);
                }
                else
                {
                    _logger.LogWarning(
                        ex,
                        "Request rejected with status {StatusCode}. CorrelationId: {CorrelationId}",
                        statusCode,
                        context.TraceIdentifier);
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = statusCode,
                    Message = statusCode == 500
                        ? "An unexpected error occurred."
                        : ex.Message,
                    TraceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
        }
    }
}
