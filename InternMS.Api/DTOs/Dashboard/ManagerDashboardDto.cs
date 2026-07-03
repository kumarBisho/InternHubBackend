namespace InternMS.Api.DTOs.Dashboard
{
    public class ManagerDashboardDto
    {
        public string Role { get; set; } = "Manager";
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalInterns { get; set; }
        public int TotalMentors { get; set; }
    }
}
