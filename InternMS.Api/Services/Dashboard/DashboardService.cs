using InternMS.Api.DTOs.Dashboard;
using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;

namespace InternMS.Api.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetDashboardAsync(Guid userId, string role)
        {
            return role switch
            {
                "Intern" => await GetInternDashboard(userId),
                "Mentor" => await GetMentorDashboard(userId),
                "Manager" => await GetManagerDashboard(),
                "Admin" => await GetAdminDashboard(),
                _ => throw new UnauthorizedAccessException("Invalid role")
            };
        }

        private async Task<InternDashboardDto> GetInternDashboard(Guid userId)
        {
            return new InternDashboardDto
            {
                Role = "Intern",
                AssignedProjects = await _db.ProjectAssignments.CountAsync(p => p.InternId == userId),
                // TotalTasks = await _db.ProjectTasks.CountAsync(t => t.AssignedToId == userId),
                // PendingTasks = await _db.ProjectTasks.CountAsync(t =>
                //     t.AssignedToId == userId && t.Status != ProjectTaskStatus.Completed),
                // CompletedTasks = await _db.ProjectTasks.CountAsync(t =>
                //     t.AssignedToId == userId && t.Status == ProjectTaskStatus.Completed),
                UnreadNotifications = await _db.Notifications.CountAsync(n =>
                    n.UserId == userId && !n.IsRead)
            };
        }

        private async Task<MentorDashboardDto> GetMentorDashboard(Guid userId)
        {
            return new MentorDashboardDto
            {
                Role = "Mentor",
                TotalInterns = await _db.Users.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Intern")),
                ActiveProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Active),
                PendingTasks = await _db.ProjectTasks.CountAsync(t =>
                    t.Status == ProjectTaskStatus.InProgress)
            };
        }

        private async Task<ManagerDashboardDto> GetManagerDashboard()
        {
            return new ManagerDashboardDto
            {
                Role = "Manager",
                TotalProjects = await _db.Projects.CountAsync(),
                ActiveProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Active),
                TotalTasks = await _db.ProjectTasks.CountAsync(),
                PendingTasks = await _db.ProjectTasks.CountAsync(t => t.Status == ProjectTaskStatus.InProgress),
                CompletedTasks = await _db.ProjectTasks.CountAsync(t => t.Status == ProjectTaskStatus.Completed),
                TotalUsers = await _db.Users.CountAsync(),
                TotalInterns = await _db.Users.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Intern")),
                TotalMentors = await _db.Users.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Mentor"))
            };
        }

        private async Task<AdminDashboardDto> GetAdminDashboard()
        {
            return new AdminDashboardDto
            {
                Role = "Admin",
                TotalUsers = await _db.Users.CountAsync(),
                TotalInterns = await _db.Users.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Intern")),
                TotalMentors = await _db.Users.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Mentor")),
                TotalProjects = await _db.Projects.CountAsync()
            };
        }
    }
}