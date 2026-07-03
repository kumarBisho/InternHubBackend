using System;

namespace InternMS.Domain.Entities
{
    public class ProjectUpdate
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = default!;
        public ProjectStatus Status { get; set; } 
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}