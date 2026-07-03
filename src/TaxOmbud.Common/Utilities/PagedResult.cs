namespace TaxOmbud.Common.Responses;

public class PagedResult<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = new();

    public PagedResult() { }

    public PagedResult(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        Items = items is List<T> list ? list : new List<T>(items);
    }
}
