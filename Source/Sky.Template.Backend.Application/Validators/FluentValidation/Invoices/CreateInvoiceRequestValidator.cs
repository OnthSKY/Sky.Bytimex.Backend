using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Invoices;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Invoices;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.InvoiceDate).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.BuyerId).NotEmpty();
    }
}
