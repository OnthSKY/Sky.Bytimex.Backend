namespace Sky.Template.Backend.Contract.Responses.VendorResponses;

public class VerificationStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
