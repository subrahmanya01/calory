namespace Calory.Api.Features;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public static class Pagination
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        return (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
    }

    public static PagedResponse<T> Create<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        return new(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}