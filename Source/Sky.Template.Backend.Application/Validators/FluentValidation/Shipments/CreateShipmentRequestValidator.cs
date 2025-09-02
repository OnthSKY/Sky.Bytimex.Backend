using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Shipments;

public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.ShipmentDate).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Carrier).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.TrackingNumber).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(v => Enum.IsDefined(typeof(ShipmentStatus), v))
            .WithMessage(SharedResourceKeys.InvalidShipmentStatus);
    }
}
