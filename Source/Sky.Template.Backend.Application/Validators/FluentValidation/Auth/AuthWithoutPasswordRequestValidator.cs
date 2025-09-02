using FluentValidation;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Contract.Requests.Auth;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Auth;

public class AuthWithoutPasswordRequestValidator : AbstractValidator<AuthWithoutPasswordRequest>
{
    public AuthWithoutPasswordRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(SharedResourceKeys.UsernameRequired);
    }
}
