namespace InternMS.Api.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public string Role { get; set; } = "Admin";
        public int TotalUsers { get; set; }
        public int TotalInterns { get; set; }
        public int TotalMentors { get; set; }
        public int TotalProjects { get; set; }
    }
}