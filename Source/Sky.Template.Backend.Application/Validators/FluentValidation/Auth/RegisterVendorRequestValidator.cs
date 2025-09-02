using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Auth;

public class RegisterVendorRequestValidator : AbstractValidator<RegisterVendorRequest>
{
    public RegisterVendorRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(SharedResourceKeys.EmailIsRequired)
            .EmailAddress().WithMessage(SharedResourceKeys.InvalidEmail);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(SharedResourceKeys.PhoneIsRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(SharedResourceKeys.PasswordIsRequired)
            .MinimumLength(8).WithMessage(SharedResourceKeys.PasswordMinLength)
            .Matches("[A-Z]").WithMessage(SharedResourceKeys.PasswordUppercaseRequired)
            .Matches("[a-z]").WithMessage(SharedResourceKeys.PasswordLowercaseRequired)
            .Matches("[0-9]").WithMessage(SharedResourceKeys.PasswordNumberRequired)
            .Matches("[^a-zA-Z0-9]").WithMessage(SharedResourceKeys.PasswordSpecialCharRequired);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(SharedResourceKeys.FirstNameIsRequired)
            .MaximumLength(50).WithMessage(SharedResourceKeys.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(SharedResourceKeys.LastNameIsRequired)
            .MaximumLength(50).WithMessage(SharedResourceKeys.LastNameMaxLength);
    }
}

