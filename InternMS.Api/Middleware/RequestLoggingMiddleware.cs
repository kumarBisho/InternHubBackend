namespace InternMS.Api.Middleware;

/// <summary>
/// Middleware for structured request/response logging with Serilog
/// Logs HTTP requests and responses with correlation IDs for tracing
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log requests and responses
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID if not present
        var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
            ? context.Request.Headers["X-Correlation-ID"].ToString()
            : Guid.NewGuid().ToString();

        // Add correlation ID to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Log request
        var request = context.Request;
        var requestBody = await ReadRequestBodyAsync(request);

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} | CorrelationId: {CorrelationId} | IP: {RemoteIP}",
            request.Method,
            request.Path,
            correlationId,
            context.Connection.RemoteIpAddress
        );

        if (!string.IsNullOrEmpty(requestBody) && requestBody.Length < 10000)
        {
            _logger.LogDebug("Request Body: {RequestBody}", requestBody);
        }

        // Store original response stream
        var originalResponseBody = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await _next(context);
                stopwatch.Stop();

                // Log response
                var response = context.Response;
                var responseContent = await ReadResponseBodyAsync(response);

                _logger.LogInformation(
                    "HTTP Response: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId
                );

                if (!string.IsNullOrEmpty(responseContent) && responseContent.Length < 10000)
                {
                    _logger.LogDebug("Response Body: {ResponseBody}", responseContent);
                }

                // Copy logged response back to original stream
                context.Response.Body = originalResponseBody;
                await responseBody.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RequestLoggingMiddleware | CorrelationId: {CorrelationId}", correlationId);
                context.Response.Body = originalResponseBody;
                throw;
            }
        }
    }

    /// <summary>
    /// Reads the request body without consuming it
    /// </summary>
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // Ensure the body can be read multiple times
            request.EnableBuffering();

            using (var reader = new StreamReader(request.Body, encoding: System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                return body;
            }
        }
        catch
        {
            return "[Could not read body]";
        }
    }

    /// <summary>
    /// Reads the response body without consuming it
    /// </summary>
    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        try
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(response.Body, encoding: System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                return body;
            }
        }
        catch
        {
            return "[Could not read body]";
        }
    }
}

/// <summary>
/// Extension methods for RequestLoggingMiddleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds RequestLoggingMiddleware to the request pipeline
    /// Should be added after CORS but before other routing middleware
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
