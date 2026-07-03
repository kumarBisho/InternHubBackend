using InternMS.Api.DTOs.Collaboration;
using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternMS.Api.Services.Collaboration
{
    public class CollaborationService : ICollaborationService
    {
        private readonly AppDbContext _context;
        private static Dictionary<string, PresenceStatusDto> _onlineUsers = new();
        private static int _commentIdCounter = 1;

        public CollaborationService(AppDbContext context)
        {
            _context = context;
        }

        // Activity Management - Now using Database Persistence
        public async Task<ActivityLogDto> LogActivityAsync(ActivityLogDto activityLog)
        {
            try
            {
                if (activityLog == null)
                    throw new ArgumentNullException(nameof(activityLog));

                activityLog.Timestamp = DateTime.UtcNow;

                // Create database entity from DTO
                var dbActivity = new ActivityLog
                {
                    UserId = activityLog.UserId,
                    UserName = activityLog.UserName ?? "Unknown User",
                    UserEmail = activityLog.UserEmail,
                    ActionType = activityLog.ActionType,
                    ResourceType = activityLog.ResourceType,
                    ResourceId = activityLog.ResourceId,
                    ResourceName = activityLog.ResourceName,
                    Description = activityLog.Description,
                    ChangeDetails = activityLog.ChangeDetails,
                    Timestamp = activityLog.Timestamp
                };

                _context.ActivityLogs.Add(dbActivity);
                await _context.SaveChangesAsync();

                // Return DTO with database-generated ID
                activityLog.Id = dbActivity.Id;
                return activityLog;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error logging activity: {ex.Message}", ex);
            }
        }

        public async Task<PaginatedActivityLogDto> GetActivitiesAsync(ActivityLogFilterDto filter)
        {
            try
            {
                var query = _context.ActivityLogs.AsQueryable();

                if (filter.UserId != Guid.Empty)
                    query = query.Where(a => a.UserId == filter.UserId);

                if (!string.IsNullOrEmpty(filter.ResourceType))
                    query = query.Where(a => a.ResourceType == filter.ResourceType);

                if (!string.IsNullOrEmpty(filter.ResourceId))
                    query = query.Where(a => a.ResourceId == filter.ResourceId);

                if (!string.IsNullOrEmpty(filter.ActionType))
                    query = query.Where(a => a.ActionType == filter.ActionType);

                if (filter.FromDate.HasValue)
                    query = query.Where(a => a.Timestamp >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(a => a.Timestamp <= filter.ToDate.Value);

                var totalCount = await query.CountAsync();

                var activities = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        UserEmail = a.UserEmail,
                        ActionType = a.ActionType,
                        ResourceType = a.ResourceType,
                        ResourceId = a.ResourceId,
                        ResourceName = a.ResourceName,
                        Description = a.Description,
                        ChangeDetails = a.ChangeDetails,
                        Timestamp = a.Timestamp
                    })
                    .ToListAsync();

                return new PaginatedActivityLogDto
                {
                    Activities = activities,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    HasNextPage = (filter.PageNumber * filter.PageSize) < totalCount,
                    HasPreviousPage = filter.PageNumber > 1
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting activities: {ex.Message}", ex);
            }
        }

        public async Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 20)
        {
            try
            {
                return await _context.ActivityLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        UserEmail = a.UserEmail,
                        ActionType = a.ActionType,
                        ResourceType = a.ResourceType,
                        ResourceId = a.ResourceId,
                        ResourceName = a.ResourceName,
                        Description = a.Description,
                        ChangeDetails = a.ChangeDetails,
                        Timestamp = a.Timestamp
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting recent activities: {ex.Message}", ex);
            }
        }

        public async Task<List<ActivityLogDto>> GetResourceActivitiesAsync(string resourceType, string resourceId)
        {
            try
            {
                return await _context.ActivityLogs
                    .Where(a => a.ResourceType == resourceType && a.ResourceId == resourceId)
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        UserEmail = a.UserEmail,
                        ActionType = a.ActionType,
                        ResourceType = a.ResourceType,
                        ResourceId = a.ResourceId,
                        ResourceName = a.ResourceName,
                        Description = a.Description,
                        ChangeDetails = a.ChangeDetails,
                        Timestamp = a.Timestamp
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting resource activities: {ex.Message}", ex);
            }
        }

        // Comments Management - Now using Database Persistence
        public async Task<CollaborativeCommentDto> AddCommentAsync(string userId, CreateCommentDto comment)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                    throw new UnauthorizedAccessException("Invalid user ID");

                var user = await _context.Users.FindAsync(userGuid);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found");

                // Create database entity
                var dbComment = new CollaborativeComment
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    UserEmail = user.Email,
                    Content = comment.Content,
                    ResourceType = comment.ResourceType,
                    ResourceId = comment.ResourceId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CollaborativeComments.Add(dbComment);
                await _context.SaveChangesAsync();

                // Convert to DTO
                var commentDto = new CollaborativeCommentDto
                {
                    Id = dbComment.Id,
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    UserEmail = user.Email,
                    Content = dbComment.Content,
                    ResourceType = dbComment.ResourceType,
                    ResourceId = dbComment.ResourceId,
                    CreatedAt = dbComment.CreatedAt,
                    Replies = new List<CommentReplyDto>(),
                    ReplyCount = 0
                };

                // Log activity for the comment
                var activity = new ActivityLogDto
                {
                    UserId = Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : Guid.Empty,
                    UserName = commentDto.UserName,
                    UserEmail = user.Email,
                    ActionType = "Commented",
                    ResourceType = comment.ResourceType,
                    ResourceId = comment.ResourceId.ToString(),  // Convert int to string
                    ResourceName = $"{comment.ResourceType} {comment.ResourceId}",
                    Description = $"Commented: {comment.Content.Substring(0, Math.Min(50, comment.Content.Length))}",
                    Timestamp = DateTime.UtcNow
                };

                await LogActivityAsync(activity);

                return commentDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding comment: {ex.Message}", ex);
            }
        }

        public async Task<CollaborativeCommentDto> UpdateCommentAsync(int commentId, UpdateCommentDto updates)
        {
            try
            {
                var dbComment = await _context.CollaborativeComments.FindAsync(commentId);
                if (dbComment == null)
                    throw new Exception("Comment not found");

                dbComment.Content = updates.Content;
                dbComment.UpdatedAt = DateTime.UtcNow;

                _context.CollaborativeComments.Update(dbComment);
                await _context.SaveChangesAsync();

                return new CollaborativeCommentDto
                {
                    Id = dbComment.Id,
                    UserId = dbComment.UserId,
                    UserName = dbComment.UserName,
                    UserEmail = dbComment.UserEmail,
                    Content = dbComment.Content,
                    ResourceType = dbComment.ResourceType,
                    ResourceId = dbComment.ResourceId,
                    CreatedAt = dbComment.CreatedAt,
                    UpdatedAt = dbComment.UpdatedAt,
                    Replies = new List<CommentReplyDto>(),
                    ReplyCount = dbComment.Replies.Count
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating comment: {ex.Message}", ex);
            }
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            try
            {
                var dbComment = await _context.CollaborativeComments.FindAsync(commentId);
                if (dbComment != null)
                {
                    dbComment.IsDeleted = true;
                    _context.CollaborativeComments.Update(dbComment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting comment: {ex.Message}", ex);
            }
        }

        public async Task<List<CollaborativeCommentDto>> GetCommentsAsync(string resourceType, int resourceId)
        {
            try
            {
                var dbComments = await _context.CollaborativeComments
                    .Where(c => c.ResourceType == resourceType && 
                               c.ResourceId == resourceId && 
                               !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return dbComments.Select(c => new CollaborativeCommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.UserName,
                    UserEmail = c.UserEmail,
                    Content = c.Content,
                    ResourceType = c.ResourceType,
                    ResourceId = c.ResourceId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Replies = c.Replies
                        .Where(r => !r.IsDeleted)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new CommentReplyDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            UserName = r.UserName,
                            UserEmail = r.UserEmail,
                            Content = r.Content,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt
                        })
                        .ToList(),
                    ReplyCount = c.Replies.Count(r => !r.IsDeleted)
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting comments: {ex.Message}", ex);
            }
        }

        // Presence Management
        public async Task<OnlineUsersDto> GetOnlineUsersAsync()
        {
            try
            {
                var onlineUsers = _onlineUsers.Values.Where(u => u.IsOnline).ToList();

                return await Task.FromResult(new OnlineUsersDto
                {
                    OnlineUsers = onlineUsers,
                    TotalOnlineCount = onlineUsers.Count
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting online users: {ex.Message}", ex);
            }
        }

        public async Task<PresenceStatusDto> UpdateUserPresenceAsync(string userId, PresenceStatusDto presence)
        {
            try
            {
                presence.UserId = userId;
                presence.LastActiveAt = DateTime.UtcNow;
                _onlineUsers[userId] = presence;

                return await Task.FromResult(presence);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating presence: {ex.Message}", ex);
            }
        }

        // Collaboration Metrics
        public async Task<CollaborationMetricsDto> GetCollaborationMetricsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Get today's activity count
                var todaysActivityCount = 0;
                if (_context.ActivityLogs != null)
                {
                    todaysActivityCount = await _context.ActivityLogs
                        .Where(a => a.Timestamp.Date == now.Date)
                        .CountAsync();
                }

                // Get active tasks count
                var activeTasksCount = 0;
                if (_context.ActivityLogs != null)
                {
                    activeTasksCount = await _context.ActivityLogs
                        .Where(a => a.ResourceType == "Task" && a.ActionType == "Updated")
                        .Select(a => a.ResourceId)
                        .Distinct()
                        .CountAsync();
                }

                // Get comments count
                var commentsCount = 0;
                if (_context.CollaborativeComments != null)
                {
                    commentsCount = await _context.CollaborativeComments
                        .Where(c => c != null && !c.IsDeleted)
                        .CountAsync();
                }

                // Get online users count
                var onlineUsersCount = 0;
                if (_onlineUsers != null && _onlineUsers.Values != null)
                {
                    onlineUsersCount = _onlineUsers.Values.Where(u => u != null && u.IsOnline).Count();
                }

                return new CollaborationMetricsDto
                {
                    OnlineUsersCount = onlineUsersCount,
                    ActiveTasksCount = activeTasksCount,
                    TodaysActivityCount = todaysActivityCount,
                    commentsCount = commentsCount,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (NullReferenceException nex)
            {
                // Log the specific null reference for debugging
                System.Diagnostics.Debug.WriteLine($"Null reference in GetCollaborationMetricsAsync: {nex.StackTrace}");
                throw new Exception($"Error getting collaboration metrics: Null reference encountered - {nex.Message}", nex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting collaboration metrics: {ex.Message}", ex);
            }
        }
    }
}
