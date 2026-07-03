using InternMS.Api.DTOs.Analytics;
using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext db, ILogger<AnalyticsService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AnalyticsDashboardDto> GetDashboardAsync(AnalyticsFilterDto filter, string userRole)
        {
            try
            {
                var dashboard = new AnalyticsDashboardDto
                {
                    TeamPerformance = await GetTeamPerformanceAsync(filter),
                    ProjectsProgress = await GetProjectsProgressAsync(filter),
                    InternPerformance = await GetInternsPerformanceAsync(filter),
                    CompletionTrends = await GetCompletionTrendsAsync(filter)
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDashboardAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ProjectProgressDto>> GetProjectsProgressAsync(AnalyticsFilterDto filter)
        {
            try
            {
                var query = _db.Projects.AsQueryable();

                if (filter.StartDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= filter.StartDate);

                if (filter.EndDate.HasValue)
                    query = query.Where(p => p.CreatedAt <= filter.EndDate);

                if (!string.IsNullOrEmpty(filter.ProjectId))
                    query = query.Where(p => p.Id.ToString() == filter.ProjectId);

                var projects = await query
                    .Select(p => new ProjectProgressDto
                    {
                        ProjectId = p.Id.ToString(),
                        ProjectTitle = p.Title,
                        Status = p.Status.ToString(),
                        TotalTasks = p.Tasks.Count,
                        CompletedTasks = p.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed),
                        ProgressPercentage = p.Tasks.Count == 0 ? 0 :
                            (decimal)p.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed) / 
                            p.Tasks.Count * 100,
                        StartDate = p.StartDate ?? p.CreatedAt,
                        EndDate = p.EndDate,
                        AssignedInterns = p.Assignments.Count
                    })
                    .ToListAsync();

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProjectsProgressAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InternPerformanceDto>> GetInternsPerformanceAsync(AnalyticsFilterDto filter)
        {
            try
            {
                var internRoleId = await _db.Roles.Where(r => r.Name == "Intern")
                    .Select(r => r.Id).FirstOrDefaultAsync();

                if (internRoleId == default)
                    return new List<InternPerformanceDto>();

                // Get all interns
                var interns = await _db.Users
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == internRoleId))
                    .ToListAsync();

                if (!string.IsNullOrEmpty(filter.UserId) && Guid.TryParse(filter.UserId, out var userId))
                {
                    interns = interns.Where(u => u.Id == userId).ToList();
                }

                var internPerformances = new List<InternPerformanceDto>();

                foreach (var intern in interns)
                {
                    var taskAssignments = await _db.ProjectTaskAssignments
                        .Where(pta => pta.InternId == intern.Id)
                        .Include(pta => pta.Task)
                        .ToListAsync();

                    var completed = taskAssignments.Count(ta => ta.Task.Status == ProjectTaskStatus.Completed);
                    var total = taskAssignments.Count;
                    var completionRate = total == 0 ? 0 : (decimal)completed / total * 100;

                    var onTime = taskAssignments.Count(ta => 
                        ta.Task.EndDate >= DateTime.UtcNow && 
                        ta.Task.Status == ProjectTaskStatus.Completed);

                    var late = taskAssignments.Count(ta => 
                        ta.Task.EndDate < DateTime.UtcNow && 
                        ta.Task.Status != ProjectTaskStatus.Completed);

                    var onTimeScore = total > 0 ? (decimal)onTime / total * 100 : 0;
                    var performanceScore = completionRate * 0.6m + onTimeScore * 0.4m;

                    internPerformances.Add(new InternPerformanceDto
                    {
                        UserId = intern.Id.ToString(),
                        FirstName = intern.FirstName,
                        LastName = intern.LastName,
                        Email = intern.Email,
                        TotalTasksAssigned = total,
                        TasksCompleted = completed,
                        TasksPending = total - completed,
                        CompletionRate = (decimal)Math.Round(completionRate, 2),
                        OnTimeDeliveries = onTime,
                        LateDeliveries = late,
                        PerformanceScore = (decimal)Math.Round(performanceScore, 2)
                    });
                }

                return internPerformances;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetInternsPerformanceAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<TeamPerformanceDto> GetTeamPerformanceAsync(AnalyticsFilterDto filter)
        {
            try
            {
                var internRoleId = await _db.Roles.Where(r => r.Name == "Intern")
                    .Select(r => r.Id).FirstOrDefaultAsync();

                var taskQuery = _db.ProjectTasks.AsQueryable();

                if (filter.StartDate.HasValue)
                    taskQuery = taskQuery.Where(t => t.CreatedAt >= filter.StartDate);

                if (filter.EndDate.HasValue)
                    taskQuery = taskQuery.Where(t => t.CreatedAt <= filter.EndDate);

                var totalInterns = await _db.Users
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == internRoleId))
                    .CountAsync();

                var allTasks = await taskQuery.ToListAsync();
                var totalTasks = allTasks.Count;
                var completedTasks = allTasks.Count(t => t.Status == ProjectTaskStatus.Completed);
                var inProgressTasks = allTasks.Count(t => t.Status == ProjectTaskStatus.InProgress);
                var activeTasks = allTasks.Count(t => t.Status == ProjectTaskStatus.Active);

                var internPerformances = await GetInternsPerformanceAsync(filter);
                var averagePerformance = internPerformances.Count > 0 ?
                    internPerformances.Average(i => i.PerformanceScore) : 0;

                var onTimeCount = allTasks
                    .Count(t => t.EndDate >= DateTime.UtcNow && t.Status == ProjectTaskStatus.Completed);

                var lateCount = allTasks
                    .Count(t => t.EndDate < DateTime.UtcNow && t.Status != ProjectTaskStatus.Completed);

                var avgDuration = await CalculateAverageTaskDurationAsync(filter);

                return new TeamPerformanceDto
                {
                    TotalInterns = totalInterns,
                    TotalTasksAssigned = totalTasks,
                    TasksCompleted = completedTasks,
                    TasksPending = inProgressTasks + activeTasks,
                    OverallCompletionRate = totalTasks == 0 ? 0 :
                        (decimal)completedTasks / totalTasks * 100,
                    AverageInternPerformance = (decimal)Math.Round(averagePerformance, 2),
                    OnTimeCount = onTimeCount,
                    LateCount = lateCount,
                    AverageTaskDuration = avgDuration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTeamPerformanceAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskCompletionTrendDto>> GetCompletionTrendsAsync(AnalyticsFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-3);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var allTasks = await _db.ProjectTasks
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                    .ToListAsync();

                var trends = allTasks
                    .GroupBy(t => t.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new TaskCompletionTrendDto
                    {
                        Date = g.Key,
                        CompletedTasks = g.Count(t => t.Status == ProjectTaskStatus.Completed),
                        PendingTasks = g.Count(t => t.Status == ProjectTaskStatus.Active),
                        InProgressTasks = g.Count(t => t.Status == ProjectTaskStatus.InProgress),
                        TotalTasks = g.Count(),
                        CompletionRate = g.Count() == 0 ? 0 :
                            (decimal)g.Count(t => t.Status == ProjectTaskStatus.Completed) /
                            g.Count() * 100
                    })
                    .ToList();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCompletionTrendsAsync: {ex.Message}");
                throw;
            }
        }

        private async Task<decimal> CalculateAverageTaskDurationAsync(AnalyticsFilterDto filter)
        {
            try
            {
                var completedTasks = await _db.ProjectTasks
                    .Where(t => t.Status == ProjectTaskStatus.Completed)
                    .ToListAsync();

                if (completedTasks.Count == 0)
                    return 0;

                var totalDuration = completedTasks.Sum(t =>
                    (t.CreatedAt - t.CreatedAt).TotalDays
                );

                return (decimal)(totalDuration / completedTasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CalculateAverageTaskDurationAsync: {ex.Message}");
                return 0;
            }
        }
    }
}
