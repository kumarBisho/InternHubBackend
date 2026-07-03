using System;

namespace InternMS.Api.DTOs.Feedback
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid MentorId { get; set; }
        public string MentorName { get; set; } = string.Empty;
        public Guid InternId { get; set; }
        public string InternName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public Guid? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
