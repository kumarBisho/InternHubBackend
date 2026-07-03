namespace InternMS.Api.DTOs.Search;

/// <summary>
/// Request DTO for task search with advanced filters
/// </summary>
public class TaskSearchRequestDto
{
    public string? SearchQuery { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? AssigneeId { get; set; }
    public string? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Result DTO for task search
/// </summary>
public class TaskSearchResultDto
{
    public string TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string ProjectId { get; set; }
    public string ProjectTitle { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for project search with advanced filters
/// </summary>
public class ProjectSearchRequestDto
{
    public string? SearchQuery { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TechStack { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Result DTO for project search
/// </summary>
public class ProjectSearchResultDto
{
    public string ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public int Progress { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for user search with advanced filters
/// </summary>
public class UserSearchRequestDto
{
    public string? SearchQuery { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "FirstName";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Result DTO for user search
/// </summary>
public class UserSearchResultDto
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Paginated search results
/// </summary>
public class PaginatedSearchResultsDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Global search request combining all search types
/// </summary>
public class GlobalSearchRequestDto
{
    public string SearchQuery { get; set; } = string.Empty;
    public List<string> SearchIn { get; set; } = new() { "tasks", "projects", "users" };
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Global search results
/// </summary>
public class GlobalSearchResultDto
{
    public List<TaskSearchResultDto> Tasks { get; set; } = new();
    public List<ProjectSearchResultDto> Projects { get; set; } = new();
    public List<UserSearchResultDto> Users { get; set; } = new();
}
