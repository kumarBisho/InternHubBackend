using InternMS.Api.DTOs.Collaboration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternMS.Api.Services.Collaboration
{
    public interface ICollaborationService
    {
        // Activity Management
        Task<ActivityLogDto> LogActivityAsync(ActivityLogDto activityLog);
        Task<PaginatedActivityLogDto> GetActivitiesAsync(ActivityLogFilterDto filter);
        Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 20);
        Task<List<ActivityLogDto>> GetResourceActivitiesAsync(string resourceType, string resourceId);

        // Comments Management
        Task<CollaborativeCommentDto> AddCommentAsync(string userId, CreateCommentDto comment);
        Task<CollaborativeCommentDto> UpdateCommentAsync(int commentId, UpdateCommentDto updates);
        Task DeleteCommentAsync(int commentId);
        Task<List<CollaborativeCommentDto>> GetCommentsAsync(string resourceType, int resourceId);

        // Presence Management
        Task<OnlineUsersDto> GetOnlineUsersAsync();
        Task<PresenceStatusDto> UpdateUserPresenceAsync(string userId, PresenceStatusDto presence);

        // Collaboration Metrics
        Task<CollaborationMetricsDto> GetCollaborationMetricsAsync();
    }
}
