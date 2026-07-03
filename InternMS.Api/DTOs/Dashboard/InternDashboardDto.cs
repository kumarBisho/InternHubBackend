namespace InternMS.Api.DTOs.Dashboard
{
    public class InternDashboardDto
    {
        public string Role { get; set; } = "Intern";
        public int AssignedProjects { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int UnreadNotifications { get; set; }
    }
}