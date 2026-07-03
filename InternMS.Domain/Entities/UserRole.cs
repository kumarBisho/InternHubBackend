using System.Collections.Generic;

namespace InternMS.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = default!;
    }
}