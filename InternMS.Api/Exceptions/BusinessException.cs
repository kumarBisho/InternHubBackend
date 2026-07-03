namespace InternMS.Api.Exceptions;

/// <summary>
/// Base exception for business logic errors
/// </summary>
public class BusinessException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public BusinessException(string message, string code = "BUSINESS_ERROR", int statusCode = 400)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when requested resource is not found
/// </summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND", 404) { }
}

/// <summary>
/// Thrown when unauthorized access is attempted
/// </summary>
public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED", 401) { }
}

/// <summary>
/// Thrown when a resource already exists
/// </summary>
public class DuplicateException : BusinessException
{
    public DuplicateException(string message)
        : base(message, "DUPLICATE", 409) { }
}

/// <summary>
/// Thrown when validation fails
/// </summary>
public class ValidationException : BusinessException
{
    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", 422) { }
}

/// <summary>
/// Thrown when operation cannot be performed due to business rules
/// </summary>
public class InvalidOperationException : BusinessException
{
    public InvalidOperationException(string message)
        : base(message, "INVALID_OPERATION", 400) { }
}

/// <summary>
/// Thrown when database operation fails
/// </summary>
public class DatabaseException : BusinessException
{
    public DatabaseException(string message, Exception innerException)
        : base(message, "DATABASE_ERROR", 500)
    {
    }
}

/// <summary>
/// Thrown when account verification is required
/// </summary>
public class AccountNotVerifiedException : BusinessException
{
    public AccountNotVerifiedException(string message = "Account not verified")
        : base(message, "ACCOUNT_NOT_VERIFIED", 403) { }
}

/// <summary>
/// Thrown when credentials are invalid
/// </summary>
public class InvalidCredentialsException : BusinessException
{
    public InvalidCredentialsException(string message = "Invalid credentials")
        : base(message, "INVALID_CREDENTIALS", 401) { }
}
