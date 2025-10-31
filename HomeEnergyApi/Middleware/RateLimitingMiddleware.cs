using Microsoft.AspNetCore.Localization;
using HomeEnergyApi.Services;

namespace HomeEnergyApi.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitingService _rateLimitingService;
        private readonly ILogger<RateLimitingMiddleware> logger;


        public RateLimitingMiddleware(RequestDelegate next, RateLimitingService rateLimitingService, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _rateLimitingService = rateLimitingService;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            logger.LogDebug("RateLimitingMiddleware Started");
            var clientKey = context.Connection.RemoteIpAddress?.ToString();

            if (!_rateLimitingService.IsRequestAllowed(clientKey))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Slow down! Too many requests.");
                return;
            }
            await _next(context);
        }
    }
}