using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.PaymentMethods;

public class CreatePaymentMethodRequest : BaseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SupportedCurrency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "ACTIVE";
    public bool IsActive { get; set; } = true;
}
