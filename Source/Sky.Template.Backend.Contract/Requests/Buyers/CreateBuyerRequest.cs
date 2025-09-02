namespace Sky.Template.Backend.Contract.Requests.Buyers;

public class CreateBuyerRequest : BaseRequest
{
    public string BuyerType { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Description { get; set; }
}
