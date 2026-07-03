using System;
using System.Collections.Generic;

namespace InternMS.Domain.Entities
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Interests { get; set; } // Comma-separated or JSON array
        public string? ProfileImageUrl { get; set; }
        
        // Collection of skills
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}