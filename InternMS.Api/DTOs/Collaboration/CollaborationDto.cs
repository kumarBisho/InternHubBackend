namespace InternMS.Api.DTOs.Collaboration
{
    public class CollaborativeCommentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Content { get; set; }
        public string ResourceType { get; set; } // "Task", "Project"
        public int ResourceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentReplyDto> Replies { get; set; }
        public int ReplyCount { get; set; }
    }

    public class CommentReplyDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public string Content { get; set; }
        public string ResourceType { get; set; } // "Task", "Project"
        public int ResourceId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string Content { get; set; }
    }

    public class PresenceStatusDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastActiveAt { get; set; }
        public string CurrentPage { get; set; } = string.Empty; // Which page they're viewing
        public int? CurrentResourceId { get; set; } // If viewing a specific resource
    }

    public class CollaborationEventDto
    {
        public string EventType { get; set; } = string.Empty; // "UserJoined", "UserLeft", "CommentAdded", "TaskUpdated", "PresenceUpdate"
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public int? ResourceId { get; set; }
        public object EventData { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class OnlineUsersDto
    {
        public List<PresenceStatusDto> OnlineUsers { get; set; }
        public int TotalOnlineCount { get; set; }
    }

    public class CollaborationMetricsDto
    {
        public int OnlineUsersCount { get; set; }
        public int ActiveTasksCount { get; set; }
        public int TodaysActivityCount { get; set; }
        public int commentsCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
