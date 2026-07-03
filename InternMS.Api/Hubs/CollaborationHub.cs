using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using InternMS.Api.Services.Collaboration;
using InternMS.Api.DTOs.Collaboration;
using System.Collections.Concurrent;

namespace InternMS.Api.Hubs
{
    [Authorize]
    public class CollaborationHub : Hub
    {
        private readonly ICollaborationService _collaborationService;
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();

        public CollaborationHub(ICollaborationService collaborationService)
        {
            _collaborationService = collaborationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("user_id")?.Value ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            
            // Try to get full name, or construct from first/last names
            var userName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                var firstName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName)?.Value ?? "";
                var lastName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName)?.Value ?? "";
                userName = $"{firstName} {lastName}".Trim();
            }
            if (string.IsNullOrEmpty(userName))
            {
                userName = userEmail;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.TryAdd(Context.ConnectionId, userId);

                // Notify all clients that a user went online
                var presence = new PresenceStatusDto
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    IsOnline = true,
                    LastActiveAt = DateTime.UtcNow,
                    CurrentPage = "connected"
                };

                // Update presence in service
                await _collaborationService.UpdateUserPresenceAsync(userId, presence);

                await Clients.All.SendAsync("UserJoined", presence);
                await Clients.Caller.SendAsync("ConnectionEstablished", new { connectionId = Context.ConnectionId });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (UserConnections.TryRemove(Context.ConnectionId, out var userId))
            {
                // Update user presence to offline
                if (!string.IsNullOrEmpty(userId))
                {
                    var presence = new PresenceStatusDto
                    {
                        UserId = userId,
                        IsOnline = false,
                        LastActiveAt = DateTime.UtcNow
                    };
                    await _collaborationService.UpdateUserPresenceAsync(userId, presence);
                }

                // Notify all clients that user went offline
                await Clients.All.SendAsync("UserLeft", new { userId = userId });
            }

            await base.OnDisconnectedAsync(exception);
        }

        // User updates their presence (which page they're on)
        public async Task UpdatePresence(PresenceStatusDto presence)
        {
            await Clients.All.SendAsync("PresenceUpdated", presence);
        }

        // Activity logging - broadcast when something happens
        public async Task LogActivity(ActivityLogDto activity)
        {
            try
            {
                var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("user_id")?.Value ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userEmail = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                
                // Extract user name with fallback chain
                var userName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    var firstName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName)?.Value ?? "";
                    var lastName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName)?.Value ?? "";
                    userName = $"{firstName} {lastName}".Trim();
                }
                if (string.IsNullOrEmpty(userName))
                {
                    userName = userEmail;
                }
                if (string.IsNullOrEmpty(userName))
                {
                    userName = "Unknown User";
                }

                // Enrich activity with authenticated user info
                if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
                {
                    activity.UserId = userGuid;
                }
                activity.UserName = userName;
                activity.UserEmail = userEmail;
                activity.Timestamp = DateTime.UtcNow;
                await _collaborationService.LogActivityAsync(activity);

                // Broadcast to all clients
                await Clients.All.SendAsync("ActivityLogged", activity);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        // Add a comment
        public async Task AddComment(CreateCommentDto comment)
        {
            try
            {
                var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User not authenticated");

                var createdComment = await _collaborationService.AddCommentAsync(userId, comment);
                
                // Broadcast comment to all clients interested in this resource
                var resourceGroup = $"{comment.ResourceType}_{comment.ResourceId}";
                await Clients.Group(resourceGroup).SendAsync("CommentAdded", createdComment);
                await Clients.Others.SendAsync("CommentAdded", createdComment);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        // Join a resource group for targeted updates
        public async Task JoinResourceGroup(string resourceType, int resourceId)
        {
            var groupName = $"{resourceType}_{resourceId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var userName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                var firstName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName)?.Value ?? "";
                var lastName = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName)?.Value ?? "";
                userName = $"{firstName} {lastName}".Trim();
            }
            if (string.IsNullOrEmpty(userName))
            {
                userName = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
            }
            
            await Clients.Group(groupName).SendAsync("UserJoinedResource", new
            {
                resourceType = resourceType,
                resourceId = resourceId,
                userName = userName
            });
        }

        // Leave a resource group
        public async Task LeaveResourceGroup(string resourceType, int resourceId)
        {
            var groupName = $"{resourceType}_{resourceId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftResource", new
            {
                resourceType = resourceType,
                resourceId = resourceId
            });
        }

        // Real-time task update notification
        public async Task TaskUpdated(int taskId, object updates)
        {
            var groupName = $"Task_{taskId}";
            await Clients.Group(groupName).SendAsync("TaskUpdated", new
            {
                taskId = taskId,
                updates = updates,
                timestamp = DateTime.UtcNow
            });
        }

        // Real-time project update notification
        public async Task ProjectUpdated(int projectId, object updates)
        {
            var groupName = $"Project_{projectId}";
            await Clients.Group(groupName).SendAsync("ProjectUpdated", new
            {
                projectId = projectId,
                updates = updates,
                timestamp = DateTime.UtcNow
            });
        }

        // Get online users
        public async Task GetOnlineUsers()
        {
            var onlineUsers = await _collaborationService.GetOnlineUsersAsync();
            await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
        }

        // Ping for connection health
        public Task Ping() => Clients.Caller.SendAsync("Pong");
    }
}
