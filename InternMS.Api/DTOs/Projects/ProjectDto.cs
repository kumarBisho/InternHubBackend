using System.Collections.Generic;

namespace InternMS.Api.DTOs.Projects
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? TechStack { get; set; } // Parsed from JSON array
        public int Progress { get; set; } = 0; // 0-100 percentage
        public string? RepositoryUrl { get; set; }
        public string? DocumentationUrl { get; set; }
        public string? DemoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
    }
}