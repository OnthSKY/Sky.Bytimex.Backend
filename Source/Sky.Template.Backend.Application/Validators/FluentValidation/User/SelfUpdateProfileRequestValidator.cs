using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.User;

public class SelfUpdateProfileRequestValidator : AbstractValidator<SelfUpdateProfileRequest>
{
    public SelfUpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(SharedResourceKeys.FirstNameIsRequired)
            .Length(2, 64).WithMessage(SharedResourceKeys.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(SharedResourceKeys.LastNameIsRequired)
            .Length(2, 64).WithMessage(SharedResourceKeys.LastNameMaxLength);

        RuleFor(x => x.Phone)
            .MaximumLength(32);
    }
}
