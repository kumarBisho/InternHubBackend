namespace InternMS.Api.DTOs.Dashboard
{
    public class MentorDashboardDto
    {
        public string Role { get; set; } = "Mentor";
        public int ActiveProjects { get; set; }
        public int TotalInterns { get; set; }
        public int PendingTasks { get; set; }
    }
}