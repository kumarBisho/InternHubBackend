using System;

namespace InternMS.Domain.Entities
{
    public class Skill
    {
        public Guid Id { get; set; }
        public Guid UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced, Expert
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
