using FluentValidation;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Contract.Requests.Auth;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Auth;

public class AuthRequestValidator : AbstractValidator<AuthRequest>
{
    public AuthRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(SharedResourceKeys.UsernameRequired);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(SharedResourceKeys.PasswordIsRequired);
    }
}
