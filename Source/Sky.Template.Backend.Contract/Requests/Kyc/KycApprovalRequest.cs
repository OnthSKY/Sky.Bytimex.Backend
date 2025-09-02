namespace Sky.Template.Backend.Contract.Requests.Kyc;

public class KycApprovalRequest : BaseRequest
{
    public Guid VerificationId { get; set; }
    public bool Approve { get; set; }
    public string? Reason { get; set; }
}
