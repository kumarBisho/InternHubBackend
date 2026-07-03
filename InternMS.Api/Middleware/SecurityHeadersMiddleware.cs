namespace InternMS.Api.Middleware;

/// <summary>
/// Middleware to add security headers to all HTTP responses
/// Protects against common web vulnerabilities
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to add security headers
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Add security headers
            AddSecurityHeaders(context.Response);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SecurityHeadersMiddleware");
            throw;
        }
    }

    /// <summary>
    /// Adds security-related headers to the response
    /// </summary>
    private static void AddSecurityHeaders(HttpResponse response)
    {
        // Prevent clickjacking attacks - disallow embedding in frames
        response.Headers["X-Frame-Options"] = "DENY";

        // Prevent MIME-type sniffing
        response.Headers["X-Content-Type-Options"] = "nosniff";

        // Enable XSS protection in older browsers
        response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Content Security Policy - strict policy for protecting against XSS
        response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' ws: wss:; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        // Referrer Policy - control how much referrer information is shared
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Feature Policy (now Permissions Policy) - control which browser features can be used
        response.Headers["Permissions-Policy"] = 
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=(), " +
            "usb=(), " +
            "magnetometer=(), " +
            "gyroscope=(), " +
            "accelerometer=()";

        // Strict-Transport-Security (HSTS) - only in production
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))) 
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true)
            {
                // 31536000 seconds = 1 year
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }
        }
    }
}

/// <summary>
/// Extension methods for SecurityHeadersMiddleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds SecurityHeadersMiddleware to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
