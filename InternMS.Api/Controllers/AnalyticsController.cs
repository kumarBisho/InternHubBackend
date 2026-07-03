using InternMS.Api.DTOs.Analytics;
using InternMS.Api.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternMS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get complete analytics dashboard with all metrics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? projectId)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Manager"))
                {
                    return Forbid("Only Admin and Manager roles can access analytics");
                }

                var filter = new AnalyticsFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProjectId = projectId
                };

                var dashboard = await _analyticsService.GetDashboardAsync(filter, userRole);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDashboard: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving analytics" });
            }
        }

        /// <summary>
        /// Get project progress metrics
        /// </summary>
        [HttpGet("projects/progress")]
        public async Task<IActionResult> GetProjectsProgress([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? projectId)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Manager"))
                {
                    return Forbid("Only Admin and Manager roles can access analytics");
                }

                var filter = new AnalyticsFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProjectId = projectId
                };

                var projectsProgress = await _analyticsService.GetProjectsProgressAsync(filter);
                return Ok(projectsProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProjectsProgress: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving projects progress" });
            }
        }

        /// <summary>
        /// Get intern performance metrics
        /// </summary>
        [HttpGet("interns/performance")]
        public async Task<IActionResult> GetInternsPerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? userId)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Manager" && userRole != "Mentor"))
                {
                    return Forbid("Only Admin, Manager, and Mentor roles can access intern analytics");
                }

                var filter = new AnalyticsFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId
                };

                var internsPerformance = await _analyticsService.GetInternsPerformanceAsync(filter);
                return Ok(internsPerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetInternsPerformance: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving interns performance" });
            }
        }

        /// <summary>
        /// Get team performance summary
        /// </summary>
        [HttpGet("team/performance")]
        public async Task<IActionResult> GetTeamPerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Manager"))
                {
                    return Forbid("Only Admin and Manager roles can access team analytics");
                }

                var filter = new AnalyticsFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var teamPerformance = await _analyticsService.GetTeamPerformanceAsync(filter);
                return Ok(teamPerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTeamPerformance: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving team performance" });
            }
        }

        /// <summary>
        /// Get task completion trends over time
        /// </summary>
        [HttpGet("trends/completion")]
        public async Task<IActionResult> GetCompletionTrends([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Manager"))
                {
                    return Forbid("Only Admin and Manager roles can access trends");
                }

                var filter = new AnalyticsFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var trends = await _analyticsService.GetCompletionTrendsAsync(filter);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCompletionTrends: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving completion trends" });
            }
        }
    }
}
