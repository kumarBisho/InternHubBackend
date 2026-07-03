using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InternMS.Api.Services
{
    /// <summary>
    /// Custom SignalR UserIdProvider that extracts user ID from JWT claims
    /// Maps the "id" claim from our JWT tokens to SignalR's user identification
    /// </summary>
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
            // Try to get the "id" claim (custom JWT claim)
            var userId = connection.User?.FindFirstValue("id");
            
            // Fallback to standard NameIdentifier claim if "id" is not present
            if (string.IsNullOrEmpty(userId))
            {
                userId = connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
            return userId;
        }
    }
}
