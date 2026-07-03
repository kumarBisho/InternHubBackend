namespace InternMS.Api.DTOs.Collaboration
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string ActionType { get; set; } // "Created", "Updated", "Completed", "Assigned", "Commented"
        public string ResourceType { get; set; } // "Task", "Project", "User"
        public string ResourceId { get; set; } // Stored as string to support different resource types
        public string ResourceName { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string ChangeDetails { get; set; } // JSON of what changed
    }

    public class ActivityLogRequestDto
    {
        public Guid UserId { get; set; }
        public string ActionType { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string Description { get; set; }
        public string ChangeDetails { get; set; }
    }

    public class ActivityLogFilterDto
    {
        public Guid? UserId { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public string ActionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PaginatedActivityLogDto
    {
        public List<ActivityLogDto> Activities { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
