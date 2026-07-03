using InternMS.Api.Constants;

namespace InternMS.Api.Utils;

/// <summary>
/// Helper class for pagination calculations and validation
/// Centralizes pagination logic to reduce code duplication
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Validates and normalizes pagination parameters
    /// </summary>
    /// <param name="pageNumber">Requested page number (1-based indexing)</param>
    /// <param name="pageSize">Requested page size</param>
    /// <returns>Tuple of validated (pageNumber, pageSize)</returns>
    public static (int pageNumber, int pageSize) ValidateAndNormalize(int pageNumber, int pageSize)
    {
        // Validate page number
        if (pageNumber < 1)
            pageNumber = PaginationConstants.DEFAULT_PAGE_NUMBER;

        // Validate and cap page size
        if (pageSize < PaginationConstants.MIN_PAGE_SIZE)
            pageSize = PaginationConstants.DEFAULT_PAGE_SIZE;
        else if (pageSize > PaginationConstants.MAX_PAGE_SIZE)
            pageSize = PaginationConstants.MAX_PAGE_SIZE;

        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Calculates the number of items to skip for pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Number of items to skip</returns>
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Calculates total number of pages
    /// </summary>
    /// <param name="totalCount">Total number of items</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Total number of pages</returns>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (totalCount == 0)
            return 0;

        return (totalCount + pageSize - 1) / pageSize; // Ceiling division
    }

    /// <summary>
    /// Determines if there is a next page
    /// </summary>
    /// <param name="pageNumber">Current page number</param>
    /// <param name="totalPages">Total pages</param>
    /// <returns>True if next page exists</returns>
    public static bool HasNextPage(int pageNumber, int totalPages)
    {
        return pageNumber < totalPages;
    }

    /// <summary>
    /// Determines if there is a previous page
    /// </summary>
    /// <param name="pageNumber">Current page number</param>
    /// <returns>True if previous page exists</returns>
    public static bool HasPreviousPage(int pageNumber)
    {
        return pageNumber > 1;
    }

    /// <summary>
    /// Creates a pagination request with validated parameters
    /// </summary>
    /// <param name="pageNumber">Requested page number</param>
    /// <param name="pageSize">Requested page size</param>
    /// <returns>PaginationRequest with validated parameters</returns>
    public static PaginationRequest CreateValidatedRequest(int pageNumber, int pageSize)
    {
        var (validatedPageNumber, validatedPageSize) = ValidateAndNormalize(pageNumber, pageSize);
        return new PaginationRequest
        {
            PageNumber = validatedPageNumber,
            PageSize = validatedPageSize
        };
    }
}

/// <summary>
/// Represents pagination request parameters
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based indexing). Defaults to 1 if not provided or invalid.
    /// </summary>
    public int PageNumber { get; set; } = PaginationConstants.DEFAULT_PAGE_NUMBER;

    /// <summary>
    /// Number of items per page. Defaults to 20 if not provided, capped at 100.
    /// </summary>
    public int PageSize { get; set; } = PaginationConstants.DEFAULT_PAGE_SIZE;

    /// <summary>
    /// Optional search/filter term
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional sort field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Optional sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; }
}
