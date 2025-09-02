 
namespace Sky.Template.Backend.Core.Requests.Base;

public class GridRequest : Pagination
{
     public string SearchValue { get; set; } = "";
    public Dictionary<string, string> Filters { get; set; } = new();
    public string OrderColumn { get; set; } = "CreatedAt";
    private string _orderDirection = "DESC";  

    public string OrderDirection
    {
        get => _orderDirection.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";
        set => _orderDirection = value;
    }
}
