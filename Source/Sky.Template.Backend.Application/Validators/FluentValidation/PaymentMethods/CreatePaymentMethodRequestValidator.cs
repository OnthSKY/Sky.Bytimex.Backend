using FluentValidation;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.PaymentMethods;

public class CreatePaymentMethodRequestValidator : AbstractValidator<CreatePaymentMethodRequest>
{
    public CreatePaymentMethodRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Code).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(v => Enum.IsDefined(typeof(PaymentMethodType), v))
            .WithMessage(SharedResourceKeys.InvalidPaymentMethodType);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => Enum.IsDefined(typeof(Status), v))
            .WithMessage(SharedResourceKeys.StatusMustBeActiveOrInactive);
        RuleFor(x => x.SupportedCurrency)
            .NotEmpty()
            .Matches("^[A-Z]{3}$")
            .WithMessage(SharedResourceKeys.InvalidCurrencyCode);
    }
}
