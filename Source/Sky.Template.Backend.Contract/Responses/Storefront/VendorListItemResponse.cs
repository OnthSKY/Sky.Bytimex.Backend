namespace Sky.Template.Backend.Contract.Responses.Storefront;

public class VendorListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LogoUrl { get; set; }
    public decimal RatingAvg { get; set; }
    public int RatingCount { get; set; }
    public int ProductCount { get; set; }
}
