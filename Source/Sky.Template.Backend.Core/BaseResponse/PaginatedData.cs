namespace Sky.Template.Backend.Core.BaseResponse;

public class PaginatedData<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public bool HasNextPage => Page < TotalPage;
    public bool HasPreviousPage => Page > 1;

}
