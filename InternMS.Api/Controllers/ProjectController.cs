using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Services.Projects;
using InternMS.Api.Services.Collaboration;
using InternMS.Api.DTOs.Projects;
using InternMS.Api.DTOs.Collaboration;
using AutoMapper;
using System.Security.Claims;

namespace InternMS.Api.Controllers
{
    [Route("api/projects")]
    public class ProjectController : BaseApiController
    {
        private readonly IProjectService _projectService;
        private readonly ICollaborationService _collaborationService;
        private readonly IMapper _mapper;

        public ProjectController(
            IProjectService projectService, 
            ICollaborationService collaborationService,
            IMapper mapper,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _collaborationService = collaborationService;
            _mapper = mapper;
            Logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            return await SafeExecute(async () =>
            {
                var userId = GetUserId();
                var userName = GetUserName();
                var userEmail = GetUserEmail();

                var project = await _projectService.CreateProjectAsync(userId, dto);

                // Log activity for project creation
                var activity = new ActivityLogDto
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    ActionType = "Created",
                    ResourceType = "Project",
                    ResourceId = project.Id.ToString(),
                    ResourceName = project.Title,
                    Description = $"Created new project: {project.Title}",
                    Timestamp = DateTime.UtcNow,
                    ChangeDetails = $"{{\"Title\": \"{project.Title}\", \"Description\": \"{project.Description}\", \"Status\": \"{project.Status}\"}}"
                };

                await _collaborationService.LogActivityAsync(activity);
                Logger?.LogInformation($"Project created: {project.Title} by {userName}");

                return Ok(_mapper.Map<ProjectDto>(project));
            }, "CreateProject");
        }

        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost("{projectId}/assign")]
        public async Task<IActionResult> Assign(Guid projectId, [FromBody] AssignProjectDto dto)
        {
            return await SafeExecute(async () =>
            {
                // Validate input using helpers
                var (isValid1, err1) = ValidateNotNull(dto, "Assignment data");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotEmptyGuid(dto.InternId, "Intern ID");
                if (!isValid2) return err2;

                var (isValid3, err3) = ValidateNotEmptyGuid(dto.MentorId ?? Guid.Empty, "Mentor ID");
                if (!isValid3) return err3;

                var (isValid4, err4) = ValidateNotEmptyGuid(projectId, "Project ID");
                if (!isValid4) return err4;

                var userName = GetUserName();
                var userEmail = GetUserEmail();

                await _projectService.AssignProjectAsync(projectId, dto);

                // Log activity for assignment
                var activity = new ActivityLogDto
                {
                    UserId = GetUserId(),
                    UserName = userName,
                    UserEmail = userEmail,
                    ActionType = "Assigned",
                    ResourceType = "Project",
                    ResourceId = projectId.ToString(),
                    ResourceName = "Project Assignment",
                    Description = "Project assigned to Intern and Mentor",
                    Timestamp = DateTime.UtcNow,
                    ChangeDetails = $"{{\"InternId\": \"{dto.InternId}\", \"MentorId\": \"{dto.MentorId}\"}}"
                };

                await _collaborationService.LogActivityAsync(activity);
                Logger?.LogInformation($"Project {projectId} assigned by {userName}");

                return Ok(new { message = "Project assigned successfully." });
            }, "AssignProject");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            return await SafeExecute(async () =>
            {
                var userId = GetUserId();
                var role = GetUserRole();

                var projects = await _projectService.GetProjectsForUserAsync(userId, role);
                return Ok(_mapper.Map<IEnumerable<ProjectDto>>(projects));
            }, "GetProjects");
        }

        /// <summary>
        /// Get projects assigned to a specific mentor
        /// MUST come BEFORE [HttpGet("{id}")] because specific routes take precedence
        /// </summary>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("mentor/{mentorId}")]
        public async Task<IActionResult> GetProjectsForMentor(Guid mentorId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(mentorId, "Mentor ID");
                if (!isValid) return err;

                var projects = await _projectService.GetProjectsForUserAsync(mentorId, "Mentor");
                return Ok(_mapper.Map<IEnumerable<ProjectDto>>(projects));
            }, "GetProjectsForMentor");
        }

        /// <summary>
        /// Get projects assigned to a specific intern
        /// MUST come BEFORE [HttpGet("{id}")] because specific routes take precedence
        /// </summary>
        [Authorize]
        [HttpGet("intern/{internId}")]
        public async Task<IActionResult> GetProjectsForIntern(Guid internId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(internId, "Intern ID");
                if (!isValid) return err;

                var projects = await _projectService.GetProjectsForUserAsync(internId, "Intern");
                return Ok(_mapper.Map<IEnumerable<ProjectDto>>(projects));
            }, "GetProjectsForIntern");
        }

        /// <summary>
        /// Debug endpoint: Get projects assigned to a specific intern with detailed info
        /// MUST come BEFORE [HttpGet("{id}")] because specific routes take precedence
        /// </summary>
        [Authorize]
        [HttpGet("debug/intern/{internId}")]
        public async Task<IActionResult> DebugGetProjectsForIntern(Guid internId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(internId, "Intern ID");
                if (!isValid) return err;

                // Get projects using GetProjectsForUserAsync
                var projects = await _projectService.GetProjectsForUserAsync(internId, "Intern");

                return Ok(new
                {
                    internId = internId.ToString(),
                    timestamp = DateTime.UtcNow,
                    projectsCount = projects.Count(),
                    debug_info = "If count is 0, check: 1) Is InternId correct? 2) Are there assignments in project_assignments table for this intern? 3) Check project.Assignments navigation property is loaded",
                    projects = projects.Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.Status,
                        p.Progress,
                        assignmentCount = p.Assignments.Count(),
                        internAssignmentCount = p.Assignments.Count(a => a.InternId == internId)
                    })
                });
            }, "DebugGetProjectsForIntern");
        }

        /// <summary>
        /// Get a single project by ID
        /// MUST come AFTER specific routes like "mentor/{mentorId}" and "intern/{internId}"
        /// because the route matching is greedy (left-to-right)
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(id, "Project ID");
                if (!isValid) return err;

                var project = await _projectService.GetProjectByIdAsync(id);

                var (exists, notFound) = ValidateNotNull(project, "Project", id);
                if (!exists) return notFound;

                return Ok(project);
            }, "GetProject");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] PartialUpdateProjectDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(id, "Project ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotNull(dto, "Project data");
                if (!isValid2) return err2;

                var project = await _projectService.PartialUpdateProjectAsync(id, dto);

                var (exists, notFound) = ValidateNotNull(project, "Project", id);
                if (!exists) return notFound;

                var userName = GetUserName();
                Logger?.LogInformation($"Project {id} updated by {userName}");

                return Ok(_mapper.Map<ProjectDto>(project));
            }, "UpdateProject");
        }

        [Authorize(Roles = "Admin,Mentor")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdateProject(Guid id, [FromBody] PartialUpdateProjectDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(id, "Project ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotNull(dto, "Project data");
                if (!isValid2) return err2;

                var project = await _projectService.PartialUpdateProjectAsync(id, dto);

                var (exists, notFound) = ValidateNotNull(project, "Project", id);
                if (!exists) return notFound;

                return Ok(project);
            }, "PartialUpdateProject");
        }

        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost("{projectId}/update")]
        public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] CreateProjectUpdateDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(projectId, "Project ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotNull(dto, "Update data");
                if (!isValid2) return err2;

                var userId = GetUserId();
                var userName = GetUserName();

                var update = await _projectService.AddProjectUpdateAsync(projectId, userId, dto);

                var (exists, notFound) = ValidateNotNull(update, "Project update", projectId);
                if (!exists) return notFound;

                Logger?.LogInformation($"Project {projectId} updated by {userName}");

                return Ok(update);
            }, "AddProjectUpdate");
        }

        [Authorize(Roles = "Admin,Mentor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(id, "Project ID");
                if (!isValid) return err;

                var userId = GetUserId();
                var userName = GetUserName();

                await _projectService.DeleteProjectAsync(id);

                Logger?.LogInformation($"Project {id} deleted by {userName}");

                return NoContent();
            }, "DeleteProject");
        }
    }
}