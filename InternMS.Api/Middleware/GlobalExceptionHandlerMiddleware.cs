using InternMS.Api.DTOs.Common;
using InternMS.Api.Exceptions;
using System.Net;
using System.Text.Json;

namespace InternMS.Api.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse();
        
        // Handle specific exception types
        switch (exception)
        {
            case BusinessException businessEx:
                context.Response.StatusCode = businessEx.StatusCode;
                response = ApiResponse.Fail(
                    businessEx.Message,
                    businessEx.Code,
                    businessEx.StatusCode
                );
                break;

            case ArgumentNullException argNullEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.Fail(
                    $"Required argument missing: {argNullEx.ParamName}",
                    "INVALID_ARGUMENT",
                    400
                );
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.Fail(
                    $"Invalid argument: {argEx.Message}",
                    "INVALID_ARGUMENT",
                    400
                );
                break;

            case System.InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.Fail(
                    invalidOpEx.Message,
                    "INVALID_OPERATION",
                    400
                );
                break;

            case FluentValidation.ValidationException fluentEx:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                var errors = fluentEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.ErrorMessage).ToArray()
                    );
                response = ApiResponse.Fail(
                    "Validation failed",
                    "VALIDATION_ERROR",
                    422
                );
                response.Error.ValidationErrors = errors;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = ApiResponse.Fail(
                    "An internal server error occurred",
                    "INTERNAL_SERVER_ERROR",
                    500
                );
                break;
        }

        response.Error.TraceId = context.TraceIdentifier;
        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension methods for middleware registration
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
