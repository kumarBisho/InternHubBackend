using InternMS.Api.DTOs.Analytics;

namespace InternMS.Api.Services.Analytics
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDashboardDto> GetDashboardAsync(AnalyticsFilterDto filter, string userRole);
        Task<List<ProjectProgressDto>> GetProjectsProgressAsync(AnalyticsFilterDto filter);
        Task<List<InternPerformanceDto>> GetInternsPerformanceAsync(AnalyticsFilterDto filter);
        Task<TeamPerformanceDto> GetTeamPerformanceAsync(AnalyticsFilterDto filter);
        Task<List<TaskCompletionTrendDto>> GetCompletionTrendsAsync(AnalyticsFilterDto filter);
    }
}
