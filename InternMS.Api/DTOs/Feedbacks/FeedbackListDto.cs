using System;

namespace InternMS.Api.DTOs.Feedback
{
    public class FeedbackListDto
    {
        public Guid Id { get; set; }
        public string MentorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUnread { get; set; }
    }
}
