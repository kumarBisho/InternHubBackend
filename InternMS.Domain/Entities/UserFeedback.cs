using System;
using InternMS.Domain.Enums;

namespace InternMS.Domain.Entities
{
    public class UserFeedback
    {
        public Guid Id { get; set; }
        
        // Mentor providing the feedback
        public Guid MentorId { get; set; }
        public User Mentor { get; set; } = default!;
        
        // Intern receiving the feedback
        public Guid InternId { get; set; }
        public User Intern { get; set; } = default!;
        
        // Feedback content
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public FeedbackType Type { get; set; }
        
        // Optional references to task or project
        public Guid? TaskId { get; set; }
        public ProjectTask? Task { get; set; }
        
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        
        // Rating (1-5)
        public int? Rating { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
