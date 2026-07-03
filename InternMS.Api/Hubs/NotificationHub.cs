using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;

namespace InternMS.Api.Hubs
{
    [Authorize] // require authenticated users
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Get user ID from JWT claim (use "id" custom claim, fallback to NameIdentifier)
            var userId = Context.User?.FindFirst("id")?.Value ??
                         Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to a group for targeted messaging (backup routing method)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} connected to NotificationHub. Connection ID: {Context.ConnectionId}");
            }
            else
            {
                _logger.LogWarning($"Could not extract user ID from claims. Anonymous connection: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _logger.LogInformation($"User {userId} disconnected from NotificationHub. Connection ID: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Sends a notification to a specific user
        /// </summary>
        public async Task SendNotificationAsync(object notification)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", notification);
            }
        }

        /// <summary>
        /// Broadcasts a notification to multiple users
        /// </summary>
        public async Task BroadcastNotificationAsync(IEnumerable<string> userIds, object notification)
        {
            foreach (var userId in userIds)
            {
                await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", notification);
            }
        }

        /// <summary>
        /// Ping-pong for connection health check
        /// </summary>
        public Task Ping() => Clients.Caller.SendAsync("Pong");
    }
}