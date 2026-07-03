using System;

namespace InternMS.Domain.Entities
{
    public enum NotificationType
    {
        TaskAssigned = 1,
        TaskUpdated = 2,
        TaskCompleted = 3,
        ProjectCreated = 4,
        ProjectUpdated = 5,
        ProjectAssignment = 6,
        CommentAdded = 7,
        UserMentioned = 8,
        CollaborationInvite = 9,
        DeadlineReminder = 10,
        StatusChanged = 11,
        PriorityChanged = 12,
        GeneralNotification = 99
    }

    public class Notification
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Notification metadata
        public NotificationType Type { get; set; } = NotificationType.GeneralNotification;
        public Guid? TriggeredByUserId { get; set; } // Who caused the notification
        public Guid? RelatedEntityId { get; set; } // Task/Project/etc ID
        public string? RelatedEntityType { get; set; } // "Task", "Project", "Comment"
        public string? ActionUrl { get; set; } // URL to navigate to on click
        
        // Read status
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Additional fields for enhanced functionality
        public int? PriorityLevel { get; set; } = 3; // 1=low, 5=critical
        public bool IsDeleted { get; set; } = false; // Soft delete support
        public DateTime? DeletedAt { get; set; }
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public bool IsEmailSent { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
        public string? Category { get; set; } // e.g., "Task", "Project", "System" for grouping
        public int? BroadcastGroupId { get; set; } // For grouping similar notifications
        public bool IsSystemNotification { get; set; } = false; // System-generated vs user-generated
    }
}