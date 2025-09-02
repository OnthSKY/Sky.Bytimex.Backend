namespace Sky.Template.Backend.Contract.Responses.Kyc;

public class VendorKycDto
{
    public Guid VendorId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Decision { get; set; } = string.Empty;
}

