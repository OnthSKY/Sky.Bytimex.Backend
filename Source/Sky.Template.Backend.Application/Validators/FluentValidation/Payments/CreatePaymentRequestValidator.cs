using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Payments;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Payments;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(SharedResourceKeys.InvalidPrice);
        RuleFor(x => x.PaymentType)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(v => Enum.IsDefined(typeof(PaymentType), v))
            .WithMessage(SharedResourceKeys.InvalidPaymentType);
        RuleFor(x => x.PaymentStatus)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(v => Enum.IsDefined(typeof(PaymentStatus), v))
            .WithMessage(SharedResourceKeys.InvalidPaymentStatus);
        When(x => x.PaymentType == PaymentType.CRYPTO.ToString() && x.TxHash != null, () =>
        {
            RuleFor(x => x.TxHash!).Matches("^[0-9a-fA-F]{64}$").WithMessage(SharedResourceKeys.InvalidGuid);
        });
    }
}
