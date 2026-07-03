using Microsoft.AspNetCore.Mvc;
using InternMS.Api.Utils;

namespace InternMS.Api.Controllers
{
    /// <summary>
    /// Base controller class for all API controllers
    /// Provides common helper methods to reduce code duplication
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ILogger<BaseApiController> Logger { get; set; }

        /// <summary>
        /// Gets current user ID from JWT claims
        /// </summary>
        protected Guid GetUserId() => ControllerHelper.GetUserId(User);

        /// <summary>
        /// Safely gets user ID, returns null if extraction fails
        /// </summary>
        protected Guid? TryGetUserId() => ControllerHelper.TryGetUserId(User);

        /// <summary>
        /// Gets current user role from JWT claims
        /// </summary>
        protected string GetUserRole() => ControllerHelper.GetUserRole(User);

        /// <summary>
        /// Gets current user email from JWT claims
        /// </summary>
        protected string GetUserEmail() => ControllerHelper.GetUserEmail(User);

        /// <summary>
        /// Gets current user full name from JWT claims with intelligent fallback
        /// </summary>
        protected string GetUserName() => ControllerHelper.GetUserName(User);

        /// <summary>
        /// Gets current user first name from JWT claims
        /// </summary>
        protected string GetFirstName() => ControllerHelper.GetFirstName(User);

        /// <summary>
        /// Gets current user last name from JWT claims
        /// </summary>
        protected string GetLastName() => ControllerHelper.GetLastName(User);

        /// <summary>
        /// Creates a NOT FOUND response for null/missing items
        /// </summary>
        protected IActionResult NotFound(string itemName, Guid? id = null)
            => ControllerHelper.NotFoundResponse(itemName, id);

        /// <summary>
        /// Creates a NOT FOUND response for null/missing items with string identifier
        /// </summary>
        protected IActionResult NotFound(string itemName, string identifier)
            => ControllerHelper.NotFoundResponse(itemName, identifier);

        /// <summary>
        /// Creates a BAD REQUEST response for validation errors
        /// </summary>
        protected IActionResult BadRequest(string message, string details = null)
            => ControllerHelper.BadRequestResponse(message, details);

        /// <summary>
        /// Creates an UNAUTHORIZED response
        /// </summary>
        protected IActionResult Unauthorized(string message = "Unauthorized access.")
            => ControllerHelper.UnauthorizedResponse(message);

        /// <summary>
        /// Creates a FORBIDDEN response
        /// </summary>
        protected IActionResult Forbidden(string message = "Access forbidden.")
            => ControllerHelper.ForbiddenResponse(message);

        /// <summary>
        /// Creates a SERVER ERROR response with optional details
        /// </summary>
        protected IActionResult ServerError(string message, string details = null)
            => ControllerHelper.ServerErrorResponse(message, details);

        /// <summary>
        /// Validates that an object is not null
        /// Returns (true, null) if valid, (false, errorResponse) if invalid
        /// </summary>
        protected (bool IsValid, IActionResult ErrorResponse) ValidateNotNull<T>(
            T item,
            string itemName,
            Guid? id = null)
            => ControllerHelper.ValidateNotNull(item, itemName, id);

        /// <summary>
        /// Validates that a string is not null or empty
        /// Returns (true, null) if valid, (false, errorResponse) if invalid
        /// </summary>
        protected (bool IsValid, IActionResult ErrorResponse) ValidateNotEmpty(string value, string fieldName)
            => ControllerHelper.ValidateNotEmpty(value, fieldName);

        /// <summary>
        /// Validates that a Guid is not empty
        /// Returns (true, null) if valid, (false, errorResponse) if invalid
        /// </summary>
        protected (bool IsValid, IActionResult ErrorResponse) ValidateNotEmptyGuid(Guid value, string fieldName)
            => ControllerHelper.ValidateNotEmptyGuid(value, fieldName);

        /// <summary>
        /// Validates that user has required role
        /// Returns (true, null) if valid, (false, errorResponse) if invalid
        /// </summary>
        protected (bool IsValid, IActionResult ErrorResponse) ValidateUserRole(params string[] requiredRoles)
            => ControllerHelper.ValidateUserRole(User, requiredRoles);

        /// <summary>
        /// Validates that requesting user only accesses their own data
        /// Returns (true, null) if valid, (false, errorResponse) if invalid
        /// </summary>
        protected (bool IsValid, IActionResult ErrorResponse) ValidateUserOwnership(Guid requestingUserId, Guid targetUserId)
            => ControllerHelper.ValidateUserOwnership(requestingUserId, targetUserId);

        /// <summary>
        /// Safe try-catch wrapper for async controller actions
        /// Handles common errors and returns appropriate responses
        /// </summary>
        protected async Task<IActionResult> SafeExecute(
            Func<Task<IActionResult>> action,
            string operationName = "Operation")
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger?.LogWarning($"Unauthorized: {operationName} - {ex.Message}");
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger?.LogWarning($"Invalid operation: {operationName} - {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Logger?.LogWarning($"Invalid argument: {operationName} - {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Error during {operationName}: {ex.Message}");
                return ServerError("An unexpected error occurred.", ex.Message);
            }
        }

        /// <summary>
        /// Safe try-catch wrapper for async controller actions with return value
        /// </summary>
        protected async Task<IActionResult> SafeExecute<T>(
            Func<Task<T>> action,
            Func<T, IActionResult> successHandler,
            string operationName = "Operation")
        {
            try
            {
                var result = await action();
                return successHandler(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger?.LogWarning($"Unauthorized: {operationName} - {ex.Message}");
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger?.LogWarning($"Invalid operation: {operationName} - {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Logger?.LogWarning($"Invalid argument: {operationName} - {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Error during {operationName}: {ex.Message}");
                return ServerError("An unexpected error occurred.", ex.Message);
            }
        }
    }
}
