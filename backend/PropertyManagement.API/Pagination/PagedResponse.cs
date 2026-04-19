namespace PropertyManagement.API.Pagination;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public static class PaginationExtensions
{
    public static PagedResponse<T> ToPagedResponse<T>(this IReadOnlyList<T> items, int page, int pageSize)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var totalCount = items.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)normalizedPageSize);
        var pageItems = items
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        return new PagedResponse<T>(pageItems, normalizedPage, normalizedPageSize, totalCount, totalPages);
    }
}
