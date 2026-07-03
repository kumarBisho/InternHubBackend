using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternMS.Api.Services.Collaboration;
using InternMS.Api.DTOs.Collaboration;
using System.Security.Claims;

namespace InternMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollaborationController : ControllerBase
    {
        private readonly ICollaborationService _collaborationService;
        private readonly ILogger<CollaborationController> _logger;

        public CollaborationController(ICollaborationService collaborationService, ILogger<CollaborationController> logger)
        {
            _collaborationService = collaborationService;
            _logger = logger;
        }

        // Get paginated activity logs
        [HttpPost("activities")]
        public async Task<IActionResult> GetActivities([FromBody] ActivityLogFilterDto filter)
        {
            try
            {
                _logger.LogInformation($"Getting activities with filter: {filter?.ResourceType}");
                var result = await _collaborationService.GetActivitiesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activities: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get recent activities
        [HttpGet("activities/recent/{limit:int?}")]
        public async Task<IActionResult> GetRecentActivities(int limit = 20)
        {
            try
            {
                _logger.LogInformation($"Getting {limit} recent activities");
                var activities = await _collaborationService.GetRecentActivitiesAsync(limit);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recent activities: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Log an activity
        [HttpPost("activities/log")]
        public async Task<IActionResult> LogActivity([FromBody] ActivityLogRequestDto activityRequest)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                
                // Extract user name using JWT standard claims with fallback
                var userName = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    var firstName = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName)?.Value ?? "";
                    var lastName = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName)?.Value ?? "";
                    userName = $"{firstName} {lastName}".Trim();
                }
                if (string.IsNullOrEmpty(userName))
                {
                    userName = userEmail;
                }
                if (string.IsNullOrEmpty(userName))
                {
                    userName = "Unknown User";
                }

                var activity = new ActivityLogDto
                {
                    UserId = activityRequest.UserId,
                    UserName = userName,
                    UserEmail = userEmail,
                    ActionType = activityRequest.ActionType,
                    ResourceType = activityRequest.ResourceType,
                    ResourceId = activityRequest.ResourceId,
                    ResourceName = activityRequest.ResourceName,
                    Description = activityRequest.Description,
                    ChangeDetails = activityRequest.ChangeDetails,
                    Timestamp = DateTime.UtcNow
                };

                var logged = await _collaborationService.LogActivityAsync(activity);
                return Ok(logged);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging activity: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get resource activities
        [HttpGet("activities/{resourceType}/{resourceId}")]
        public async Task<IActionResult> GetResourceActivities(string resourceType, int resourceId)
        {
            try
            {
                _logger.LogInformation($"Getting activities for {resourceType} {resourceId}");
                var activities = await _collaborationService.GetResourceActivitiesAsync(resourceType, resourceId.ToString());
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting resource activities: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Add a comment
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto comment)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                _logger.LogInformation($"User {userId} adding comment on {comment.ResourceType} {comment.ResourceId}");
                var created = await _collaborationService.AddCommentAsync(userId, comment);
                return CreatedAtAction(nameof(GetComments), created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding comment: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get comments for a resource
        [HttpGet("comments/{resourceType}/{resourceId}")]
        public async Task<IActionResult> GetComments(string resourceType, int resourceId)
        {
            try
            {
                _logger.LogInformation($"Getting comments for {resourceType} {resourceId}");
                var comments = await _collaborationService.GetCommentsAsync(resourceType, resourceId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting comments: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Update a comment
        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto updates)
        {
            try
            {
                _logger.LogInformation($"Updating comment {commentId}");
                var updated = await _collaborationService.UpdateCommentAsync(commentId, updates);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating comment: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Delete a comment
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                _logger.LogInformation($"Deleting comment {commentId}");
                await _collaborationService.DeleteCommentAsync(commentId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting comment: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get online users
        [HttpGet("online-users")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            try
            {
                _logger.LogInformation("Getting online users");
                var onlineUsers = await _collaborationService.GetOnlineUsersAsync();
                return Ok(onlineUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting online users: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get collaboration metrics
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                _logger.LogInformation("Getting collaboration metrics");
                var metrics = await _collaborationService.GetCollaborationMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting metrics: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
