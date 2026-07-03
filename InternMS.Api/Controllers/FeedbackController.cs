using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Services.Feedback;
using InternMS.Api.DTOs.Feedback;
using System.Security.Claims;

namespace InternMS.Api.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var id = User.FindFirstValue("id");
            if (!Guid.TryParse(id, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user ID in token.");
            return userId;
        }

        /// <summary>
        /// Create new feedback (Mentor only)
        /// </summary>
        [Authorize(Roles = "Mentor,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var mentorId = GetUserId();
                var feedback = await _feedbackService.CreateFeedbackAsync(dto, mentorId);

                _logger.LogInformation($"Feedback created by mentor {mentorId} for intern {dto.InternId}");
                return Created($"/api/feedback/{feedback.Id}", feedback);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Feedback creation failed: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized feedback creation: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid feedback creation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to create feedback", details = ex.Message });
            }
        }

        /// <summary>
        /// Get feedback by ID
        /// </summary>
        [HttpGet("{feedbackId}")]
        public async Task<IActionResult> GetFeedback(Guid feedbackId)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackByIdAsync(feedbackId);
                if (feedback == null)
                    return NotFound(new { message = "Feedback not found" });

                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Get feedback received by intern
        /// </summary>
        [HttpGet("received/{internId}")]
        public async Task<IActionResult> GetFeedbackReceivedByIntern(Guid internId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserId();
                
                // Verify that the user is requesting their own feedback or is an admin
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (userId != internId && userRole != "Admin")
                    return Forbid();

                var result = await _feedbackService.GetFeedbackReceivedByInternAsync(internId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Get feedback given by mentor
        /// </summary>
        [HttpGet("given/{mentorId}")]
        public async Task<IActionResult> GetFeedbackGivenByMentor(Guid mentorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserId();
                
                // Verify that the user is requesting their own feedback or is an admin
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (userId != mentorId && userRole != "Admin")
                    return Forbid();

                var result = await _feedbackService.GetFeedbackGivenByMentorAsync(mentorId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Get feedback for a specific task
        /// </summary>
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetFeedbackForTask(Guid taskId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _feedbackService.GetFeedbackForTaskAsync(taskId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving task feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Get feedback for a specific project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetFeedbackForProject(Guid projectId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _feedbackService.GetFeedbackForProjectAsync(projectId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving project feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Get feedback by type (TaskFeedback, ProjectFeedback, GeneralFeedback)
        /// </summary>
        [HttpGet("type/{internId}")]
        public async Task<IActionResult> GetFeedbackByType(Guid internId, [FromQuery] string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                    return BadRequest(new { message = "Feedback type is required" });

                var userId = GetUserId();
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                if (userId != internId && userRole != "Admin")
                    return Forbid();

                var result = await _feedbackService.GetFeedbackByTypeAsync(internId, type);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid feedback type: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve feedback" });
            }
        }

        /// <summary>
        /// Update feedback (Mentor only - own feedback)
        /// </summary>
        [Authorize(Roles = "Mentor,Admin")]
        [HttpPut("{feedbackId}")]
        public async Task<IActionResult> UpdateFeedback(Guid feedbackId, [FromBody] UpdateFeedbackDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var mentorId = GetUserId();
                await _feedbackService.UpdateFeedbackAsync(feedbackId, dto, mentorId);

                _logger.LogInformation($"Feedback {feedbackId} updated by mentor {mentorId}");
                return Ok(new { message = "Feedback updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Feedback not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized feedback update: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to update feedback" });
            }
        }

        /// <summary>
        /// Delete feedback (Mentor only - own feedback)
        /// </summary>
        [Authorize(Roles = "Mentor,Admin")]
        [HttpDelete("{feedbackId}")]
        public async Task<IActionResult> DeleteFeedback(Guid feedbackId)
        {
            try
            {
                var mentorId = GetUserId();
                await _feedbackService.DeleteFeedbackAsync(feedbackId, mentorId);

                _logger.LogInformation($"Feedback {feedbackId} deleted by mentor {mentorId}");
                return Ok(new { message = "Feedback deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Feedback not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized feedback deletion: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting feedback: {ex.Message}");
                return StatusCode(500, new { message = "Failed to delete feedback" });
            }
        }
    }
}
