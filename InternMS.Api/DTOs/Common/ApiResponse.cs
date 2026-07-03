namespace InternMS.Api.DTOs.Common;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public ErrorDetails Error { get; set; }
    public DateTime Timestamp { get; set; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> Fail(string message, string code = "ERROR", int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ErrorDetails
            {
                Message = message,
                Code = code,
                StatusCode = statusCode
            },
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> Fail(ErrorDetails error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Standard API response for operations without data return
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ErrorDetails Error { get; set; }
    public DateTime Timestamp { get; set; }

    public static ApiResponse Ok(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Error = null,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse Fail(string message, string code = "ERROR", int statusCode = 400)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails
            {
                Message = message,
                Code = code,
                StatusCode = statusCode
            },
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse Fail(ErrorDetails error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = error.Message,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Error details for standardized error responses
/// </summary>
public class ErrorDetails
{
    public string Message { get; set; }
    public string Code { get; set; }
    public int StatusCode { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; }
    public string TraceId { get; set; }

    public ErrorDetails() { }

    public ErrorDetails(string message, string code = "ERROR", int statusCode = 400)
    {
        Message = message;
        Code = code;
        StatusCode = statusCode;
    }
}
