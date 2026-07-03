using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Services.Users;
using InternMS.Api.DTOs.Profiles;
using AutoMapper;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace InternMS.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize] // applies to all endpoints
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, IMapper mapper, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out var userId))
                return Unauthorized("Invalid or missing user ID in token.");

            var profile = await _userService.GetProfileAsync(userId);

            if (profile == null)
                return Ok(new ProfileDto());

            return Ok(_mapper.Map<ProfileDto>(profile));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out var userId))
                return Unauthorized("Invalid or missing user ID in token.");

            try
            {
                _logger.LogInformation("Updating profile for user {UserId}", userId);
                await _userService.UpdateProfileAsync(userId, dto);
                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new { message = "Failed to update profile", error = ex.Message });
            }
        }

        [Authorize]  // All authenticated users can view any profile
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            _logger.LogInformation($"[ProfileController] GetProfile called for userId: {userId}");
            
            var profileDetail = await _userService.GetUserProfileDetailAsync(userId);
            
            if (profileDetail == null) 
            {
                _logger.LogWarning($"[ProfileController] Profile not found for userId: {userId}");
                return NotFound();
            }

            _logger.LogInformation($"[ProfileController] Profile retrieved successfully");
            _logger.LogInformation($"[ProfileController] User: {profileDetail.FirstName} {profileDetail.LastName}");
            _logger.LogInformation($"[ProfileController] Email: {profileDetail.Email}");
            _logger.LogInformation($"[ProfileController] Role from service: {profileDetail.Role ?? "NULL"}");
            _logger.LogInformation($"[ProfileController] Full profileDetail object: {@profileDetail}", profileDetail);

            return Ok(profileDetail);
        }
    }
}
