using System;
using System.Collections.Generic;

namespace InternMS.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TechStack { get; set; } // JSON array of technologies: ["React", "Node.js", "MongoDB"]
        public int Progress { get; set; } = 0; // 0-100 percentage
        public string? RepositoryUrl { get; set; } // GitHub/GitLab repository link
        public string? DocumentationUrl { get; set; } // Documentation link
        public string? DemoUrl { get; set; } // Live demo link
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = default!;

        public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
        public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<UserFeedback> Feedback { get; set; } = new List<UserFeedback>();
    }

}