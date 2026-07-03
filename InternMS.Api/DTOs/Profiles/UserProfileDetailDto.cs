using System;
using System.Collections.Generic;

namespace InternMS.Api.DTOs.Profiles
{
    public class UserProfileDetailDto
    {
        // User Information
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }  // Added: User's primary role

        // Profile Information
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Interests { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }
}
