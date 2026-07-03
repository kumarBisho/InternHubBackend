using AutoMapper;
using InternMS.Api.Services;
using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using InternMS.Api.DTOs.Projects;
using InternMS.Api.DTOs.Common;
using InternMS.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public ProjectService(AppDbContext db, INotificationService notificationService, IMapper mapper)
        {
            _db = db;
             _notificationService = notificationService;
             _mapper = mapper;
        }

        public async Task<Project> CreateProjectAsync(Guid creatorId, CreateProjectDto dto)
        {
            // Parse status from DTO
            ProjectStatus status = ProjectStatus.Active;
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<ProjectStatus>(dto.Status, true, out var parsedStatus))
                {
                    status = parsedStatus;
                }
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate.HasValue 
                    ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) 
                    : null,
                EndDate = dto.EndDate.HasValue 
                    ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) 
                    : null,
                Status = status,
                Progress = dto.Progress,
                TechStack = dto.TechStack,
                RepositoryUrl = dto.RepositoryUrl,
                DocumentationUrl = dto.DocumentationUrl,
                DemoUrl = dto.DemoUrl,
                CreatedById = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Send notification to creator about successful project creation
            await _notificationService.CreateNotificationAsync(
                creatorId,
                "Project Created", 
                $"Your project '{project.Title}' has been successfully created",
                NotificationType.ProjectCreated,
                relatedEntityId: project.Id,
                relatedEntityType: "Project",
                actionUrl: $"/projects/{project.Id}",
                description: dto.Description
            );

            return project;
        }
        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            // Load project with only the data we need to avoid circular references
            var project = await _db.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            // Load assignments with Intern and Mentor user data using AsNoTracking to avoid circular refs
            var assignments = await _db.ProjectAssignments
                .AsNoTracking()
                .Where(a => a.ProjectId == id)
                .Include(a => a.Intern)
                .Include(a => a.Mentor)
                .ToListAsync();

            project.Assignments = assignments;

            // Load tasks separately
            var tasks = await _db.ProjectTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == id)
                .ToListAsync();

            project.Tasks = tasks;

            return project;
        }

        public async Task<IEnumerable<Project>> GetProjectsForUserAsync(Guid userId, string role)
        {
            if (role == "Admin" || role == "Manager")
            {
                return await _db.Projects
                    .Include(p => p.Assignments)
                    .Include(p => p.Tasks)
                    .ToListAsync();
            }
            
            if (role == "Mentor")
            {
                return await _db.Projects
                    .Include(p => p.Assignments)
                    .Include(p => p.Tasks)
                    .Where(p => p.Assignments.Any(a => a.MentorId == userId))
                    .ToListAsync();
            }

            // Intern
            return await _db.Projects
                .Include(p => p.Assignments)
                .Include(p => p.Tasks)
                .Where(p => p.Assignments.Any(a => a.InternId == userId))
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated projects for user based on role
        /// </summary>
        public async Task<PaginatedResponse<ProjectDto>> GetProjectsForUserPaginatedAsync(Guid userId, string role, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            IQueryable<Project> query;

            if (role == "Admin" || role == "Manager")
            {
                query = _db.Projects
                    .Include(p => p.Assignments)
                    .Include(p => p.Tasks);
            }
            else if (role == "Mentor")
            {
                query = _db.Projects
                    .Include(p => p.Assignments)
                    .Include(p => p.Tasks)
                    .Where(p => p.Assignments.Any(a => a.MentorId == userId));
            }
            else // Intern
            {
                query = _db.Projects
                    .Include(p => p.Assignments)
                    .Include(p => p.Tasks)
                    .Where(p => p.Assignments.Any(a => a.InternId == userId));
            }

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var projects = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            var projectDtos = _mapper.Map<List<ProjectDto>>(projects);

            return PaginatedResponse<ProjectDto>.Create(projectDtos, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<Project> UpdateProjectAsync(Guid projectId, CreateProjectDto dto)
        {
            var project = await _db.Projects
                .Include(p => p.Assignments)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception("Project not found");

            var statusChanged = false;
            var oldStatus = project.Status;

            project.Title = dto.Title;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate.HasValue 
                ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) 
                : null;
            project.EndDate = dto.EndDate.HasValue 
                ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) 
                : null;
            project.Progress = dto.Progress;
            project.TechStack = dto.TechStack;
            project.RepositoryUrl = dto.RepositoryUrl;
            project.DocumentationUrl = dto.DocumentationUrl;
            project.DemoUrl = dto.DemoUrl;

            // Parse and set status from DTO
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<ProjectStatus>(dto.Status, true, out var parsedStatus))
                {
                    if (project.Status != parsedStatus)
                    {
                        statusChanged = true;
                    }
                    project.Status = parsedStatus;
                }
            }

            await _db.SaveChangesAsync();

            // Notify team members about the update
            var teamMemberIds = project.Assignments
                .Select(a => new[] { (Guid?)a.InternId, a.MentorId })
                .SelectMany(x => x)
                .Where(id => id != null)
                .Select(id => id!.Value)
                .Distinct();

            foreach (var memberId in teamMemberIds)
            {
                var notificationType = statusChanged ? NotificationType.StatusChanged : NotificationType.ProjectUpdated;
                var title = statusChanged ? $"Project Status Changed" : "Project Updated";
                var message = statusChanged 
                    ? $"Project '{project.Title}' status changed from {oldStatus} to {project.Status}"
                    : $"Project '{project.Title}' has been updated";

                await _notificationService.CreateNotificationAsync(
                    memberId,
                    title,
                    message,
                    notificationType,
                    relatedEntityId: projectId,
                    relatedEntityType: "Project",
                    actionUrl: $"/projects/{projectId}",
                    description: project.Description
                );
            }

            return project;
        }

        public async Task<Project> PartialUpdateProjectAsync(Guid projectId, PartialUpdateProjectDto dto)
        {
            var project = await _db.Projects
                .Include(p => p.Assignments)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception("Project not found");

            var statusChanged = false;
            var oldStatus = project.Status;

            if (dto.Title != null)
                project.Title = dto.Title;

            if (dto.Description != null)
                project.Description = dto.Description;

            if (dto.StartDate.HasValue)
                project.StartDate = DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc);

            if (dto.EndDate.HasValue)
                project.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);

            if (dto.Progress.HasValue)
                project.Progress = dto.Progress.Value;

            if (dto.RepositoryUrl != null)
                project.RepositoryUrl = dto.RepositoryUrl;

            if (dto.DocumentationUrl != null)
                project.DocumentationUrl = dto.DocumentationUrl;

            if (dto.DemoUrl != null)
                project.DemoUrl = dto.DemoUrl;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<ProjectStatus>(dto.Status, true, out var parsedStatus))
                {
                    if (project.Status != parsedStatus)
                    {
                        statusChanged = true;
                    }
                    project.Status = parsedStatus;
                }
            }

            await _db.SaveChangesAsync();

            // Notify team members about the update
            if (statusChanged)
            {
                var teamMemberIds = project.Assignments
                    .Select(a => new[] { (Guid?)a.InternId, a.MentorId })
                    .SelectMany(x => x)
                    .Where(id => id != null)
                    .Select(id => id!.Value)
                    .Distinct();

                foreach (var memberId in teamMemberIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        memberId,
                        "Project Status Changed",
                        $"Project '{project.Title}' status has been updated to {project.Status}",
                        NotificationType.StatusChanged,
                        actionUrl: $"/projects/{projectId}",
                        relatedEntityId: projectId,
                        relatedEntityType: "Project"
                    );
                }
            }

            return project;
        }

        public async Task AssignProjectAsync(Guid projectId, AssignProjectDto dto)
        {
            var exists = await _db.ProjectAssignments
                .AnyAsync(a => a.ProjectId == projectId && a.InternId == dto.InternId);

            if (exists)
                throw new Exception("Intern is already assigned!");

            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                throw new Exception("Project not found");

            var assignment = new ProjectAssignment
            {
                ProjectId = projectId,
                InternId = dto.InternId,
                MentorId = dto.MentorId,
                AssignedAt = DateTime.UtcNow
            };

            _db.ProjectAssignments.Add(assignment);
            await _db.SaveChangesAsync();

            try
            {
                // Notify intern about project assignment with metadata
                await _notificationService.CreateNotificationAsync(
                    dto.InternId,
                    "Project Assigned",
                    $"You have been assigned to project: {project.Title}",
                    NotificationType.ProjectAssignment,
                    triggeredByUserId: dto.MentorId,
                    relatedEntityId: projectId,
                    relatedEntityType: "Project",
                    actionUrl: $"/projects/{projectId}",
                    description: project.Description
                );

                // Notify mentor about the assignment only if mentor was assigned
                if (dto.MentorId.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        dto.MentorId.Value,
                        "Intern Assigned to Project",
                        $"An intern has been assigned to your project: {project.Title}",
                        NotificationType.ProjectAssignment,
                        triggeredByUserId: dto.MentorId.Value,
                        relatedEntityId: projectId,
                        relatedEntityType: "Project",
                        actionUrl: $"/projects/{projectId}"
                    );
                }
            }
            catch (Exception ex)
            {
                // Log notification error but don't fail the assignment
                Console.WriteLine($"Notification error during assignment: {ex.Message}");
            }
        }

        public async Task<ProjectUpdateDto> AddProjectUpdateAsync(Guid projectId, Guid authorId, CreateProjectUpdateDto dto)
        {
            var update = new ProjectUpdate
            {
                ProjectId = projectId,
                AuthorId = authorId,
                Status = Enum.Parse<ProjectStatus>(dto.Status),
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow  
            };

            _db.ProjectUpdates.Add(update);

            // Update project status
            var project = await _db.Projects
                .Include(p => p.Assignments)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception("Project not found");

            project.Status = update.Status;

            await _db.SaveChangesAsync();

            // Get all team members (interns and mentors) assigned to the project
            var teamMemberIds = project.Assignments
                .Select(a => new[] { (Guid?)a.InternId, a.MentorId })
                .SelectMany(x => x)
                .Where(id => id != null && id != authorId)
                .Select(id => id!.Value)
                .Distinct();

            // Notify all team members about the project update
            foreach (var memberId in teamMemberIds)
            {
                await _notificationService.CreateNotificationAsync(
                    memberId,
                    "Project Updated",
                    $"Project '{project.Title}' status has been updated to {update.Status}",
                    NotificationType.ProjectUpdated,
                    triggeredByUserId: authorId,
                    relatedEntityId: projectId,
                    relatedEntityType: "Project",
                    actionUrl: $"/projects/{projectId}",
                    description: update.Comment
                );
            }

            return _mapper.Map<ProjectUpdateDto>(update);
        }

        public async Task DeleteProjectAsync(Guid id)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                throw new Exception("Project not found");
            }

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }
    }
}