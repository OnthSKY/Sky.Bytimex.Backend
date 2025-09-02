using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Invoices;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Invoices;

public class UpdateInvoiceRequestValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.InvoiceDate).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.BuyerId).NotEmpty();
    }
}
