namespace Sky.Template.Backend.Core.Requests.Base;

public class Pagination
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
