using System;
using System.Collections.Generic;

namespace InternMS.Domain.Entities
{
    public class ProjectTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = default!;

        public DateTime EndDate { get; set; }
        public Priority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectTaskAssignment> Assignments { get; set; } = new List<ProjectTaskAssignment>();
        public ICollection<UserFeedback> Feedback { get; set; } = new List<UserFeedback>();
    }
}
