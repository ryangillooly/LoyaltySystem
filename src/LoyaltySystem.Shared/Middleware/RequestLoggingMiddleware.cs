using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            
            // Add correlation ID to the response headers
            context.Response.Headers.Add("X-Request-ID", requestId);
            
            // Log request details
            _logger.LogInformation(
                "Request {RequestId} - {Method} {Path} started at {Time}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            try
            {
                await _next(context);
                stopwatch.Stop();
                
                // Log response details
                _logger.LogInformation(
                    "Request {RequestId} - {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log any unhandled exceptions
                _logger.LogError(
                    ex,
                    "Request {RequestId} - {Method} {Path} failed with exception after {ElapsedMs}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
} 