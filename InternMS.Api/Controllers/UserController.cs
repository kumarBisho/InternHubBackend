using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Services.Users;
using InternMS.Api.DTOs.Users;
using AutoMapper;

namespace InternMS.Api.Controllers
{
    [Route("api/Users")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper, ILogger<UserController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            Logger = logger;
        }

        // GET api/Users/me - Get current user info including roles from JWT
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUserInfo()
        {
            try
            {
                var firstName = User.FindFirst("given_name")?.Value;
                var lastName = User.FindFirst("family_name")?.Value;
                var email = GetUserEmail();
                var userId = GetUserId();

                // Get all roles from token
                var roles = User.FindAll("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role")
                    .Select(c => c.Value)
                    .ToList();

                // Alternative role claim name
                if (!roles.Any())
                {
                    roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();
                }

                var response = new
                {
                    userId,
                    email,
                    firstName,
                    lastName,
                    name = GetUserName(),
                    roles,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    allClaims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
                };

                Logger?.LogInformation("User current info retrieved");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Error getting current user info: {ex.Message}");
                return ServerError("Failed to get current user info", ex.Message);
            }
        }

        // GET api/Users/project-assignments-debug - Debug endpoint
        [HttpGet("project-assignments-debug")]
        public async Task<IActionResult> GetProjectAssignmentsDebug()
        {
            return await SafeExecute(async () =>
            {
                var assignments = await _userService.GetAllProjectAssignmentsAsync();
                Logger?.LogInformation($"Retrieved {assignments.Count()} project assignments");
                return Ok(assignments);
            }, "GetProjectAssignmentsDebug");
        }

        // GET api/Users/users-debug - Debug endpoint to get all users
        [HttpGet("users-debug")]
        public async Task<IActionResult> GetUsersDebug()
        {
            return await SafeExecute(async () =>
            {
                var users = await _userService.GetAllUsersAsync();
                var userList = users.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email }).ToList();
                Logger?.LogInformation($"Retrieved {userList.Count} users");
                return Ok(userList);
            }, "GetUsersDebug");
        }

        // GET api/Users/mentor/{mentorId}/interns - Must be before {id}
        [HttpGet("mentor/{mentorId}/interns")]
        [Authorize(Roles = "Admin,Manager,Mentor")]
        public async Task<IActionResult> GetInternsByMentor(Guid mentorId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(mentorId, "Mentor ID");
                if (!isValid) return err;

                var userId = TryGetUserId();
                var isAdmin = User.IsInRole("Admin");
                var isManager = User.IsInRole("Manager");
                var isMentor = User.IsInRole("Mentor");

                // Authorization: Admin and Manager can view all interns, Mentor can only view their own
                if (!isAdmin && !isManager && (!isMentor || userId != mentorId))
                {
                    Logger?.LogWarning($"User {userId} attempted unauthorized access to mentor {mentorId}'s interns");
                    return Forbidden("Not authorized to view these interns");
                }

                var interns = await _userService.GetInternsByMentorAsync(mentorId);
                var dto = _mapper.Map<IEnumerable<UserDto>>(interns);

                Logger?.LogInformation($"Retrieved {dto.Count()} interns for mentor {mentorId}");
                return Ok(dto);
            }, "GetInternsByMentor");
        }

        // GET api/Users/mentor/{mentorId}/project/{projectId}/interns - Get interns assigned to mentor for specific project
        [HttpGet("mentor/{mentorId}/project/{projectId}/interns")]
        [Authorize(Roles = "Admin,Manager,Mentor")]
        public async Task<IActionResult> GetInternsByMentorAndProject(Guid mentorId, Guid projectId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(mentorId, "Mentor ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotEmptyGuid(projectId, "Project ID");
                if (!isValid2) return err2;

                var userId = TryGetUserId();
                var isAdmin = User.IsInRole("Admin");
                var isManager = User.IsInRole("Manager");
                var isMentor = User.IsInRole("Mentor");

                // Authorization: Admin and Manager can view all interns, Mentor can only view their own
                if (!isAdmin && !isManager && (!isMentor || userId != mentorId))
                {
                    return Forbidden("Not authorized to view these interns");
                }

                var interns = await _userService.GetInternsByMentorAndProjectAsync(mentorId, projectId);
                var dto = _mapper.Map<IEnumerable<UserDto>>(interns);

                Logger?.LogInformation($"Retrieved {dto.Count()} interns for mentor {mentorId} in project {projectId}");
                return Ok(dto);
            }, "GetInternsByMentorAndProject");
        }

        // GET api/Users/intern/{internId}/mentor - Must be before {id}
        [HttpGet("intern/{internId}/mentor")]
        [Authorize]
        public async Task<IActionResult> GetAssignedMentor(Guid internId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(internId, "Intern ID");
                if (!isValid) return err;

                var currentUserId = TryGetUserId();

                // Verify the user is accessing their own mentor
                var (hasOwnership, forbidden) = ValidateUserOwnership(currentUserId ?? Guid.Empty, internId);
                if (!hasOwnership) return forbidden;

                var mentor = await _userService.GetAssignedMentorAsync(internId);

                var (exists, notFound) = ValidateNotNull(mentor, "Mentor assignment", internId);
                if (!exists) return notFound;

                var dto = _mapper.Map<UserDto>(mentor);
                Logger?.LogInformation($"Retrieved mentor for intern {internId}");
                return Ok(dto);
            }, "GetAssignedMentor");
        }

        // GET api/user
        [HttpGet]
        [Authorize(Roles = "Admin, Manager, Mentor, Intern")]
        public async Task<IActionResult> GetAll()
        {
            return await SafeExecute(async () =>
            {
                var users = await _userService.GetAllUsersAsync();
                var dto = _mapper.Map<IEnumerable<UserDto>>(users);
                Logger?.LogInformation($"Retrieved {dto.Count()} users");
                return Ok(dto);
            }, "GetAll");
        }

        // GET api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(id, "User ID");
                if (!isValid) return err;

                var user = await _userService.GetUserByIdAsync(id);

                var (exists, notFound) = ValidateNotNull(user, "User", id);
                if (!exists) return notFound;

                var dto = _mapper.Map<UserDto>(user);
                return Ok(dto);
            }, "Get");
        }

        // GET api/Users/mentors-and-interns - Get all mentors and interns (Admin and Manager only)
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("mentors-and-interns")]
        public async Task<IActionResult> GetAllMentorsAndInterns()
        {
            return await SafeExecute(async () =>
            {
                var users = await _userService.GetAllMentorsAndInternsAsync();
                var dto = _mapper.Map<IEnumerable<UserDto>>(users);
                Logger?.LogInformation($"Retrieved {dto.Count()} mentors and interns");
                return Ok(dto);
            }, "GetAllMentorsAndInterns");
        }

        // PUT api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(id, "User ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotNull(dto, "User data");
                if (!isValid2) return err2;

                await _userService.UpdateUserAsync(id, dto);
                Logger?.LogInformation($"User {id} updated");
                return NoContent();
            }, "Update");
        }

        // DELETE api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(id, "User ID");
                if (!isValid) return err;

                await _userService.DeleteUserAsync(id);
                Logger?.LogInformation($"User {id} deleted");
                return NoContent();
            }, "Delete");
        }
    }
}