using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternMS.Api.Services;
using AutoMapper;
using InternMS.Api.DTOs.Notifications;
using System.Security.Claims;
using InternMS.Api.Exceptions;
using InternMS.Api.DTOs.Common;
using InternMS.Domain.Entities;

namespace InternMS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IMapper mapper,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Extract authenticated user ID from JWT token
        /// </summary>
        private Guid GetUserId()
        {
            var id = User.FindFirstValue("id");
            if (!Guid.TryParse(id, out var userId))
            {
                throw new UnauthorizedException("Invalid or missing user ID in token");
            }
            return userId;
        }

        #region Read Endpoints

        /// <summary>
        /// Get all notifications for authenticated user
        /// </summary>
        /// <returns>List of all non-deleted notifications</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Fetching notifications for user {UserId}", userId);
                
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
                
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notificationDtos));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to notifications");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications");
                throw new BusinessException("Failed to retrieve notifications", "FETCH_ERROR", 500);
            }
        }

        /// <summary>
        /// Get only unread notifications for authenticated user
        /// </summary>
        /// <returns>List of unread notifications</returns>
        [HttpGet("unread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetUnreadNotifications()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Fetching unread notifications for user {UserId}", userId);
                
                var notifications = await _notificationService.GetUserUnreadNotificationsAsync(userId);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
                
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notificationDtos));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to unread notifications");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread notifications");
                throw new BusinessException("Failed to retrieve unread notifications", "FETCH_ERROR", 500);
            }
        }

        /// <summary>
        /// Get count of unread notifications
        /// </summary>
        /// <returns>Count of unread notifications</returns>
        [HttpGet("unread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                
                return Ok(ApiResponse<int>.Ok(count));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to unread count");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread count");
                throw new BusinessException("Failed to retrieve unread count", "FETCH_ERROR", 500);
            }
        }

        /// <summary>
        /// Get notifications by type with optional date range
        /// </summary>
        /// <param name="type">Notification type to filter by</param>
        /// <param name="fromDate">Optional: Start date for filtering</param>
        /// <param name="toDate">Optional: End date for filtering</param>
        [HttpGet("by-type/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotificationsByType(
            [FromRoute] NotificationType type,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Fetching {Type} notifications for user {UserId}", type, userId);
                
                var notifications = await _notificationService.GetUserNotificationsByTypeAsync(
                    userId, type, fromDate, toDate);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
                
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notificationDtos));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to notifications by type");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications by type");
                throw new BusinessException("Failed to retrieve notifications by type", "FETCH_ERROR", 500);
            }
        }

        #endregion

        #region Update Endpoints

        /// <summary>
        /// Mark a specific notification as read
        /// </summary>
        /// <param name="id">Notification ID</param>
        [HttpPost("{id}/mark-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", id, userId);
                
                await _notificationService.MarkAsReadAsync(id, userId);
                
                return Ok(ApiResponse.Ok("Notification marked as read"));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
                throw;
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to mark notification");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw new BusinessException("Failed to mark notification as read", "UPDATE_ERROR", 500);
            }
        }

        /// <summary>
        /// Mark all notifications as read for authenticated user
        /// </summary>
        [HttpPost("mark-all-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);
                
                await _notificationService.MarkAllAsReadAsync(userId);
                
                return Ok(ApiResponse.Ok("All notifications marked as read"));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to mark all notifications");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw new BusinessException("Failed to mark all notifications as read", "UPDATE_ERROR", 500);
            }
        }

        #endregion

        #region Delete Endpoints

        /// <summary>
        /// Soft delete a specific notification
        /// </summary>
        /// <param name="id">Notification ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteNotification(int id)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", id, userId);
                
                await _notificationService.DeleteNotificationAsync(id, userId);
                
                return Ok(ApiResponse.Ok("Notification deleted successfully"));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
                throw;
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to delete notification");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw new BusinessException("Failed to delete notification", "DELETE_ERROR", 500);
            }
        }

        /// <summary>
        /// Clear all read notifications for authenticated user
        /// </summary>
        [HttpDelete("clear-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> ClearReadNotifications()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Clearing read notifications for user {UserId}", userId);
                
                await _notificationService.ClearReadNotificationsAsync(userId);
                
                return Ok(ApiResponse.Ok("Read notifications cleared successfully"));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to clear read notifications");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing read notifications");
                throw new BusinessException("Failed to clear read notifications", "DELETE_ERROR", 500);
            }
        }

        #endregion
    }
}