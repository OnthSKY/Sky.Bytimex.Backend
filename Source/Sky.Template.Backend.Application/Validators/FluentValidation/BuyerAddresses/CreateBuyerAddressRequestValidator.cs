using FluentValidation;
using Sky.Template.Backend.Contract.Requests.BuyerAddresses;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.BuyerAddresses;

public class CreateBuyerAddressRequestValidator : AbstractValidator<CreateBuyerAddressRequest>
{
    public CreateBuyerAddressRequestValidator(IBuyerAddressRepository repository)
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.AddressTitle).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.FullAddress).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.City).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Country).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .Matches("^[0-9A-Za-z\\- ]+$").WithMessage(SharedResourceKeys.InvalidPostalCode);

        RuleFor(x => x).MustAsync(async (request, cancellation) =>
        {
            if (!request.IsDefault)
                return true;
            return !await repository.HasDefaultAsync(request.BuyerId);
        }).WithMessage(SharedResourceKeys.DefaultAddressAlreadyExistsForBuyer);
    }
}
