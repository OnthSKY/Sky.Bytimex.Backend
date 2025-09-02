using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.PaymentMethods;

public class PaymentMethodFilterRequest : GridRequest
{
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? SupportedCurrency { get; set; }
    public bool? IsActive { get; set; }
}
