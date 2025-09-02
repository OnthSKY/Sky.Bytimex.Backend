namespace Sky.Template.Backend.Contract.Requests.Vendors;

public class CreateVendorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
