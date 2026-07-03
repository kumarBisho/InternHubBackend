using InternMS.Api.DTOs.Search;
using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SearchService> _logger;

        public SearchService(AppDbContext db, ILogger<SearchService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PaginatedSearchResultsDto<TaskSearchResultDto>> SearchTasksAsync(TaskSearchRequestDto request)
        {
            try
            {
                var query = _db.ProjectTasks.AsQueryable();

                // Text search
                if (!string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    var searchQuery = request.SearchQuery.ToLower();
                    query = query.Where(t => t.Title.ToLower().Contains(searchQuery));
                }

                // Status filter
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<ProjectTaskStatus>(request.Status, true, out var status))
                    {
                        query = query.Where(t => t.Status == status);
                    }
                }

                // Priority filter
                if (!string.IsNullOrWhiteSpace(request.Priority))
                {
                    if (Enum.TryParse<Priority>(request.Priority, true, out var priority))
                    {
                        query = query.Where(t => t.Priority == priority);
                    }
                }

                // Assignee filter
                if (!string.IsNullOrWhiteSpace(request.AssigneeId) && Guid.TryParse(request.AssigneeId, out var assigneeId))
                {
                    query = query.Where(t => t.Assignments.Any(a => a.InternId == assigneeId));
                }

                // Project filter
                if (!string.IsNullOrWhiteSpace(request.ProjectId) && Guid.TryParse(request.ProjectId, out var projectId))
                {
                    query = query.Where(t => t.ProjectId == projectId);
                }

                // Date range filter
                if (request.StartDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
                }

                // Sorting
                query = ApplySorting(query, request.SortBy, request.SortDescending);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(t => new TaskSearchResultDto
                    {
                        TaskId = t.Id.ToString(),
                        Title = t.Title,
                        Description = "",
                        Status = t.Status.ToString(),
                        Priority = t.Priority.ToString(),
                        ProjectId = t.ProjectId.ToString(),
                        ProjectTitle = t.Project.Title,
                        EndDate = t.EndDate,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedSearchResultsDto<TaskSearchResultDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching tasks: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedSearchResultsDto<ProjectSearchResultDto>> SearchProjectsAsync(ProjectSearchRequestDto request)
        {
            try
            {
                var query = _db.Projects
                    .Include(p => p.Tasks)
                    .AsQueryable();

                // Text search
                if (!string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    var searchQuery = request.SearchQuery.ToLower();
                    query = query.Where(p => p.Title.ToLower().Contains(searchQuery) ||
                                            (p.Description != null && p.Description.ToLower().Contains(searchQuery)));
                }

                // Status filter
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
                    {
                        query = query.Where(p => p.Status == status);
                    }
                }

                // Date range filter
                if (request.StartDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt <= request.EndDate.Value);
                }

                // Tech stack filter (partial match)
                if (!string.IsNullOrWhiteSpace(request.TechStack))
                {
                    var techQuery = request.TechStack.ToLower();
                    query = query.Where(p => p.TechStack != null && p.TechStack.ToLower().Contains(techQuery));
                }

                // Sorting
                query = ApplyProjectSorting(query, request.SortBy, request.SortDescending);

                // Get total count
                var totalCount = await query.CountAsync();

                // Pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProjectSearchResultDto
                    {
                        ProjectId = p.Id.ToString(),
                        Title = p.Title,
                        Description = p.Description ?? "",
                        Status = p.Status.ToString(),
                        Progress = p.Progress,
                        TotalTasks = p.Tasks.Count,
                        CompletedTasks = p.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed),
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedSearchResultsDto<ProjectSearchResultDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching projects: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedSearchResultsDto<UserSearchResultDto>> SearchUsersAsync(UserSearchRequestDto request)
        {
            try
            {
                var query = _db.Users.AsQueryable();

                // Text search
                if (!string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    var searchQuery = request.SearchQuery.ToLower();
                    query = query.Where(u => u.FirstName.ToLower().Contains(searchQuery) ||
                                           u.LastName.ToLower().Contains(searchQuery) ||
                                           u.Email.ToLower().Contains(searchQuery));
                }

                // Role filter
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == request.Role));
                }

                // Active status filter
                if (request.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == request.IsActive.Value);
                }

                // Email confirmed filter
                if (request.EmailConfirmed.HasValue)
                {
                    query = query.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value);
                }

                // Sorting
                query = ApplyUserSorting(query, request.SortBy, request.SortDescending);

                // Get total count
                var totalCount = await query.CountAsync();

                // Pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserSearchResultDto
                    {
                        UserId = u.Id.ToString(),
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.UserRoles.FirstOrDefault().Role.Name ?? "Unknown",
                        IsActive = u.IsActive,
                        EmailConfirmed = u.EmailConfirmed,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedSearchResultsDto<UserSearchResultDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching users: {ex.Message}");
                throw;
            }
        }

        public async Task<GlobalSearchResultDto> GlobalSearchAsync(GlobalSearchRequestDto request)
        {
            try
            {
                var result = new GlobalSearchResultDto();
                var searchQuery = request.SearchQuery.ToLower();

                if (request.SearchIn.Contains("tasks"))
                {
                    var taskRequest = new TaskSearchRequestDto
                    {
                        SearchQuery = request.SearchQuery,
                        PageSize = request.PageSize
                    };
                    var taskResults = await SearchTasksAsync(taskRequest);
                    result.Tasks = taskResults.Items;
                }

                if (request.SearchIn.Contains("projects"))
                {
                    var projectRequest = new ProjectSearchRequestDto
                    {
                        SearchQuery = request.SearchQuery,
                        PageSize = request.PageSize
                    };
                    var projectResults = await SearchProjectsAsync(projectRequest);
                    result.Projects = projectResults.Items;
                }

                if (request.SearchIn.Contains("users"))
                {
                    var userRequest = new UserSearchRequestDto
                    {
                        SearchQuery = request.SearchQuery,
                        PageSize = request.PageSize
                    };
                    var userResults = await SearchUsersAsync(userRequest);
                    result.Users = userResults.Items;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in global search: {ex.Message}");
                throw;
            }
        }

        private IQueryable<ProjectTask> ApplySorting(IQueryable<ProjectTask> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "enddate" => descending ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
                _ => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
            };
        }

        private IQueryable<Project> ApplyProjectSorting(IQueryable<Project> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "title" => descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
                "status" => descending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                "progress" => descending ? query.OrderByDescending(p => p.Progress) : query.OrderBy(p => p.Progress),
                _ => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };
        }

        private IQueryable<User> ApplyUserSorting(IQueryable<User> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "createdat" => descending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                "lastname" => descending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                _ => descending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName)
            };
        }
    }
}
