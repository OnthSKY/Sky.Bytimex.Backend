namespace Sky.Template.Backend.Contract.Responses.Kyc;

public class KycStatusResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? DocumentExpiryDate { get; set; }
    public string? SelfieUrl { get; set; }
    public string? DocumentFrontUrl { get; set; }
    public string? DocumentBackUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}
