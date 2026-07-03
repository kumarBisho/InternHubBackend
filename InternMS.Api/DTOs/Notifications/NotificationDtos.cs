using InternMS.Domain.Entities;

namespace InternMS.Api.DTOs.Notifications
{
    /// <summary>
    /// DTO for creating notifications
    /// </summary>
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Description { get; set; }
        public NotificationType Type { get; set; } = NotificationType.GeneralNotification;
        public Guid? TriggeredByUserId { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ActionUrl { get; set; }
        public int? PriorityLevel { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// DTO for notification preferences
    /// </summary>
    public class NotificationPreferenceDto
    {
        public Guid UserId { get; set; }
        
        // Notification type preferences
        public bool IsTaskAssignedEnabled { get; set; } = true;
        public bool IsTaskUpdatedEnabled { get; set; } = true;
        public bool IsTaskCompletedEnabled { get; set; } = true;
        public bool IsProjectCreatedEnabled { get; set; } = true;
        public bool IsProjectUpdatedEnabled { get; set; } = true;
        public bool IsProjectAssignmentEnabled { get; set; } = true;
        public bool IsCommentAddedEnabled { get; set; } = true;
        public bool IsUserMentionedEnabled { get; set; } = true;
        public bool IsCollaborationInviteEnabled { get; set; } = true;
        public bool IsDeadlineReminderEnabled { get; set; } = true;
        public bool IsStatusChangedEnabled { get; set; } = true;
        public bool IsPriorityChangedEnabled { get; set; } = true;
        
        // Delivery method preferences
        public bool EnableEmailNotifications { get; set; } = false;
        public bool EnableBrowserNotifications { get; set; } = true;
        public bool EnableSoundNotifications { get; set; } = true;
        public bool EnableDailyDigest { get; set; } = false;
        
        // Quiet hours
        public TimeSpan? QuietHourStartTime { get; set; }
        public TimeSpan? QuietHourEndTime { get; set; }
        public bool IsQuietHourEnabled { get; set; } = false;
    }

    /// <summary>
    /// DTO for bulk notification creation
    /// </summary>
    public class CreateBulkNotificationDto
    {
        public IEnumerable<Guid> UserIds { get; set; } = new List<Guid>();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Description { get; set; }
        public NotificationType Type { get; set; } = NotificationType.GeneralNotification;
        public Guid? TriggeredByUserId { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// DTO for notification statistics
    /// </summary>
    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ReadNotifications { get; set; }
        public Dictionary<string, int> CountByType { get; set; } = new();
        public Dictionary<string, int> CountByCategory { get; set; } = new();
        public DateTime LastNotificationDate { get; set; }
    }

    /// <summary>
    /// DTO for notification template
    /// </summary>
    public class NotificationTemplateDto
    {
        public int Id { get; set; }
        public NotificationType NotificationType { get; set; }
        public string TitleTemplate { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public string? DescriptionTemplate { get; set; }
        public string? Parameters { get; set; }
        public bool SendInRealTime { get; set; } = true;
        public bool IncludeInEmailDigest { get; set; } = true;
        public int PriorityLevel { get; set; } = 3;
        public string? ActionUrlTemplate { get; set; }
        public string Language { get; set; } = "en";
        public bool IsActive { get; set; } = true;
    }
}
