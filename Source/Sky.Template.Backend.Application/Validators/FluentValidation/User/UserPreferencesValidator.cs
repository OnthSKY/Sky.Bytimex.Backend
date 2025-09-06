using FluentValidation;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using System.Linq;

using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.User;

public class UserPreferencesValidator : AbstractValidator<UserPreferencesDto>
{
    private static readonly string[] AllowedThemes = ["light", "dark", "system"];

    public UserPreferencesValidator()
    {
        RuleFor(x => x.Language).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Currency).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.Theme).Must(t => AllowedThemes.Contains(t))
            .WithMessage(SharedResourceKeys.Required);
    }
}
