using InternMS.Api.DTOs;
using InternMS.Api.DTOs.Common;
using InternMS.Domain.Entities;

namespace InternMS.Api.Services
{
    /// <summary>
    /// Enhanced notification service interface with support for preferences, templates, and better error handling
    /// </summary>
    public interface INotificationService
    {
        #region Read Operations
        
        /// <summary>
        /// Get all notifications for a user (excluding deleted)
        /// </summary>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        
        /// <summary>
        /// Get all notifications for a user with pagination
        /// </summary>
        Task<PaginatedResponse<Notification>> GetUserNotificationsPaginatedAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// Get only unread notifications for a user
        /// </summary>
        Task<IEnumerable<Notification>> GetUserUnreadNotificationsAsync(Guid userId);
        
        /// <summary>
        /// Get unread notifications for a user with pagination
        /// </summary>
        Task<PaginatedResponse<Notification>> GetUserUnreadNotificationsPaginatedAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// Get count of unread notifications for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(Guid userId);
        
        /// <summary>
        /// Get notifications filtered by type and optional date range
        /// </summary>
        Task<IEnumerable<Notification>> GetUserNotificationsByTypeAsync(
            Guid userId,
            NotificationType type,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        #endregion

        #region Create Operations
        
        /// <summary>
        /// Create a simple notification with minimal parameters
        /// </summary>
        Task<Notification> CreateNotificationAsync(Guid userId, string title, string message);
        
        /// <summary>
        /// Create a notification with full metadata, templates, and preference checking
        /// </summary>
        Task<Notification> CreateNotificationAsync(
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
            string? category = null);
        
        /// <summary>
        /// Create notifications for multiple users in a single operation (batch)
        /// </summary>
        Task<IEnumerable<Notification>> CreateBulkNotificationsAsync(
            IEnumerable<Guid> userIds,
            string title,
            string message,
            NotificationType type,
            Guid? triggeredByUserId = null,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            string? actionUrl = null,
            string? description = null);

        #endregion

        #region Update Operations
        
        /// <summary>
        /// Mark a single notification as read
        /// </summary>
        Task MarkAsReadAsync(int notificationId, Guid userId);
        
        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        Task MarkAllAsReadAsync(Guid userId);

        #endregion

        #region Delete Operations
        
        /// <summary>
        /// Soft delete a notification
        /// </summary>
        Task DeleteNotificationAsync(int notificationId, Guid userId);
        
        /// <summary>
        /// Soft delete all read notifications for a user
        /// </summary>
        Task ClearReadNotificationsAsync(Guid userId);
        
        /// <summary>
        /// Permanently delete notifications older than specified days (for cleanup jobs)
        /// </summary>
        Task DeleteOldNotificationsAsync(int olderThanDays = 30);

        #endregion

        #region Broadcasting
        
        /// <summary>
        /// Broadcast notification to a specific user via SignalR
        /// </summary>
        Task BroadcastNotificationAsync(Guid userId, Notification notification);
        
        /// <summary>
        /// Broadcast notification to multiple users via SignalR
        /// </summary>
        Task BroadcastToMultipleUsersAsync(IEnumerable<Guid> userIds, Notification notification);

        #endregion

        #region Preferences
        
        /// <summary>
        /// Get or create user notification preferences
        /// </summary>
        Task<NotificationPreference> GetOrCreateUserPreferencesAsync(Guid userId);
        
        /// <summary>
        /// Update user notification preferences
        /// </summary>
        Task UpdatePreferencesAsync(Guid userId, NotificationPreference preferences);

        #endregion
    }
}