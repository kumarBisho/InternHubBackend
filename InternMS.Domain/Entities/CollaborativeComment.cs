using System;
using System.Collections.Generic;

namespace InternMS.Domain.Entities
{
    public class CollaborativeComment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;  // Stores User.Id (Guid) as string
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty; // "Task", "Project"
        public int ResourceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Replies to this comment
        public ICollection<CommentReply> Replies { get; set; } = new List<CommentReply>();
    }

    public class CommentReply
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public CollaborativeComment Comment { get; set; } = default!;
        public string UserId { get; set; } = string.Empty;  // Stores User.Id (Guid) as string
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
