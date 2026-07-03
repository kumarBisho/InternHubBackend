using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using InternMS.Api.Services;
using InternMS.Api.DTOs.Common;
using InternMS.Api.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using InternMS.Api.Hubs;
using InternMS.Api.Exceptions;

namespace InternMS.Api.Services
{
    /// <summary>
    /// Enhanced notification service with improved error handling, preferences, templates, and logging
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        
        public NotificationService(
            AppDbContext db, 
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _db = db;
            _hubContext = hubContext;
            _logger = logger;
        }

        #region Read Operations

        /// <summary>
        /// Get all notifications for a user (excluding deleted ones)
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            _logger.LogInformation("Fetching all notifications for user {UserId}", userId);
            
            return await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all notifications for a user with pagination
        /// </summary>
        public async Task<PaginatedResponse<Notification>> GetUserNotificationsPaginatedAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var notifications = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            return PaginatedResponse<Notification>.Create(notifications, validatedPageNumber, validatedPageSize, totalCount);
        }

        /// <summary>
        /// Get unread notifications for a user
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUserUnreadNotificationsAsync(Guid userId)
        {
            _logger.LogInformation("Fetching unread notifications for user {UserId}", userId);
            
            return await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get unread notifications for a user with pagination
        /// </summary>
        public async Task<PaginatedResponse<Notification>> GetUserUnreadNotificationsPaginatedAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var notifications = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            return PaginatedResponse<Notification>.Create(notifications, validatedPageNumber, validatedPageSize, totalCount);
        }

        /// <summary>
        /// Get count of unread notifications for a user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }

        /// <summary>
        /// Get notifications filtered by type and date range
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUserNotificationsByTypeAsync(
            Guid userId, 
            NotificationType type,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            _logger.LogInformation("Fetching {Type} notifications for user {UserId}", type, userId);
            
            var query = _db.Notifications
                .Where(n => n.UserId == userId && n.Type == type && !n.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Create notification with minimal parameters
        /// </summary>
        public async Task<Notification> CreateNotificationAsync(Guid userId, string title, string message)
        {
            return await CreateNotificationAsync(
                userId,
                title, 
                message, 
                NotificationType.GeneralNotification
            );
        }

        /// <summary>
        /// Create notification with full metadata and preference checking
        /// </summary>
        public async Task<Notification> CreateNotificationAsync(
            Guid userId,
            string title,
            string message,
            NotificationType type,
            Guid? triggeredByUserId = null,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            string? actionUrl = null,
            string? description = null,
            int? priorityLevel = null,
            string? category = null)
        {
            // Validate inputs
            ValidateNotificationInput(userId, title, message);

            // Convert empty Guids to null
            var validTriggeredByUserId = triggeredByUserId == Guid.Empty ? null : triggeredByUserId;
            var validRelatedEntityId = relatedEntityId == Guid.Empty ? null : relatedEntityId;

            // Check user preferences
            var preferences = await GetOrCreateUserPreferencesAsync(userId);
            if (!preferences.IsNotificationTypeEnabled(type))
            {
                _logger.LogInformation("Notification type {Type} disabled for user {UserId}", type, userId);
                return new Notification(); // Return empty notification, not created
            }

            // Create the notification
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Description = description,
                Type = type,
                TriggeredByUserId = validTriggeredByUserId,
                RelatedEntityId = validRelatedEntityId,
                RelatedEntityType = relatedEntityType,
                ActionUrl = actionUrl,
                Category = category ?? GetCategoryForType(type),
                PriorityLevel = priorityLevel ?? GetDefaultPriorityForType(type),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            try
            {
                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Notification created: {NotificationId} for user {UserId}", 
                    notification.Id, userId);

                // Check quiet hours before broadcasting
                if (!preferences.IsInQuietHours())
                {
                    // Broadcast to connected client in real-time
                    await BroadcastNotificationAsync(userId, notification);
                }
                else
                {
                    _logger.LogInformation("Notification in quiet hours - not broadcasting to user {UserId}", userId);
                }

                return notification;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating notification for user {UserId}", userId);
                throw new BusinessException("Failed to create notification", "CREATE_ERROR", 500);
            }
        }

        /// <summary>
        /// Create notifications for multiple users (batch operation)
        /// </summary>
        public async Task<IEnumerable<Notification>> CreateBulkNotificationsAsync(
            IEnumerable<Guid> userIds,
            string title,
            string message,
            NotificationType type,
            Guid? triggeredByUserId = null,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            string? actionUrl = null,
            string? description = null)
        {
            _logger.LogInformation("Creating bulk notifications for {Count} users", 
                userIds.Count());

            var notifications = new List<Notification>();
            foreach (var userId in userIds)
            {
                try
                {
                    var notification = await CreateNotificationAsync(
                        userId, title, message, type,
                        triggeredByUserId, relatedEntityId, relatedEntityType, actionUrl, description);
                    notifications.Add(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
                    // Continue with remaining users instead of failing entire batch
                }
            }

            return notifications;
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Mark a single notification as read
        /// </summary>
        public async Task MarkAsReadAsync(int notificationId, Guid userId)
        {
            _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", 
                notificationId, userId);

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
                throw new NotFoundException("Notification not found");

            if (notification.IsRead)
                return; // Already read, no need to update

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw new BusinessException("Failed to mark notification as read", "UPDATE_ERROR", 500);
            }
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

            var unreadNotifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return;

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", 
                    unreadNotifications.Count, userId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw new BusinessException("Failed to mark notifications as read", "UPDATE_ERROR", 500);
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft delete a notification
        /// </summary>
        public async Task DeleteNotificationAsync(int notificationId, Guid userId)
        {
            _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", 
                notificationId, userId);

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                throw new NotFoundException("Notification not found");

            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw new BusinessException("Failed to delete notification", "DELETE_ERROR", 500);
            }
        }

        /// <summary>
        /// Delete all read notifications (soft delete with cascade mark)
        /// </summary>
        public async Task ClearReadNotificationsAsync(Guid userId)
        {
            _logger.LogInformation("Clearing all read notifications for user {UserId}", userId);

            var readNotifications = await _db.Notifications
                .Where(n => n.UserId == userId && n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (!readNotifications.Any())
                return;

            foreach (var notification in readNotifications)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = DateTime.UtcNow;
            }

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Cleared {Count} read notifications for user {UserId}", 
                    readNotifications.Count, userId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error clearing read notifications");
                throw new BusinessException("Failed to clear read notifications", "DELETE_ERROR", 500);
            }
        }

        /// <summary>
        /// Permanently delete old notifications (for cleanup job)
        /// </summary>
        public async Task DeleteOldNotificationsAsync(int olderThanDays = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            _logger.LogInformation("Deleting notifications older than {Days} days (before {Date})", 
                olderThanDays, cutoffDate);

            var oldNotifications = await _db.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsDeleted)
                .ToListAsync();

            if (!oldNotifications.Any())
            {
                _logger.LogInformation("No old notifications to delete");
                return;
            }

            try
            {
                _db.Notifications.RemoveRange(oldNotifications);
                var deleted = await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old notifications", deleted);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting old notifications");
                throw new BusinessException("Failed to delete old notifications", "DELETE_ERROR", 500);
            }
        }

        #endregion

        #region Broadcasting

        /// <summary>
        /// Broadcast notification to a specific user via SignalR
        /// </summary>
        public async Task BroadcastNotificationAsync(Guid userId, Notification notification)
        {
            try
            {
                var notificationDto = new
                {
                    notification.Id,
                    notification.UserId,
                    notification.Title,
                    notification.Message,
                    notification.Description,
                    Type = notification.Type.ToString(),
                    notification.ActionUrl,
                    notification.CreatedAt,
                    notification.IsRead,
                    notification.ReadAt,
                    notification.TriggeredByUserId,
                    notification.RelatedEntityId,
                    notification.RelatedEntityType,
                    notification.PriorityLevel,
                    notification.Category
                };

                // Send to specific user using their ID
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveNotification", notificationDto);

                _logger.LogInformation("Notification {NotificationId} broadcast to user {UserId}", 
                    notification.Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification to user {UserId}", userId);
                // Don't throw - notification was saved to DB even if real-time broadcast failed
            }
        }

        /// <summary>
        /// Broadcast notification to multiple users via SignalR
        /// </summary>
        public async Task BroadcastToMultipleUsersAsync(IEnumerable<Guid> userIds, Notification notification)
        {
            _logger.LogInformation("Broadcasting notification to {Count} users", userIds.Count());

            var tasks = userIds.Select(userId => BroadcastNotificationAsync(userId, notification));
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Preferences

        /// <summary>
        /// Get or create user notification preferences
        /// </summary>
        public async Task<NotificationPreference> GetOrCreateUserPreferencesAsync(Guid userId)
        {
            var preferences = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preferences != null)
                return preferences;

            // Create default preferences for new user
            preferences = new NotificationPreference
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _db.NotificationPreferences.Add(preferences);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created default notification preferences for user {UserId}", userId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating notification preferences");
                // Return the in-memory preferences if save fails
            }

            return preferences;
        }

        /// <summary>
        /// Update user notification preferences
        /// </summary>
        public async Task UpdatePreferencesAsync(Guid userId, NotificationPreference preferences)
        {
            _logger.LogInformation("Updating notification preferences for user {UserId}", userId);

            try
            {
                preferences.UpdatedAt = DateTime.UtcNow;
                _db.NotificationPreferences.Update(preferences);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                throw new BusinessException("Failed to update notification preferences", "UPDATE_ERROR", 500);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validate notification input to prevent empty/invalid data
        /// </summary>
        private void ValidateNotificationInput(Guid userId, string title, string message)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("UserId cannot be empty");

            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Title cannot be empty");

            if (string.IsNullOrWhiteSpace(message))
                throw new ValidationException("Message cannot be empty");

            if (title.Length > 200)
                throw new ValidationException("Title cannot exceed 200 characters");

            if (message.Length > 1000)
                throw new ValidationException("Message cannot exceed 1000 characters");
        }

        /// <summary>
        /// Get category for a notification type
        /// </summary>
        private string GetCategoryForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.TaskAssigned => "Task",
                NotificationType.TaskUpdated => "Task",
                NotificationType.TaskCompleted => "Task",
                NotificationType.ProjectCreated => "Project",
                NotificationType.ProjectUpdated => "Project",
                NotificationType.ProjectAssignment => "Project",
                NotificationType.CommentAdded => "Collaboration",
                NotificationType.UserMentioned => "Collaboration",
                NotificationType.CollaborationInvite => "Collaboration",
                NotificationType.DeadlineReminder => "Reminder",
                NotificationType.StatusChanged => "Update",
                NotificationType.PriorityChanged => "Update",
                _ => "General"
            };
        }

        /// <summary>
        /// Get default priority level for a notification type
        /// </summary>
        private int GetDefaultPriorityForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.DeadlineReminder => 5, // Critical
                NotificationType.TaskAssigned => 4,     // High
                NotificationType.ProjectAssignment => 4,
                NotificationType.TaskCompleted => 3,    // Medium
                NotificationType.ProjectCreated => 2,   // Low
                NotificationType.CommentAdded => 2,
                NotificationType.UserMentioned => 4,
                _ => 3                                   // Default: Medium
            };
        }

        #endregion
    }
}