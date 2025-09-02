namespace Sky.Template.Backend.Contract.Responses.Kyc;

public class VendorKycStatusResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastUpdatedAt { get; set; }
    public string? RejectionReason { get; set; }
}
