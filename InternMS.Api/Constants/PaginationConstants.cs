namespace InternMS.Api.Constants;

/// <summary>
/// Constants for pagination configuration across the API
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Default page size for list endpoints (20 items per page)
    /// </summary>
    public const int DEFAULT_PAGE_SIZE = 20;

    /// <summary>
    /// Maximum page size allowed (prevents performance issues)
    /// </summary>
    public const int MAX_PAGE_SIZE = 100;

    /// <summary>
    /// Minimum page size allowed
    /// </summary>
    public const int MIN_PAGE_SIZE = 1;

    /// <summary>
    /// Default page number (starts at 1)
    /// </summary>
    public const int DEFAULT_PAGE_NUMBER = 1;

    /// <summary>
    /// Page size for small lists (recent activity, notifications)
    /// </summary>
    public const int SMALL_PAGE_SIZE = 10;

    /// <summary>
    /// Page size for large lists (analytics, dashboards)
    /// </summary>
    public const int LARGE_PAGE_SIZE = 50;

    /// <summary>
    /// Page size for bulk operations
    /// </summary>
    public const int BULK_PAGE_SIZE = 100;
}

/// <summary>
/// Constants for sorting and filtering
/// </summary>
public static class SortingConstants
{
    /// <summary>
    /// Default sort order (newest first)
    /// </summary>
    public const string DEFAULT_SORT_BY = "CreatedAt";

    /// <summary>
    /// Default sort direction (descending = newest first)
    /// </summary>
    public const string DEFAULT_SORT_DIRECTION = "desc";

    /// <summary>
    /// Allowed sort directions
    /// </summary>
    public static readonly string[] ALLOWED_SORT_DIRECTIONS = { "asc", "desc" };
}
