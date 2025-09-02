using System;
using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.Kyc;

public class VendorKycReviewRequest : BaseRequest
{
    public Guid VendorId { get; set; }
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

