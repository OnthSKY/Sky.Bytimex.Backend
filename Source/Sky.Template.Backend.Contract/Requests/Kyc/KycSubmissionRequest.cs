namespace Sky.Template.Backend.Contract.Requests.Kyc;

using Microsoft.AspNetCore.Http;

public class KycSubmissionRequest : BaseRequest
{
    public Guid VerificationId { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? DocumentExpiryDate { get; set; }
    public IFormFile? Selfie { get; set; }
    public IFormFile? DocumentFront { get; set; }
    public IFormFile? DocumentBack { get; set; }
    public string? Reason { get; set; }
}
