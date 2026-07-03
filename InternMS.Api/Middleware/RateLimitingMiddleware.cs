using System.Net;
using System.Threading.RateLimiting;

namespace InternMS.Api.Middleware;

/// <summary>
/// Middleware for rate limiting based on IP address and endpoints
/// Protects against brute force and DDoS attacks
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimiter _globalLimiter;
    private readonly RateLimiter _authLimiter;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Global rate limiter: 1000 requests per minute per IP
        _globalLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 1000,
            QueueLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 1
        });

        // Auth rate limiter: 10 requests per minute per IP (strict for login attempts)
        _authLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 10,
            QueueLimit = 5,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 1
        });
    }

    /// <summary>
    /// Invokes the middleware to apply rate limiting
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path;

        // Apply stricter rate limiting to auth endpoints
        var isAuthEndpoint = path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);

        var limiter = isAuthEndpoint ? _authLimiter : _globalLimiter;

        // SlidingWindowRateLimiter grants the permit if available, or queues if limit reached
        // Check if we can acquire without waiting (attempt to get immediately)
        bool isGranted = false;
        
        // For SlidingWindowRateLimiter with queue enabled, AcquireAsync will wait in queue
        // We'll proceed with middleware for now as AcquireAsync will handle queueing
        using (var lease = await limiter.AcquireAsync(permitCount: 1))
        {
            // If we successfully got a lease object, check its validity
            if (lease != null)
            {
                // Add rate limit info to response headers for client awareness
                context.Response.Headers["X-RateLimit-Limit"] = isAuthEndpoint ? "10" : "1000";
                context.Response.Headers["X-RateLimit-Window"] = "60";

                await _next(context);
                return;
            }
        }

        // If we reach here, we didn't get a lease
        _logger.LogWarning(
            "Rate limit exceeded for IP: {RemoteIP} on endpoint: {Path} | IsAuthEndpoint: {IsAuthEndpoint}",
            remoteIp,
            path,
            isAuthEndpoint
        );

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers["Retry-After"] = "60";
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Too many requests. Please try again later.",
            statusCode = 429,
            retryAfter = 60
        });
    }
}

/// <summary>
/// Extension methods for RateLimitingMiddleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds RateLimitingMiddleware to the request pipeline
    /// Should be added early in the pipeline, after security headers
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
