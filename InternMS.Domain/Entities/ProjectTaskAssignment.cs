using System;

namespace InternMS.Domain.Entities
{
    public class ProjectTaskAssignment
    {
        public int Id { get; set; }
        public Guid TaskId { get; set; }
        public ProjectTask Task { get; set; } = default!;
        public Guid InternId { get; set; }
        public User Intern { get; set; } = default!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
