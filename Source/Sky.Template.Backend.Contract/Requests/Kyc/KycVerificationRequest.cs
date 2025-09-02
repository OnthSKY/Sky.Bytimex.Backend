namespace Sky.Template.Backend.Contract.Requests.Kyc;

public class KycVerificationRequest : BaseRequest
{
    public string NationalId { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? DocumentExpiryDate { get; set; }
    public string? SelfieUrl { get; set; }
    public string? DocumentFrontUrl { get; set; }
    public string? DocumentBackUrl { get; set; }
}
