using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Services.Tasks;
using InternMS.Api.Services.Collaboration;
using InternMS.Api.DTOs.Tasks;
using InternMS.Api.DTOs.Collaboration;
using System.Security.Claims;

namespace InternMS.Api.Controllers
{
    [Route("api/tasks")]
    [Authorize]
    public class TaskController : BaseApiController
    {
        private readonly ITaskService _taskService;
        private readonly ICollaborationService _collaborationService;

        public TaskController(
            ITaskService taskService,
            ICollaborationService collaborationService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _collaborationService = collaborationService;
            Logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotNull(dto, "Task data");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotEmpty(dto.Title, "Task title");
                if (!isValid2) return err2;

                var (isValid3, err3) = ValidateNotEmptyGuid(dto.ProjectId, "Project ID");
                if (!isValid3) return err3;

                var userId = GetUserId();
                var userName = GetUserName();
                var userEmail = GetUserEmail();

                var task = await _taskService.CreateTaskAsync(dto, userId);

                var (exists, notFound) = ValidateNotNull(task, "Task");
                if (!exists) return notFound;

                // Log activity for task creation
                var activity = new ActivityLogDto
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    ActionType = "Created",
                    ResourceType = "Task",
                    ResourceId = task.Id.ToString(),
                    ResourceName = task.Title,
                    Description = $"Created new task: {task.Title}",
                    Timestamp = DateTime.UtcNow,
                    ChangeDetails = $"{{\"Priority\": \"{task.Priority}\", \"Status\": \"{task.Status}\"}}"
                };

                await _collaborationService.LogActivityAsync(activity);
                Logger?.LogInformation($"Task created: {task.Title} by {userName}");

                return CreatedAtAction(nameof(GetTaskById), new { taskId = task.Id }, task);
            }, "CreateTask");
        }

        [HttpGet("{taskId:guid}")]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(taskId, "Task ID");
                if (!isValid) return err;

                var task = await _taskService.GetTaskByIdAsync(taskId);

                var (exists, notFound) = ValidateNotNull(task, "Task", taskId);
                if (!exists) return notFound;

                return Ok(task);
            }, "GetTaskById");
        }

        [HttpGet("project/{projectId:guid}")]
        public async Task<IActionResult> GetTasksByProject(Guid projectId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(projectId, "Project ID");
                if (!isValid) return err;

                var tasks = await _taskService.GetTasksByProjectAsync(projectId);
                Logger?.LogInformation($"Retrieved tasks for project {projectId}");
                return Ok(tasks);
            }, "GetTasksByProject");
        }

        [HttpGet("{taskId:guid}/assignments")]
        public async Task<IActionResult> GetTaskWithAssignments(Guid taskId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(taskId, "Task ID");
                if (!isValid) return err;

                var task = await _taskService.GetTaskWithAssignmentsAsync(taskId);

                var (exists, notFound) = ValidateNotNull(task, "Task", taskId);
                if (!exists) return notFound;

                return Ok(task);
            }, "GetTaskWithAssignments");
        }

        [HttpPut("{taskId:guid}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotEmptyGuid(taskId, "Task ID");
                if (!isValid1) return err1;

                var (isValid2, err2) = ValidateNotNull(dto, "Task data");
                if (!isValid2) return err2;

                if (string.IsNullOrWhiteSpace(dto.Title) && dto.Title != null)
                    return BadRequest("Task title cannot be empty");

                var userId = GetUserId();
                var userName = GetUserName();
                var userEmail = GetUserEmail();

                await _taskService.UpdateTaskAsync(taskId, dto);

                // Log activity for task update
                var activity = new ActivityLogDto
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    ActionType = "Updated",
                    ResourceType = "Task",
                    ResourceId = taskId.ToString(),
                    ResourceName = dto.Title ?? "Task",
                    Description = $"Task updated by {userName}",
                    Timestamp = DateTime.UtcNow,
                    ChangeDetails = $"{{\"Priority\": \"{dto.Priority}\", \"Status\": \"{dto.Status}\"}}"
                };

                await _collaborationService.LogActivityAsync(activity);
                Logger?.LogInformation($"Task {taskId} updated by {userName}");

                return NoContent();
            }, "UpdateTask");
        }

        [HttpDelete("{taskId:guid}")]
        [Authorize(Roles = "Admin,Mentor,Manager,Intern")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(taskId, "Task ID");
                if (!isValid) return err;

                await _taskService.DeleteTaskAsync(taskId);
                Logger?.LogInformation($"Task {taskId} deleted");
                return NoContent();
            }, "DeleteTask");
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin,Mentor,Manager,Intern")]
        public async Task<IActionResult> AssignTaskToInterns([FromBody] AssignTaskDto dto)
        {
            return await SafeExecute(async () =>
            {
                var (isValid1, err1) = ValidateNotNull(dto, "Assignment data");
                if (!isValid1) return err1;

                if (!dto.InternIds?.Any() ?? true)
                    return BadRequest("Please provide at least one intern to assign");

                var assignments = await _taskService.AssignTaskToInternsAsync(dto);
                Logger?.LogInformation($"Task assigned to {dto.InternIds.Count()} interns");
                return Ok(assignments);
            }, "AssignTaskToInterns");
        }

        [HttpGet("assigned/{internId:guid}")]
        [Authorize(Roles = "Admin,Manager,Mentor,Intern")]
        public async Task<IActionResult> GetTasksAssignedToIntern(Guid internId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(internId, "Intern ID");
                if (!isValid) return err;

                Logger?.LogInformation($"[TaskController] ========== START GetTasksAssignedToIntern ==========");
                Logger?.LogInformation($"[TaskController] Endpoint: GET /api/tasks/assigned/{internId}");
                Logger?.LogInformation($"[TaskController] Request received for internId (GUID): {internId}");
                
                var tasks = await _taskService.GetTasksAssignedToInternAsync(internId);
                Logger?.LogInformation($"[TaskController] Retrieved {tasks.Count()} tasks for intern {internId}");
                
                if (!tasks.Any())
                {
                    Logger?.LogWarning($"[TaskController] ⚠️ No tasks found for intern {internId}");
                }
                
                Logger?.LogInformation($"[TaskController] ========== END GetTasksAssignedToIntern ==========");
                return Ok(tasks);
            }, "GetTasksAssignedToIntern");
        }

        // Fallback route without GUID constraint for debugging
        [HttpGet("assigned-debug/{internId}")]
        [Authorize(Roles = "Admin,Manager,Mentor,Intern")]
        public async Task<IActionResult> GetTasksAssignedToInternDebug(string internId)
        {
            return await SafeExecute(async () =>
            {
                Logger?.LogInformation($"[TaskController] ========== DEBUG ENDPOINT ==========");
                Logger?.LogInformation($"[TaskController] Endpoint: GET /api/tasks/assigned-debug/{internId}");
                Logger?.LogInformation($"[TaskController] String internId received: {internId}");
                
                // Try to parse as GUID
                if (!Guid.TryParse(internId, out var parsedGuid))
                {
                    Logger?.LogError($"[TaskController] ❌ Could not parse '{internId}' as GUID");
                    return BadRequest(new { error = $"Invalid GUID format: {internId}" });
                }
                
                Logger?.LogInformation($"[TaskController] Parsed GUID: {parsedGuid}");
                var tasks = await _taskService.GetTasksAssignedToInternAsync(parsedGuid);
                Logger?.LogInformation($"[TaskController] DEBUG: Retrieved {tasks.Count()} tasks");
                
                return Ok(tasks);
            }, "GetTasksAssignedToInternDebug");
        }

        /// <summary>
        /// DIAGNOSTIC ENDPOINT - Shows raw database query results for debugging
        /// </summary>
        [HttpGet("assigned-raw-diagnostic/{internId}")]
        [Authorize(Roles = "Admin,Manager,Mentor,Intern")]
        public async Task<IActionResult> GetTasksRawDiagnostic(string internId)
        {
            return await SafeExecute(async () =>
            {
                Logger?.LogInformation($"[TaskController] ╔══════════════════════════════════════════════════════════╗");
                Logger?.LogInformation($"[TaskController] ║  RAW DIAGNOSTIC ENDPOINT                                ║");
                Logger?.LogInformation($"[TaskController] ╚══════════════════════════════════════════════════════════╝");
                Logger?.LogInformation($"[TaskController] Received string internId: '{internId}' (Type: {internId?.GetType().Name})");
                Logger?.LogInformation($"[TaskController] String length: {internId?.Length}");
                
                // Try to parse as GUID
                if (!Guid.TryParse(internId, out var parsedGuid))
                {
                    Logger?.LogError($"[TaskController] ❌ Could not parse '{internId}' as GUID");
                    return BadRequest(new { error = $"Invalid GUID format: {internId}", receivedValue = internId, type = internId?.GetType().Name });
                }
                
                Logger?.LogInformation($"[TaskController] Parsed GUID: {parsedGuid}");
                Logger?.LogInformation($"[TaskController] Parsed GUID (uppercase): {parsedGuid.ToString().ToUpper()}");
                
                var diagnosticData = await _taskService.GetRawDiagnosticDataAsync(parsedGuid);
                
                return Ok(diagnosticData);
            }, "GetTasksRawDiagnostic");
        }

        [HttpGet("assigned/me/current")]
        [Authorize(Roles = "Intern, Admin,Manager,Mentor")]
        public async Task<IActionResult> GetMyAssignedTasks()
        {
            return await SafeExecute(async () =>
            {
                var internId = GetUserId();
                var tasks = await _taskService.GetTasksAssignedToInternAsync(internId);
                Logger?.LogInformation($"Retrieved assigned tasks for intern {internId}");
                return Ok(tasks);
            }, "GetMyAssignedTasks");
        }

        [HttpGet("completed/{internId:guid}")]
        [Authorize(Roles = "Admin,Manager,Mentor")]
        public async Task<IActionResult> GetCompletedTasksForIntern(Guid internId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(internId, "Intern ID");
                if (!isValid) return err;

                var tasks = await _taskService.GetCompletedTasksForInternAsync(internId);
                Logger?.LogInformation($"Retrieved completed tasks for intern {internId}");
                return Ok(tasks);
            }, "GetCompletedTasksForIntern");
        }

        [HttpGet("completed/me/current")]
        [Authorize(Roles = "Intern")]
        public async Task<IActionResult> GetMyCompletedTasks()
        {
            return await SafeExecute(async () =>
            {
                var internId = GetUserId();
                var tasks = await _taskService.GetCompletedTasksForInternAsync(internId);
                Logger?.LogInformation($"Retrieved completed tasks for intern {internId}");
                return Ok(tasks);
            }, "GetMyCompletedTasks");
        }
    }
}
