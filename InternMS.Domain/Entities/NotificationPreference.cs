using System;

namespace InternMS.Domain.Entities
{
    /// <summary>
    /// Stores user's notification preferences for different notification types.
    /// Allows users to control which notifications they receive and how they want to receive them.
    /// </summary>
    public class NotificationPreference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

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

        // Quiet hours (Don't disturb)
        public TimeSpan? QuietHourStartTime { get; set; } // e.g., 22:00
        public TimeSpan? QuietHourEndTime { get; set; }   // e.g., 08:00
        public bool IsQuietHourEnabled { get; set; } = false;

        // Audit trail
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Check if a specific notification type is enabled
        /// </summary>
        public bool IsNotificationTypeEnabled(NotificationType type)
        {
            return type switch
            {
                NotificationType.TaskAssigned => IsTaskAssignedEnabled,
                NotificationType.TaskUpdated => IsTaskUpdatedEnabled,
                NotificationType.TaskCompleted => IsTaskCompletedEnabled,
                NotificationType.ProjectCreated => IsProjectCreatedEnabled,
                NotificationType.ProjectUpdated => IsProjectUpdatedEnabled,
                NotificationType.ProjectAssignment => IsProjectAssignmentEnabled,
                NotificationType.CommentAdded => IsCommentAddedEnabled,
                NotificationType.UserMentioned => IsUserMentionedEnabled,
                NotificationType.CollaborationInvite => IsCollaborationInviteEnabled,
                NotificationType.DeadlineReminder => IsDeadlineReminderEnabled,
                NotificationType.StatusChanged => IsStatusChangedEnabled,
                NotificationType.PriorityChanged => IsPriorityChangedEnabled,
                _ => true // Default: allow notification
            };
        }

        /// <summary>
        /// Check if we're currently in quiet hours
        /// </summary>
        public bool IsInQuietHours()
        {
            if (!IsQuietHourEnabled || !QuietHourStartTime.HasValue || !QuietHourEndTime.HasValue)
                return false;

            var now = DateTime.Now.TimeOfDay;
            var start = QuietHourStartTime.Value;
            var end = QuietHourEndTime.Value;

            if (start < end)
            {
                // Normal case: quiet hours don't cross midnight
                return now >= start && now <= end;
            }
            else
            {
                // Quiet hours cross midnight (e.g., 22:00 to 08:00)
                return now >= start || now <= end;
            }
        }
    }
}
