using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Shipments;

public class UpdateShipmentRequestValidator : AbstractValidator<UpdateShipmentRequest>
{
    public UpdateShipmentRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Must(v => Enum.IsDefined(typeof(ShipmentStatus), v))
            .WithMessage(SharedResourceKeys.InvalidShipmentStatus);
    }
}
