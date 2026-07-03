namespace InternMS.Api.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination
/// Centralizes pagination logic to prevent copy-paste code
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to an IQueryable sequence
    /// </summary>
    /// <typeparam name="T">Type of elements in the sequence</typeparam>
    /// <param name="query">The queryable sequence</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated IQueryable sequence</returns>
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
        where T : class
    {
        int skip = Utils.PaginationHelper.CalculateSkip(pageNumber, pageSize);
        return query.Skip(skip).Take(pageSize);
    }

    /// <summary>
    /// Applies pagination to a List sequence
    /// </summary>
    /// <typeparam name="T">Type of elements in the sequence</typeparam>
    /// <param name="list">The list sequence</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list</returns>
    public static List<T> Paginate<T>(
        this List<T> list,
        int pageNumber,
        int pageSize)
        where T : class
    {
        int skip = Utils.PaginationHelper.CalculateSkip(pageNumber, pageSize);
        return list.Skip(skip).Take(pageSize).ToList();
    }

    /// <summary>
    /// Applies pagination to an IEnumerable sequence
    /// </summary>
    /// <typeparam name="T">Type of elements in the sequence</typeparam>
    /// <param name="enumerable">The enumerable sequence</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated enumerable</returns>
    public static IEnumerable<T> Paginate<T>(
        this IEnumerable<T> enumerable,
        int pageNumber,
        int pageSize)
        where T : class
    {
        int skip = Utils.PaginationHelper.CalculateSkip(pageNumber, pageSize);
        return enumerable.Skip(skip).Take(pageSize);
    }
}
