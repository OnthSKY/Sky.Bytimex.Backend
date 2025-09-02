using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.PaymentMethods;

public class UpdatePaymentMethodRequest : BaseRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? SupportedCurrency { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
}
