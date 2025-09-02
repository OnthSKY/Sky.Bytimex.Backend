namespace Sky.Template.Backend.Contract.Requests.Kyc;

using Sky.Template.Backend.Core.Enums;

public class SubmitVendorKycRequest : BaseRequest
{
    public Guid VendorId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

