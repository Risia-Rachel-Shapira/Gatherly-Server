
namespace Gatherly.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId =
                context.Request.Headers[HeaderName].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId) ||
    correlationId.Length > 100)
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.TraceIdentifier = correlationId;

            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[HeaderName] = correlationId;
                    return Task.CompletedTask;
                });

                _logger.LogInformation(
                    "Incoming request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);
            }
        }
    }
}
