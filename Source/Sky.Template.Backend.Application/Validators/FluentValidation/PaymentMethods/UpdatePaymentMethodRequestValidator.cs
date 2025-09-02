using FluentValidation;
using Sky.Template.Backend.Contract.Requests.PaymentMethods;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.PaymentMethods;

public class UpdatePaymentMethodRequestValidator : AbstractValidator<UpdatePaymentMethodRequest>
{
    public UpdatePaymentMethodRequestValidator()
    {
        When(x => x.Name != null, () => RuleFor(x => x.Name!).NotEmpty().WithMessage(SharedResourceKeys.Required));
        When(x => x.Code != null, () => RuleFor(x => x.Code!).NotEmpty().WithMessage(SharedResourceKeys.Required));
        When(x => x.Type != null, () =>
        {
            RuleFor(x => x.Type!)
                .Must(v => Enum.IsDefined(typeof(PaymentMethodType), v))
                .WithMessage(SharedResourceKeys.InvalidPaymentMethodType);
        });
        When(x => x.Status != null, () =>
        {
            RuleFor(x => x.Status!)
                .Must(v => Enum.IsDefined(typeof(Status), v))
                .WithMessage(SharedResourceKeys.StatusMustBeActiveOrInactive);
        });
        When(x => x.SupportedCurrency != null, () =>
        {
            RuleFor(x => x.SupportedCurrency!)
                .Matches("^[A-Z]{3}$")
                .WithMessage(SharedResourceKeys.InvalidCurrencyCode);
        });
    }
}
