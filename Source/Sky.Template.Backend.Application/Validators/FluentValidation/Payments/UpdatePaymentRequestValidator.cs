using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Payments;

public class UpdatePaymentRequestValidator : AbstractValidator<UpdatePaymentRequest>
{
    public UpdatePaymentRequestValidator()
    {
        RuleFor(x => x.PaymentStatus).NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(v => Enum.IsDefined(typeof(PaymentStatus), v))
            .WithMessage(SharedResourceKeys.InvalidPaymentStatus);
        RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue).WithMessage(SharedResourceKeys.InvalidPrice);
        When(x => !string.IsNullOrEmpty(x.PaymentType), () =>
        {
            RuleFor(x => x.PaymentType!)
                .Must(v => Enum.IsDefined(typeof(PaymentType), v))
                .WithMessage(SharedResourceKeys.InvalidPaymentType);
        });
        When(x => x.PaymentType == PaymentType.CRYPTO.ToString() && x.TxHash != null, () =>
        {
            RuleFor(x => x.TxHash!).Matches("^[0-9a-fA-F]{64}$").WithMessage(SharedResourceKeys.InvalidGuid);
        });
    }
}
