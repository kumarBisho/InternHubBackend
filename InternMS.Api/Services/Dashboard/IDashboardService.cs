using InternMS.Api.DTOs.Dashboard;

namespace InternMS.Api.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<object> GetDashboardAsync(Guid userId, string role);
    }
}
