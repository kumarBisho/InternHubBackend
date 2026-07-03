using InternMS.Api.DTOs.Common;
using InternMS.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Services.Pagination;

/// <summary>
/// Generic pagination service interface
/// Provides standardized pagination operations for all services
/// </summary>
/// <typeparam name="T">Entity type to paginate</typeparam>
/// <typeparam name="D">DTO type to return</typeparam>
public interface IPaginationService<T, D>
    where T : class
    where D : class
{
    /// <summary>
    /// Gets a paginated list of items
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response containing items and metadata</returns>
    Task<PaginatedResponse<D>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of filtered items
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response containing filtered items and metadata</returns>
    Task<PaginatedResponse<D>> GetPaginatedAsync(
        Func<IQueryable<T>, IQueryable<T>> predicate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic pagination service implementation
/// Centralizes pagination logic for consistent behavior across all services
/// </summary>
/// <typeparam name="T">Entity type to paginate</typeparam>
/// <typeparam name="D">DTO type to return</typeparam>
public class PaginationService<T, D> : IPaginationService<T, D>
    where T : class
    where D : class
{
    private readonly IQueryable<T> _queryable;
    private readonly Func<T, D> _selector;

    public PaginationService(IQueryable<T> queryable, Func<T, D> selector)
    {
        _queryable = queryable;
        _selector = selector;
    }

    /// <summary>
    /// Gets a paginated list of all items
    /// </summary>
    public async Task<PaginatedResponse<D>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await GetPaginatedInternalAsync(_queryable, pageNumber, pageSize, cancellationToken);
    }

    /// <summary>
    /// Gets a paginated list of filtered items
    /// </summary>
    public async Task<PaginatedResponse<D>> GetPaginatedAsync(
        Func<IQueryable<T>, IQueryable<T>> predicate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filteredQuery = predicate(_queryable);
        return await GetPaginatedInternalAsync(filteredQuery, pageNumber, pageSize, cancellationToken);
    }

    /// <summary>
    /// Internal method to handle pagination logic
    /// </summary>
    private async Task<PaginatedResponse<D>> GetPaginatedInternalAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Validate parameters
        var (validatedPageNumber, validatedPageSize) = 
            PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);

        // Get total count
        int totalCount = await query.CountAsync(cancellationToken);

        // Calculate pagination info
        int totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);
        int skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

        // Get paginated items
        var items = await query
            .Skip(skip)
            .Take(validatedPageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Select DTOs
        var dtoItems = items.Select(_selector).ToList();

        // Return paginated response
        return new PaginatedResponse<D>
        {
            Items = dtoItems,
            TotalCount = totalCount,
            PageNumber = validatedPageNumber,
            PageSize = validatedPageSize,
            TotalPages = totalPages,
            HasPreviousPage = PaginationHelper.HasPreviousPage(validatedPageNumber),
            HasNextPage = PaginationHelper.HasNextPage(validatedPageNumber, totalPages)
        };
    }
}

