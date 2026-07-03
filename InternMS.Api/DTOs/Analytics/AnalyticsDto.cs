namespace InternMS.Api.DTOs.Analytics;

/// <summary>
/// Represents task completion metrics over time
/// </summary>
public class TaskCompletionTrendDto
{
    public DateTime Date { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TotalTasks { get; set; }
    public decimal CompletionRate { get; set; } // Percentage 0-100
}

/// <summary>
/// Represents project progress metrics
/// </summary>
public class ProjectProgressDto
{
    public string ProjectId { get; set; }
    public string ProjectTitle { get; set; }
    public string Status { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal ProgressPercentage { get; set; } // 0-100
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int AssignedInterns { get; set; }
}

/// <summary>
/// Represents individual intern performance metrics
/// </summary>
public class InternPerformanceDto
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int TotalTasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksPending { get; set; }
    public decimal CompletionRate { get; set; } // 0-100
    public int OnTimeDeliveries { get; set; }
    public int LateDeliveries { get; set; }
    public decimal PerformanceScore { get; set; } // 0-100
}

/// <summary>
/// Represents team/overall performance summary
/// </summary>
public class TeamPerformanceDto
{
    public int TotalInterns { get; set; }
    public int TotalTasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksPending { get; set; }
    public decimal OverallCompletionRate { get; set; } // 0-100
    public decimal AverageInternPerformance { get; set; } // 0-100
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public decimal AverageTaskDuration { get; set; } // in days
}

/// <summary>
/// Analytics filter parameters
/// </summary>
public class AnalyticsFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ProjectId { get; set; }
    public string? UserId { get; set; }
}

/// <summary>
/// Combined analytics dashboard data
/// </summary>
public class AnalyticsDashboardDto
{
    public TeamPerformanceDto TeamPerformance { get; set; }
    public List<ProjectProgressDto> ProjectsProgress { get; set; }
    public List<InternPerformanceDto> InternPerformance { get; set; }
    public List<TaskCompletionTrendDto> CompletionTrends { get; set; }
}
