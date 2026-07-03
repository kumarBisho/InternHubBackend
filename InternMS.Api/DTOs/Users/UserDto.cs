namespace InternMS.Api.DTOs.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
    }
}