using System;

namespace InternMS.Domain.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string ActionType { get; set; } = string.Empty; // Created, Updated, Completed, Assigned, Commented, etc.
        public string ResourceType { get; set; } = string.Empty; // Task, Project, Comment, etc.
        public string? ResourceId { get; set; } // Stored as string to support different resource types
        public string? ResourceName { get; set; }
        public string? Description { get; set; }
        public string? ChangeDetails { get; set; } // JSON storing what changed
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
