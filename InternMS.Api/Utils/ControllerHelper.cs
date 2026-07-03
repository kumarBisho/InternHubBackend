using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using InternMS.Api.Exceptions;

namespace InternMS.Api.Utils
{
    /// <summary>
    /// Helper class for common controller operations to eliminate code duplication
    /// and provide consistent error handling across all controllers
    /// </summary>
    public static class ControllerHelper
    {
        /// <summary>
        /// Extracts the user ID from JWT claims with validation
        /// </summary>
        public static Guid GetUserId(ClaimsPrincipal user)
        {
            var id = user?.FindFirstValue("id") ?? user?.FindFirstValue("sub");

            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
            {
                throw new UnauthorizedException("Invalid or missing user ID in token.");
            }

            return userId;
        }

        /// <summary>
        /// Safely extracts user ID from claims, returns null if extraction fails
        /// </summary>
        public static Guid? TryGetUserId(ClaimsPrincipal user)
        {
            try
            {
                return GetUserId(user);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts user role from JWT claims with fallback to "Intern"
        /// </summary>
        public static string GetUserRole(ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.Role) ?? "Intern";
        }

        /// <summary>
        /// Extracts user email from JWT claims
        /// </summary>
        public static string GetUserEmail(ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.Email) ?? "unknown@example.com";
        }

        /// <summary>
        /// Extracts user full name from JWT claims with intelligent fallback
        /// Tries: Name claim → (GivenName + FamilyName) → Email → "Unknown User"
        /// </summary>
        public static string GetUserName(ClaimsPrincipal user)
        {
            // Try standard name claim
            var userName = user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name);
            if (!string.IsNullOrEmpty(userName))
                return userName;

            // Try given name + family name
            var firstName = user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName) ?? "";
            var lastName = user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName) ?? "";
            var fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrEmpty(fullName))
                return fullName;

            // Fallback to email
            var email = user?.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
                return email;

            return "Unknown User";
        }

        /// <summary>
        /// Extracts first name from JWT claims
        /// </summary>
        public static string GetFirstName(ClaimsPrincipal user)
        {
            return user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName) ?? "";
        }

        /// <summary>
        /// Extracts last name from JWT claims
        /// </summary>
        public static string GetLastName(ClaimsPrincipal user)
        {
            return user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName) ?? "";
        }

        /// <summary>
        /// Creates a standardized error response for null/not found items
        /// </summary>
        public static IActionResult NotFoundResponse(string itemName, Guid? id = null)
        {
            var message = id.HasValue
                ? $"{itemName} with ID {id} not found."
                : $"{itemName} not found.";

            return new NotFoundObjectResult(new { message });
        }

        /// <summary>
        /// Creates a standardized error response for null/not found items
        /// </summary>
        public static IActionResult NotFoundResponse(string itemName, string identifier)
        {
            return new NotFoundObjectResult(new { message = $"{itemName} '{identifier}' not found." });
        }

        /// <summary>
        /// Creates a standardized bad request response for validation errors
        /// </summary>
        public static IActionResult BadRequestResponse(string message, string details = null)
        {
            if (!string.IsNullOrEmpty(details))
                return new BadRequestObjectResult(new { message, details });
            
            return new BadRequestObjectResult(new { message });
        }

        /// <summary>
        /// Creates a standardized unauthorized response
        /// </summary>
        public static IActionResult UnauthorizedResponse(string message = "Unauthorized access.")
        {
            return new UnauthorizedObjectResult(new { message });
        }

        /// <summary>
        /// Creates a standardized forbidden response
        /// </summary>
        public static IActionResult ForbiddenResponse(string message = "Access forbidden.")
        {
            return new ObjectResult(new { message }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        /// <summary>
        /// Creates a standardized server error response  
        /// </summary>
        public static IActionResult ServerErrorResponse(string message, string details = null)
        {
            if (!string.IsNullOrEmpty(details))
                return new ObjectResult(new { message, details }) { StatusCode = StatusCodes.Status500InternalServerError };
            
            return new ObjectResult(new { message }) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        /// <summary>
        /// Validates that an object is not null, returns appropriate response if null
        /// </summary>
        public static (bool IsValid, IActionResult ErrorResponse) ValidateNotNull<T>(
            T item,
            string itemName,
            Guid? id = null)
        {
            if (item == null)
                return (false, NotFoundResponse(itemName, id));

            return (true, new OkResult());
        }

        /// <summary>
        /// Validates that a string is not null or empty
        /// </summary>
        public static (bool IsValid, IActionResult ErrorResponse) ValidateNotEmpty(
            string value,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, BadRequestResponse($"{fieldName} cannot be empty."));

            return (true, null);
        }

        /// <summary>
        /// Validates that a Guid is not empty
        /// </summary>
        public static (bool IsValid, IActionResult ErrorResponse) ValidateNotEmptyGuid(
            Guid value,
            string fieldName)
        {
            if (value == Guid.Empty)
                return (false, BadRequestResponse($"{fieldName} cannot be empty."));

            return (true, null);
        }

        /// <summary>
        /// Validates that user has required role
        /// </summary>
        public static (bool IsValid, IActionResult ErrorResponse) ValidateUserRole(
            ClaimsPrincipal user,
            params string[] requiredRoles)
        {
            var userRole = GetUserRole(user);
            if (!requiredRoles.Contains(userRole))
                return (false, ForbiddenResponse($"Access denied. Required role(s): {string.Join(", ", requiredRoles)}"));

            return (true, null);
        }

        /// <summary>
        /// Validates that the requesting user matches the target user ID
        /// (for operations that should only work on own data)
        /// </summary>
        public static (bool IsValid, IActionResult ErrorResponse) ValidateUserOwnership(
            Guid requestingUserId,
            Guid targetUserId)
        {
            if (requestingUserId != targetUserId)
                return (false, ForbiddenResponse("You can only access your own data."));

            return (true, null);
        }
    }
}
