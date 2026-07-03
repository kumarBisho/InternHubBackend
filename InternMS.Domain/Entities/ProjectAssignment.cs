using System;

namespace InternMS.Domain.Entities
{
    public class ProjectAssignment
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public Guid InternId { get; set; }
        public User Intern { get; set; } = default!;
        public Guid? MentorId { get; set; } 
        public User? Mentor { get; set; }
        public DateTime AssignedAt { get; set; }= DateTime.UtcNow;

    }
}