using AutoMapper;
using AutoMapper.QueryableExtensions;
using InternMS.Api.DTOs.Tasks;
using InternMS.Api.DTOs.Common;
using InternMS.Api.Utils;
using InternMS.Domain.Entities;
using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskService> _logger;

        public TaskService(AppDbContext db, IMapper mapper, INotificationService notificationService, ILogger<TaskService> logger)
        {
            _db = db;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid createdById)
        {
            var project = await _db.Projects
                .Include(p => p.Assignments)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            
            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            // Validate that Title is not empty
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Task title cannot be empty.");

            // Validate EndDate is in the future
            if (dto.EndDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Task end date must be in the future.");

            var task = _mapper.Map<ProjectTask>(dto);
            task.CreatedById = createdById;
            _db.ProjectTasks.Add(task);
            await _db.SaveChangesAsync();

            // Notify project team about new task
            var teamMemberIds = project.Assignments
                .Select(a => new[] { (Guid?)a.InternId, a.MentorId })
                .SelectMany(x => x)
                .Where(id => id != null && id != createdById)
                .Select(id => id!.Value)
                .Distinct();

            if (teamMemberIds.Any())
            {
                _logger.LogInformation($"Notifying {teamMemberIds.Count()} team members about new task: {task.Title}");
                foreach (var memberId in teamMemberIds)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            memberId,
                            "New Task Created",
                            $"A new task '{task.Title}' has been created in project '{project.Title}'",
                            NotificationType.StatusChanged,
                            triggeredByUserId: createdById,
                            relatedEntityId: task.Id,
                            relatedEntityType: "Task",
                            actionUrl: $"/projects/{dto.ProjectId}/tasks/{task.Id}"
                        );
                        _logger.LogInformation($"Notification sent to user {memberId} about new task");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send notification to user {memberId}: {ex.Message}");
                    }
                }
            }
            else
            {
                _logger.LogInformation($"No team members to notify for task: {task.Title}");
            }

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<IEnumerable<TaskListDto>> GetTasksByProjectAsync(Guid projectId)
        {
            return await _db.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.EndDate)
                .ProjectTo<TaskListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Gets tasks for a project with pagination
        /// </summary>
        public async Task<PaginatedResponse<TaskListDto>> GetTasksByProjectPaginatedAsync(Guid projectId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.EndDate);

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var tasks = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ProjectTo<TaskListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PaginatedResponse<TaskListDto>.Create(tasks, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _db.ProjectTasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            return task == null ? null : _mapper.Map<TaskDto>(task);
        }

        public async Task UpdateTaskAsync(Guid taskId, UpdateTaskDto dto)
        {
            var task = await _db.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var oldStatus = task.Status;
            var oldPriority = task.Priority;

            _mapper.Map(dto, task);
            await _db.SaveChangesAsync();

            // Get all interns assigned to this task
            var assignedInternIds = task.Assignments
                .Select(a => a.InternId)
                .Distinct();

            // Notify assigned interns about task update
            var notificationType = NotificationType.TaskUpdated;
            var title = "Task Updated";
            var message = $"Task '{task.Title}' has been updated";

            if (oldStatus != task.Status)
            {
                if (task.Status == ProjectTaskStatus.Completed)
                {
                    notificationType = NotificationType.TaskCompleted;
                    title = "Task Completed";
                    message = $"Task '{task.Title}' has been marked as completed";
                }
                else
                {
                    notificationType = NotificationType.StatusChanged;
                    title = "Task Status Changed";
                    message = $"Task '{task.Title}' status changed from {oldStatus} to {task.Status}";
                }
            }
            else if (oldPriority != task.Priority)
            {
                notificationType = NotificationType.PriorityChanged;
                title = "Task Priority Changed";
                message = $"Task '{task.Title}' priority changed from {oldPriority} to {task.Priority}";
            }

            if (assignedInternIds.Any())
            {
                _logger.LogInformation($"Notifying {assignedInternIds.Count()} interns about task update: {task.Title}");
                foreach (var internId in assignedInternIds)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            internId,
                            title,
                            message,
                            notificationType,
                            relatedEntityId: taskId,
                            relatedEntityType: "Task",
                            actionUrl: $"/projects/{task.ProjectId}/tasks/{taskId}"
                        );
                        _logger.LogInformation($"Notification sent to intern {internId} about task update");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send notification to intern {internId}: {ex.Message}");
                    }
                }
            }
            else
            {
                _logger.LogInformation($"No interns assigned to task for notification: {task.Title}");
            }
        }

        public async Task DeleteTaskAsync(Guid taskId)
        {
            var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            _db.ProjectTasks.Remove(task);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskAssignmentDto>> AssignTaskToInternsAsync(AssignTaskDto dto)
        {
            var task = await _db.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == dto.TaskId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            // Get all interns assigned to the project
            var projectAssignments = await _db.ProjectAssignments
                .Where(pa => pa.ProjectId == task.ProjectId)
                .Select(pa => pa.InternId)
                .ToListAsync();

            // Validate that all requested interns are assigned to the project
            var invalidInterns = dto.InternIds
                .Where(internId => !projectAssignments.Contains(internId))
                .ToList();

            if (invalidInterns.Any())
                throw new InvalidOperationException($"Some interns are not assigned to this project.");

            // Check which interns already have this task assigned
            var existingAssignments = await _db.ProjectTaskAssignments
                .Where(pta => pta.TaskId == dto.TaskId)
                .Select(pta => pta.InternId)
                .ToListAsync();

            // Assign only new interns
            var newAssignments = new List<ProjectTaskAssignment>();
            foreach (var internId in dto.InternIds)
            {
                if (!existingAssignments.Contains(internId))
                {
                    newAssignments.Add(new ProjectTaskAssignment
                    {
                        TaskId = dto.TaskId,
                        InternId = internId,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            if (newAssignments.Any())
            {
                _db.ProjectTaskAssignments.AddRange(newAssignments);
                await _db.SaveChangesAsync();

                // Notify newly assigned interns
                _logger.LogInformation($"Notifying {newAssignments.Count()} interns about task assignment: {task.Title}");
                foreach (var assignment in newAssignments)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            assignment.InternId,
                            "Task Assigned",
                            $"You have been assigned to task: '{task.Title}'",
                            NotificationType.TaskAssigned,
                            relatedEntityId: task.Id,
                            relatedEntityType: "Task",
                            actionUrl: $"/projects/{task.ProjectId}/tasks/{task.Id}"
                        );
                        _logger.LogInformation($"Notification sent to intern {assignment.InternId} about task assignment");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send task assignment notification to intern {assignment.InternId}: {ex.Message}");
                    }
                }
            }

            // Return all assignments for this task
            var assignments = await _db.ProjectTaskAssignments
                .Where(pta => pta.TaskId == dto.TaskId)
                .Include(pta => pta.Intern)
                .ProjectTo<TaskAssignmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return assignments;
        }

        public async Task<IEnumerable<AssignedTaskDto>> GetTasksAssignedToInternAsync(Guid internId)
        {
            _logger.LogInformation($"[TaskService] ╔══════════════════════════════════════════════════════════╗");
            _logger.LogInformation($"[TaskService] ║  GetTasksAssignedToInternAsync - ProjectTaskAssignments  ║");
            _logger.LogInformation($"[TaskService] ╚══════════════════════════════════════════════════════════╝");
            _logger.LogInformation($"[TaskService] Input internId: {internId}");
            
            try
            {
                // DIRECT QUERY: Start from ProjectTaskAssignments table, filter by internId, then get related tasks
                var tasks = await _db.ProjectTaskAssignments
                    .Where(pta => pta.InternId == internId)
                    .Include(pta => pta.Task)
                        .ThenInclude(t => t.Assignments)
                            .ThenInclude(a => a.Intern)
                    .Select(pta => pta.Task)
                    .Distinct()
                    .ToListAsync();
                
                Console.WriteLine($"[TaskService] GetTasksAssignedToInternAsync - Found {tasks.Count} tasks assigned to intern {internId}");

                _logger.LogInformation($"[TaskService] ✓ Query executed successfully");
                _logger.LogInformation($"[TaskService] ✓ Found {tasks.Count} tasks assigned to intern {internId}");
                
                if (tasks.Count == 0)
                {
                    _logger.LogWarning($"[TaskService] ⚠️ No tasks assigned to intern: {internId}");
                    
                    // Debug: Show all assignments in database
                    var allAssignments = await _db.ProjectTaskAssignments
                        .Include(a => a.Intern)
                        .ToListAsync();
                    _logger.LogWarning($"[TaskService] DEBUG: Total assignments in DB: {allAssignments.Count}");
                    foreach (var a in allAssignments.Take(10))
                    {
                        _logger.LogInformation($"[TaskService]   - TaskId: {a.TaskId}, InternId: {a.InternId}, InternName: {a.Intern?.Email ?? "null"}");
                    }
                }
                
                // Map to DTO
                var assignedTaskDtos = _mapper.Map<List<AssignedTaskDto>>(tasks);
                
                foreach (var task in assignedTaskDtos)
                {
                    _logger.LogInformation($"[TaskService]   Task: {task.Id} | Title: {task.Title} | Status: {task.Status}");
                }
                
                _logger.LogInformation($"[TaskService] ╔══════════════════════════════════════════════════════════╗");
                _logger.LogInformation($"[TaskService] ║  ✓ RETURNING {assignedTaskDtos.Count} ASSIGNED TASKS               ║");
                _logger.LogInformation($"[TaskService] ╚══════════════════════════════════════════════════════════╝");
                
                return assignedTaskDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TaskService] ❌ EXCEPTION in GetTasksAssignedToInternAsync: {ex.GetType().Name}");
                _logger.LogError($"[TaskService] Message: {ex.Message}");
                _logger.LogError($"[TaskService] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// DIAGNOSTIC: Get raw data for debugging task assignment queries
        /// </summary>
        public async Task<object> GetRawDiagnosticDataAsync(Guid internId)
        {
            _logger.LogInformation($"[TaskService] ╔══════════════════════════════════════════════════════════╗");
            _logger.LogInformation($"[TaskService] ║  RAW DIAGNOSTIC - FULL QUERY ANALYSIS                   ║");
            _logger.LogInformation($"[TaskService] ╚══════════════════════════════════════════════════════════╝");
            _logger.LogInformation($"[TaskService] Input internId: {internId}");
            _logger.LogInformation($"[TaskService] Input internId (uppercase): {internId.ToString().ToUpper()}");

            try
            {
                // Step 1: Get ALL assignments from database
                var allAssignments = await _db.ProjectTaskAssignments.ToListAsync();
                _logger.LogInformation($"[TaskService] STEP 1: Total assignments in DB: {allAssignments.Count}");
                foreach (var assignment in allAssignments)
                {
                    _logger.LogInformation($"[TaskService]   - TaskId: {assignment.TaskId}, InternId: {assignment.InternId}, InternIdUpper: {assignment.InternId.ToString().ToUpper()}");
                }

                // Step 2: Try exact match
                var exactMatches = allAssignments.Where(a => a.InternId == internId).ToList();
                _logger.LogInformation($"[TaskService] STEP 2: Exact matches for {internId}: {exactMatches.Count}");

                // Step 3: Try case-insensitive string comparison
                var caseInsensitiveMatches = allAssignments.Where(a => a.InternId.ToString().Equals(internId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                _logger.LogInformation($"[TaskService] STEP 3: Case-insensitive string matches: {caseInsensitiveMatches.Count}");

                // Step 4: Get all unique InternIds in DB
                var uniqueInternIds = allAssignments.Select(a => a.InternId).Distinct().ToList();
                _logger.LogInformation($"[TaskService] STEP 4: Unique InternIds in DB: {uniqueInternIds.Count}");
                foreach (var id in uniqueInternIds)
                {
                    _logger.LogInformation($"[TaskService]   - InternId: {id}");
                }

                // Step 5: Check if internId is null or default
                _logger.LogInformation($"[TaskService] STEP 5: Input internId is null? {internId == null}");
                _logger.LogInformation($"[TaskService] STEP 5: Input internId is Guid.Empty? {internId == Guid.Empty}");

                // Step 6: Query using EF Core directly
                var efMatches = await _db.ProjectTaskAssignments
                    .Where(pta => pta.InternId == internId)
                    .ToListAsync();
                _logger.LogInformation($"[TaskService] STEP 6: EF Core query result: {efMatches.Count} matches");

                // Step 7: Get task count for those assignments
                var taskIds = efMatches.Select(a => a.TaskId).Distinct().ToList();
                var tasks = await _db.ProjectTasks
                    .Where(t => taskIds.Contains(t.Id))
                    .ToListAsync();
                _logger.LogInformation($"[TaskService] STEP 7: Tasks found for matched assignments: {tasks.Count}");

                var result = new
                {
                    inputInternId = internId,
                    inputInternIdUppercase = internId.ToString().ToUpper(),
                    step1_totalAssignmentsInDb = allAssignments.Count,
                    step1_allAssignments = allAssignments.Select(a => new 
                    { 
                        taskId = a.TaskId, 
                        internId = a.InternId,
                        internIdUppercase = a.InternId.ToString().ToUpper()
                    }).ToList(),
                    step2_exactMatches = exactMatches.Count,
                    step3_caseInsensitiveMatches = caseInsensitiveMatches.Count,
                    step4_uniqueInternIdsInDb = uniqueInternIds.Select(id => new { id, idUppercase = id.ToString().ToUpper() }).ToList(),
                    step5_isInputNull = internId == null,
                    step5_isInputGuidEmpty = internId == Guid.Empty,
                    step6_efQueryMatches = efMatches.Count,
                    step7_taskIds = taskIds,
                    step7_tasksFound = tasks.Count
                };

                _logger.LogInformation($"[TaskService] ╔══════════════════════════════════════════════════════════╗");
                _logger.LogInformation($"[TaskService] ║  DIAGNOSTIC COMPLETE - Result serialized above          ║");
                _logger.LogInformation($"[TaskService] ╚══════════════════════════════════════════════════════════╝");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TaskService] ❌ DIAGNOSTIC ERROR: {ex.GetType().Name}");
                _logger.LogError($"[TaskService] Message: {ex.Message}");
                _logger.LogError($"[TaskService] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets tasks assigned to an intern with pagination
        /// </summary>
        public async Task<PaginatedResponse<AssignedTaskDto>> GetTasksAssignedToInternPaginatedAsync(Guid internId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            // Query from ProjectTaskAssignments, filter by internId, then get tasks
            var query = _db.ProjectTaskAssignments
                .Where(pta => pta.InternId == internId)
                .Include(pta => pta.Task)
                    .ThenInclude(t => t.Assignments)
                        .ThenInclude(a => a.Intern)
                .Select(pta => pta.Task)
                .Distinct();

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var tasks = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ProjectTo<AssignedTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PaginatedResponse<AssignedTaskDto>.Create(tasks, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<IEnumerable<AssignedTaskDto>> GetCompletedTasksForInternAsync(Guid internId)
        {
            var tasks = await _db.ProjectTasks
                .Where(t => t.Assignments.Any(a => a.InternId == internId) && t.Status == ProjectTaskStatus.Completed)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Intern)
                .ProjectTo<AssignedTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return tasks;
        }

        /// <summary>
        /// Gets completed tasks for an intern with pagination
        /// </summary>
        public async Task<PaginatedResponse<AssignedTaskDto>> GetCompletedTasksForInternPaginatedAsync(Guid internId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.ProjectTasks
                .Where(t => t.Assignments.Any(a => a.InternId == internId) && t.Status == ProjectTaskStatus.Completed)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Intern);

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var tasks = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ProjectTo<AssignedTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PaginatedResponse<AssignedTaskDto>.Create(tasks, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<AssignedTaskDto?> GetTaskWithAssignmentsAsync(Guid taskId)
        {
            var task = await _db.ProjectTasks
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Intern)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            return task == null ? null : _mapper.Map<AssignedTaskDto>(task);
        }
    }
}
