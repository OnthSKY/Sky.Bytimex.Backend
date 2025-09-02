using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Buyers;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Buyers;

public class CreateBuyerRequestValidator : AbstractValidator<CreateBuyerRequest>
{
    public CreateBuyerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
         
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .EmailAddress().WithMessage(SharedResourceKeys.InvalidEmail);
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(SharedResourceKeys.Required);
    }
}
