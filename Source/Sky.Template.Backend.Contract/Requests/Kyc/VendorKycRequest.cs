using System.Collections.Generic;
using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.Kyc;

public class VendorKycRequest : BaseRequest
{
    public string LegalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public List<string> Documents { get; set; } = new();
}
